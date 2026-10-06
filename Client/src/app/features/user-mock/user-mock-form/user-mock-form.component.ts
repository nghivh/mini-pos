import { CommonModule } from '@angular/common';
import { Component, effect, EventEmitter, inject, input, Input, OnChanges, output, Output, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { User } from '@core/models/user.model';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { CheckboxComponent } from '@shared/ui/checkbox/checkbox.component';
import { InputComponent } from '@shared/ui/input/input.component';
import { SelectSearchComponent } from '@shared/ui/select-search/select-search.component';

@Component({
  selector: 'app-user-mock-form',
  imports: [
    CommonModule, ReactiveFormsModule,
    InputComponent, ButtonComponent, SelectSearchComponent, CheckboxComponent
  ],
  templateUrl: './user-mock-form.component.html',
  styleUrl: './user-mock-form.component.scss',
})
export class UserMockFormComponent {
// --- 1. SIGNAL INPUTS (Thay thế @Input())---
  initialData = input<User | null>(null);
  loading = input<boolean>(false);

  // --- 2. SIGNAL OUTPUTS (Thay thế @Output() + EventEmitter)---
  save = output<any>();
  cancel = output<void>();

  form;

  roleOptions = [
    {label: 'Administrator', value: 'Admin'},
    {label: 'Editor', value: 'Editor'},
    {label: 'Viewer', value: 'Viewer'}
  ]

  fb = inject(FormBuilder);

  constructor(){
    // Khởi tạo Form rỗng ngay lập tức
    this.form = this.fb.group({
      name: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      role: [null as string | null, Validators.required],
      status: [true]
    })

    // --- 3. REPLACEMENT FOR NG_ON_CHANGES ---
    // effect() tự động chạy mỗi khi signal 'initialData' thay đổi
    effect(() => {
      // Đọc giá trị signal (Dependency tracking)
      const user = this.initialData();

      if(user){
        // === EDIT MODE ===
        this.form.patchValue({
          name: user.name,
          email: user.email,
          role: user.role,
          status: user.status === 'Active'
        });
        this.form.get('email')?.disable();
      }
      else{
        // === CREATE MODE ===
        this.form.reset({ status: true});
        this.form.get('email')?.enable();
      }
    });
  }  

  onSubmit(){
    if(this.form.invalid){
      this.form.markAllAsTouched(); // Hiển thị đỏ tất cả lỗi cho user thấy
      return;
    }
    // Lấy giá trị (kể cả field bị disable) và bắn ra ngoài
    console.log('User-Form', this.form.getRawValue());
    this.save.emit(this.form.getRawValue());
  }

  onCancel(){
    this.cancel.emit();
  }
}
