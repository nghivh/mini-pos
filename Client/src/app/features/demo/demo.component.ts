import { Component, AfterViewInit, ViewChild, TemplateRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { of, delay, Observable } from 'rxjs';

// Import các UI Components
import { BadgeComponent } from '@shared/ui/badge/badge.component';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { CheckboxComponent } from '@shared/ui/checkbox/checkbox.component';
import { InputComponent } from '@shared/ui/input/input.component';
import { SelectComponent } from '@shared/ui/select/select.component';
// Import Select Search mới
import { SelectSearchComponent, SelectOption } from '@shared/ui/select-search/select-search.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { PageEvent, TableColumn } from '@shared/ui/table/table.types';
import { ModalComponent } from '@shared/ui/modal/modal.component';

@Component({
  selector: 'app-demo',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule, 
    ReactiveFormsModule,
    InputComponent, 
    ButtonComponent, 
    SelectComponent, 
    CheckboxComponent, 
    BadgeComponent,
    SelectSearchComponent
  ],
  templateUrl: './demo.component.html',
  styleUrl: './demo.component.scss',
})
export class DemoComponent {
  isPrimaryLoading = false;

  // 1. DATA CHO VIRTUAL SCROLL (1000 items)
  // Tạo giả 1000 quốc gia
  countries: SelectOption[] = Array.from({ length: 1000 }, (_, i) => ({
    label: `Country Name ${i + 1}`,
    value: `country-${i + 1}`
  }));

  // 2. DATA CHO ASYNC SEARCH
  // Danh sách user gốc (để giả lập DB)
  private allUsers = Array.from({ length: 50 }, (_, i) => ({
    id: i + 1,
    name: `User ${i + 1}`,
    email: `user${i + 1}@example.com`,
    avatar: `https://ui-avatars.com/api/?name=User+${i+1}&background=random`
  }));

  // Hàm Search Async (Giả lập gọi API delay 1s)
  // Lưu ý: Dùng arrow function để giữ 'this' nếu cần
  searchUsersFn = (term: string): Observable<SelectOption[]> => {
    console.log('Searching for:', term);
    
    // Filter logic giả lập backend
    const lowerTerm = term.toLowerCase();
    const filtered = this.allUsers.filter(u => 
      u.name.toLowerCase().includes(lowerTerm) || 
      u.email.toLowerCase().includes(lowerTerm)
    );

    // Map sang format SelectOption
    const results: SelectOption[] = filtered.map(u => ({
      label: u.name,
      value: u // Value là nguyên object User
    }));

    // Return Observable + Delay 1s để thấy loading spinner
    return of(results).pipe(delay(800));
  };

  onPrimaryClick() {
    if (this.isPrimaryLoading) return;
    this.isPrimaryLoading = true;
    setTimeout(() => {
      this.isPrimaryLoading = false;
    }, 3000);
  }

  // --- VIEW CHILDS (Lấy template từ HTML) ---
  @ViewChild('userTpl') userTpl!: TemplateRef<any>;
  @ViewChild('statusTpl') statusTpl!: TemplateRef<any>;
  @ViewChild('actionTpl') actionTpl!: TemplateRef<any>;

  // --- DATA ---
  // Mock 50 users
  private allUsersTable = Array.from({ length: 50 }, (_, i) => ({
    id: i + 1,
    name: `User ${i + 1}`,
    email: `user${i + 1}@company.com`,
    role: i % 3 === 0 ? 'Admin' : 'Member',
    status: i % 4 === 0 ? 'Inactive' : 'Active',
    lastLogin: '2023-12-20'
  }));

  // Mock Country Options cho Select Search
  countryOptions = [
    { label: 'Vietnam', value: 'vn' },
    { label: 'United States', value: 'us' },
    { label: 'Japan', value: 'jp' },
    { label: 'Singapore', value: 'sg' },
  ];
}