import { Component, effect, inject, signal, TemplateRef, viewChild } from '@angular/core';
import { Customer } from '@core/models/customer.model';
import { CustomerService } from '@core/services/customer.service';
import { ToastService } from '@core/services/toast.service';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { InputComponent } from '@shared/ui/input/input.component';
import { ModalComponent } from '@shared/ui/modal/modal.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { TableColumn } from '@shared/ui/table/table.types';
import { finalize } from 'rxjs';
import { CustomerFormComponent } from '../customer-form/customer-form.component';

@Component({
  selector: 'app-customer-list',
  imports: [
    ButtonComponent,
    TableComponent,
    ModalComponent,
    CustomerFormComponent
  ],
  templateUrl: './customer-list.component.html',
  styleUrl: './customer-list.component.scss',
})
export class CustomerListComponent {
  // Service
  customerService = inject(CustomerService);
  toast = inject(ToastService);

  // Signal
  isLoading = signal(false);

  customers = signal<Customer[]>([]);
  filteredCustomers = signal<Customer[]>([]);
  columns = signal<TableColumn<Customer>[]>([]);

  selectedCustomer = signal<Customer | null>(null);
  selectedCustomers = signal<Customer[]>([]);

  currentPage = signal(1);

  // Modal
  isOpenModal = signal(false);

  actionTemplate = viewChild.required<TemplateRef<any>>('actionTemplate');

  constructor() {
    effect(() => {
      // Khi các template đã sẵn sàng (signal có giá trị)
      let actionTemp = this.actionTemplate();

      // Cập nhật lại columns config
      this.columns.set([
        { key: '', header: 'Action', width: '50px', template: actionTemp },
        { key: 'id', header: 'ID' },
        { key: 'fullName', header: 'Họ và tên' },
        { key: 'phoneNumber', header: 'Số điện thoại' },
        { key: 'loyaltyPoints', header: 'Điểm tích lũy' },
        { key: 'createdAt', header: 'Ngày tạo', type: 'date', format: 'yyyy-MM-dd HH:mm:ss' }
      ])
    })

    this.loadAllCustomer();
  }

  loadAllCustomer() {
    this.isLoading.set(true);

    this.customerService.getAllCustomers()
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.customers.set(res);
          this.filteredCustomers.set(res);
        },
        error: (err) => {
          this.toast.error(err.error?.message || err.message);
        }
      })
  }

  onSearch(term: string) {
    const searchTerm = term.toLocaleLowerCase().trim();

    this.currentPage.set(1);

    if (!searchTerm) {
      // Nếu xóa hết ô search -> Trả lại danh sách gốc
      this.filteredCustomers.set(this.customers());
      return;
    }

    // Logic lọc
    const results = this.customers().filter(item =>
      item.fullName.toLocaleLowerCase().includes(searchTerm) ||
      item.phoneNumber.toLocaleLowerCase().includes(searchTerm)
    )

    this.filteredCustomers.set(results);
  }

  onCreate() {
    this.selectedCustomer.set(null);
    this.isOpenModal.set(true);
  }

  onEdit(customer: any) {
    this.selectedCustomer.set(customer);
    this.isOpenModal.set(true);
  }

  onDelete(customer: any) {
    this.toast.error('Chưa triển khai...');
  }

  onCloseModal() {
    this.isOpenModal.set(false);
    this.selectedCustomer.set(null);
  }

  onHandleSave(data: any) {
    this.isLoading.set(true);

    const payload = {
      ...data
    }

    if (this.selectedCustomer()) {
      // Update
      const id = this.selectedCustomer()!.id;
      this.customerService.updateCustomer(id, payload)
        .pipe(
          finalize(() => this.isLoading.set(false))
        )
        .subscribe({
          next: (res) => {
            this.toast.success("Cập nhật thành công");
            this.onCloseModal();
            this.loadAllCustomer();
          },
          error: (err) => {
            this.toast.error(err.error?.message || err.message);
          }
        })
    }
    else {
      // Insert
      this.customerService.createCustomer(payload)
        .pipe(
          finalize(() => this.isLoading.set(false))
        )
        .subscribe({
          next: (res) => {
            this.toast.success("Thêm mới thành công");
            this.onCloseModal();
            this.loadAllCustomer();
          },
          error: (err) => {
            this.toast.error(err.error?.message || err.message);
          }
        })
    }
  }
}
