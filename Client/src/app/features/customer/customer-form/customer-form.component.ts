import { CommonModule } from '@angular/common';
import { Component, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Customer } from '@core/models/customer.model';
import { InputComponent } from '@shared/ui/input/input.component';
import { ButtonComponent } from "@shared/ui/button/button.component";

@Component({
  selector: 'app-customer-form',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    InputComponent,
    ButtonComponent
],
  templateUrl: './customer-form.component.html',
  styleUrl: './customer-form.component.scss',
})
export class CustomerFormComponent {
  fb = inject(FormBuilder);
  isEditMode = signal(false);
  loading = input<boolean>(false);

  // Input
  initialData = input<Customer | null>(null);

  // Ouput
  save = output<any>();
  cancel = output<void>();

  customerForm : FormGroup = this.fb.group({
    id: [0],
    fullName: ['', [Validators.required, Validators.maxLength(200)]],
    phoneNumber: ['', [Validators.required, Validators.pattern('^[0-9]{10,11}$')]],
    loyaltyPoints: [0]
  })

  constructor(){
    effect(() => {
      // Đọc giá trị từ signal (Dependency tracking)
      const customer = this.initialData();

      if(customer){
        // Edit mode
        this.isEditMode.set(true);
        this.customerForm.patchValue({
          id: customer.id,
          fullName: customer.fullName,
          phoneNumber: customer.phoneNumber,
          loyaltyPoints: customer.loyaltyPoints
        })

        // Enable/disable fields
        this.customerForm.get('id')?.disable();
        this.customerForm.get('loyaltyPoints')?.enable();
      }
      else{
        // Insert mode
        this.isEditMode.set(false);
        this.customerForm.reset({
          id: 0,
          loyaltyPoints: 0
        });

        // Enable/disable fields
        this.customerForm.get('id')?.disable();
        this.customerForm.get('loyaltyPoints')?.disable();
      }
    })
  }
  
  onSave(){
    if(this.customerForm.invalid){
      this.customerForm.markAllAsTouched();
      return;
    }

    console.log('Customer-Form', this.customerForm.getRawValue());
    this.save.emit(this.customerForm.getRawValue());
  }

  onCancel(){
    this.cancel.emit();
  }
}
