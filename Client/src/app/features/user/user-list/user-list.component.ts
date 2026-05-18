import { CommonModule } from '@angular/common';
import { Component, effect, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { ModalComponent } from '@shared/ui/modal/modal.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { UserFormComponent } from '../user-form/user-form.component';
import { User } from '@core/models/user.model';
import { TableColumn } from '@shared/ui/table/table.types';
import { UserService } from '@core/services/user.service';

@Component({
  selector: 'app-user-list',
  imports: [
    CommonModule,
    TableComponent,
    ButtonComponent,
    ModalComponent,
    BadgeComponent,
    UserFormComponent
  ],
  templateUrl: './user-list.component.html',
  styleUrl: './user-list.component.scss',
})
export class UserListComponent {
  // --- 1. STATE SIGNALS ---
  users = signal<User[]>([]);
  filteredUsers = signal<User[]>([]);
  loading = signal(false);
  currentPage = signal(1);

  // Modal State
  isModalOpen = signal(false);
  submitting = signal(false);
  selectedUser = signal<User | null>(null);
  selectedUsers = signal<User[]>([]);

  // --- 2. VIEW QUERIES (NEW SIGNAL API) ---
  // Thay thế @ViewChild. 'required' nghĩa là template này bắt buộc phải có trong HTML.
  userTemplate = viewChild.required<TemplateRef<any>>('userTemplate');
  statusTemplate = viewChild.required<TemplateRef<any>>('statusTemplate');
  actionTemplate = viewChild.required<TemplateRef<any>>('actionTemplate');

  // --- 3. COLUMNS CONFIGURATION ---
  // Khởi tạo columns là một signal rỗng hoặc mặc định
  columns = signal<TableColumn<User>[]>([]);

  private userService = inject(UserService);

  constructor() {
    this.loadData();

    // --- 4. EFFECT (Thay thế ngAfterViewInit + setTimeout) ---
    // effect sẽ tự động chạy khi viewChild tìm thấy template
    effect(() => {
      // Khi các template đã sẵn sàng (signal có giá trị)
      const uTmp = this.userTemplate();
      const sTmp = this.statusTemplate();
      const aTmp = this.actionTemplate();

      // Cập nhật lại columns config
      this.columns.set([
        { key: 'id', header: 'ID', width: 'w-16' },
        { key: 'info', header: 'User Info', template: uTmp },
        { key: 'email', header: 'Email' },
        { key: 'role', header: 'Role', sortable: true },
        { key: 'status', header: 'Status', template: sTmp },
        { key: 'actions', header: 'Actions', width: 'w-16', template: aTmp }
      ]);
    });
  }

  // --- DATA LOGIC ---
  loadData() {
    this.loading.set(true);
    this.userService.getUsers().subscribe(data => {
      this.users.set(data);
      this.filteredUsers.set(data);
      this.loading.set(false);
    });
  }

  onPageChange(event: any) {
    // Kiểm tra xem event là object {page: 2} hay số 2 để xử lý
    const pageNumber = event.page || event;
    this.currentPage.set(pageNumber);
  }

  onSelectionChange(selected: any[]){
    this.selectedUsers.set(selected);
  }

  onSearch(term: string) {
    const searchTerm = term.toLowerCase().trim();

    this.currentPage.set(1);

    if (!searchTerm) {
      // Nếu xóa hết ô search -> Trả lại danh sách gốc
      this.filteredUsers.set(this.users());
      return;
    }

    // Logic lọc (Tìm theo tên hoặc email)
    const result = this.users().filter(user =>
      user.name.toLowerCase().includes(searchTerm) ||
      user.email.toLowerCase().includes(searchTerm)
    );

    this.filteredUsers.set(result);
  }

  // --- DATA LOGIC ---
  // Mở Modal thêm mới
  onOpenModal() {
    this.selectedUser.set(null);
    this.isModalOpen.set(true);
  }

  // Mở Modal chỉnh sửa
  onEdit(user: User) {
    this.selectedUser.set(user);
    this.isModalOpen.set(true);
  }

  // Đóng Modal
  closeModal() {
    this.isModalOpen.set(false);
    this.selectedUser.set(null);
  }

  // Xử lý Save (Create hoặc Update)
  handleSave(formData: any) {
    this.submitting.set(true);

    const payload = {
      ...formData,
      // Đảm bảo status luôn có giá trị
      status: formData.status ? 'Active' : 'Inactive'
    };

    let request$;

    if (this.selectedUser()) {
      // === GỌI UPDATE ===
      const id = this.selectedUser()!.id;
      request$ = this.userService.updateUser(id, payload);
    } else {
      // === GỌI CREATE ===
      request$ = this.userService.createUser(payload);
    }

    request$.subscribe({
      next: (res) => {
        this.submitting.set(false);
        alert(this.selectedUser() ? 'User updated!' : 'User created!');
        this.closeModal();        
        this.loadData(); // Load lại để thấy dữ liệu mới        
      },
      error: (err) => {
        console.error(err);
        this.submitting.set(false);
        alert('An error occurred');
      }
    });
  }

  // Xử lý Delete
  onDelete(user: User) {
    if (confirm(`Are you sure you want to delete ${user.name}`)) {
      this.loading.set(true);

      this.userService.deleteUser(user.id).subscribe({
        next: () => {
          this.loadData(); // Load lại bảng
          // this.loading.set(false); // loadData đã tự xử lý tắt loading
        },
        error: (err) => {
          console.error('Delete failed', err);
          this.loading.set(false);
        }
      })
    }
  }
}
