import { CommonModule } from '@angular/common';
import { Component, effect, inject, OnInit, signal, TemplateRef, viewChild } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { Category } from '@core/models/category.model';
import { PagedResult } from '@core/models/paged-result.model';
import { ProductQueryRequest, ProductResponse } from '@core/models/product.model';
import { CategoryService } from '@core/services/category.service';
import { ProductService } from '@core/services/product.service';
import { ToastService } from '@core/services/toast.service';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { TableComponent } from '@shared/ui/table/table.component';
import { PageEvent, TableColumn } from '@shared/ui/table/table.types';
import { finalize } from 'rxjs';
import { ModalComponent } from "@shared/ui/modal/modal.component";
import { ProductFormComponent } from "../product-form/product-form.component";

@Component({
  selector: 'app-product-list',
  imports: [
    CommonModule, ReactiveFormsModule,
    ButtonComponent, TableComponent,
    ModalComponent,
    ProductFormComponent
],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent implements OnInit {
  productService = inject(ProductService);
  categoryService = inject(CategoryService);
  toast = inject(ToastService);

  // Signals quản lý State
  products = signal<ProductResponse[]>([]);
  categories = signal<Category[]>([]);
  totalCount = signal(0);
  isLoading = signal(false);
  showModal = signal(false);
  selectedProduct = signal<ProductResponse | null>(null);

  // Query state
  query = signal<ProductQueryRequest>({
    page: 1,
    pageSize: 10,
    sortBy: 'Id',
    isDescending: false,
    search: ''
  });

  // Cấu hình các cột cho TableComponent
  columns = signal<TableColumn<ProductResponse>[]>([]);

  actionTemplate = viewChild.required<TemplateRef<any>>('actionTemplate');

  searchTimer: any; // Biến lưu trữ timer

  ngOnInit(): void {
    this.loadCategories();
    this.fetchProducts();
  }

  constructor() {
    effect(() => {
      // Khi các template đã sẵn sàng (signal có giá trị)
      let actionTemp = this.actionTemplate();

      // Cập nhật lại columns config
      this.columns.set([
        { key: 'actions', header: 'Actions', template: actionTemp, width: '50px' },
        { key: 'id', header: 'Mã Sản phẩm' },
        { key: 'productName', header: 'Sản phẩm', sortable: true, className: 'font-medium' },
        { key: 'categoryName', header: 'Danh mục', sortable: false },
        { key: 'barcode', header: 'Mã vạch', sortable: true },
        { key: 'price', header: 'Giá bán', type: 'currency', format: 'VND', sortable: true },
        { key: 'stockQuantity', header: 'Tồn kho', type: 'number', sortable: true },
        { key: 'actions', header: '', sortable: false, width: '100px' }
      ])
    })
  }

  loadCategories() {
    this.categoryService.getAllCategories()
      .subscribe({
        next: (res) => {
          this.categories.set(res);
        }
      })
  }

  fetchProducts() {
    this.isLoading.set(true);
    this.productService.getAdvanced(this.query())
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res: PagedResult<ProductResponse>) => {
          console.log(res);
          this.products.set(res.items);
          this.totalCount.set(res.totalCount);

          this.query.update(q => ({ ...q, page: q.page }));
        },
        error: (err) => {
          console.error('Lỗi khi tải sản phẩm:', err);
        }
      })
  }

  onPageChange(event: PageEvent) {
    this.query.update(q => ({ ...q, page: event.page }));
    this.fetchProducts();
  }

  onSortChange(event: { key: string, direction: 'asc' | 'desc' }) {
    this.query.update(q => ({
      ...q,
      sortBy: event.key,
      isDescending: event.direction === 'desc'
    }));
    this.fetchProducts();
  }

  onSearch(term: string) {
    // 1. Nếu đang có một timer chờ, hãy xóa nó đi (Hủy lệnh gọi API cũ)
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    // 2. Thiết lập một timer mới
    this.searchTimer = setTimeout(() => {
      const searchTerm = term.trim();

      // 3. Cập nhật query và gọi API
      this.query.update(q => ({
        ...q,
        search: searchTerm,
        page: 1
      }));

      this.fetchProducts();

      console.log('Đang gọi API search với từ khóa:', searchTerm);
    }, 1000); // Đợi 1000ms sau khi người dùng ngừng gõ
  }

  onExportAllProducts() {

  }

  onCreate() {
    this.selectedProduct.set(null);
    this.showModal.set(true);
  }

  onEdit(product: any) {
    this.selectedProduct.set(product);
    this.showModal.set(true);
  }

  onDelete(product: any) {
    let confirm = prompt('Bạn có chắc chắn muốn xóa record này?');
    if(!confirm){
      return;
    }

    this.isLoading.set(true);

    this.productService.delete(product.id)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.toast.success('Xóa sản phẩm thành công');
          this.fetchProducts();
        },
        error: (err) => {
          this.toast.error(err.error?.message || err.message);
        }
      })
  }

  onHandleSave(formData: any){
    this.isLoading.set(true);

    const payload = {
      ...formData
    };

    // Quyết định request nào sẽ được chạy
    const request$ = this.selectedProduct()
      ? this.productService.update(this.selectedProduct()!.id, payload)
      : this.productService.create(payload);

    // Xử lý chung tại một chỗ
    request$.pipe(
      finalize(() => this.isLoading.set(false))
    ).subscribe({
      next: (res) => {
        console.log('Save product success:', res);
        this.toast.success('Lưu sản phẩm thành công');
        this.onCloseModal();
        this.fetchProducts();
      },
      error: (err) => {
        console.error('Lỗi khi lưu sản phẩm:', err);
        this.toast.error(err.error?.message || err.message);
      }
    });
  }

  onCloseModal(){
    this.selectedProduct.set(null);
    this.showModal.set(false);
  }
}
