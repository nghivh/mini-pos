import { CommonModule } from '@angular/common';
import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Category } from '@core/models/category.model';
import { DropDownListDto } from '@core/models/dropdownlist.model';
import { ProductResponse } from '@core/models/product.model';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { InputComponent } from '@shared/ui/input/input.component';
import { SelectSearchComponent } from '@shared/ui/select-search/select-search.component';

@Component({
  selector: 'app-product-form',
  imports: [
    CommonModule, ReactiveFormsModule,
    InputComponent, ButtonComponent, SelectSearchComponent
  ],
  templateUrl: './product-form.component.html',
  styleUrl: './product-form.component.scss',
})
export class ProductFormComponent {
  fb = inject(FormBuilder);

  categories = input<Category[]>([]); // Danh sách Categories truyền từ form cha vào
  initialData = input<ProductResponse | null>(null);
  loading = input<boolean>(false);

  save = output<any>();
  cancel = output<void>();

  productForm: FormGroup = this.fb.group({
    id: [0],
    productName: ['', [Validators.required]],
    barcode: ['', [Validators.required]],
    price: [0, [Validators.min(0.1)]],
    costPrice: [0, [Validators.min(0.1)]],
    stockQuantity: [0, [Validators.min(1)]],
    categoryId: [null, [Validators.required]]
  });

  categoryOptions = computed<DropDownListDto[]>(() =>
    this.categories().map(cat => ({
      value: cat.id.toString(), // value thường là string để dễ handle
      label: cat.name           // hoặc cat.categoryName tùy theo model của bạn
    }))
  );

  constructor() {
    effect(() => {
      const product = this.initialData();

      if (product) {
        // Update mode
        this.productForm.patchValue({
          ...product,
          categoryId: product.categoryId.toString() //Chuyển sang string để bind đúng Category trên UI
        });

        this.productForm.get('id')?.disable();
      }
      else {
        // Insert mode
        this.productForm.reset({
          id: 0
        });

        this.productForm.get('id')?.disable();
      }
    })
  }

  onSubmit() {
    if (this.productForm.invalid) {
      this.productForm.markAllAsTouched();
      return;
    }

    console.log('Product-Form', this.productForm.getRawValue());
    this.save.emit(this.productForm.getRawValue());
  }

  onCancel() {
    this.cancel.emit();
  }
}
