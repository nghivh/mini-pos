import { Component, inject, OnInit, signal, HostListener, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ProductService } from '@core/services/product.service';
import { CategoryService } from '@core/services/category.service';
import { CartService } from '@core/services/cart.service';
import { OrderService } from '@core/services/order.service'; // Bạn cần tạo Service này gọi tới SalesController
import { ProductQueryRequest, ProductResponse } from '@core/models/product.model';
import { Category } from '@core/models/category.model';
import { debounceTime, distinctUntilChanged, finalize, Subject } from 'rxjs';
import { CheckoutRequest } from '@core/models/order.model';
import { ToastService } from '@core/services/toast.service';
import { CustomerService } from '@core/services/customer.service';
import { Customer } from '@core/models/customer.model';
import { ModalComponent } from '@shared/ui/modal/modal.component';
import { CustomerFormComponent } from "@features/customer/customer-form/customer-form.component";

@Component({
  selector: 'app-order-create',
  standalone: true,
  imports: [CommonModule, FormsModule, ModalComponent, CustomerFormComponent],
  templateUrl: './order-create.component.html',
  styleUrls: ['./order-create.component.scss']
})
export class OrderCreateComponent implements OnInit {
  // Inject services
  public cart = inject(CartService);
  private productService = inject(ProductService);
  private categoryService = inject(CategoryService);
  private orderService = inject(OrderService);
  private customerService = inject(CustomerService);
  private toast = inject(ToastService);

  // Signals
  products = signal<ProductResponse[]>([]);
  categories = signal<Category[]>([]);
  selectedCategoryId = signal<number | null>(null);
  searchQuery = signal('');
  isLoading = signal(false);

  // Customer
  customers = signal<Customer[]>([]);
  selectedCustomer = signal<Customer | null>(null);
  customerSearchResults = signal<Customer[]>([]);
  isOpenModalCustomer = signal(false);

  // Các biến dùng cho việc in hóa đơn
  orderIdToPrint = '';
  printedCartItems: any[] = [];
  printedTotalAmount = 0;
  printedDiscount = 0;
  printedFinalAmount = 0;
  printDate: Date = new Date();

  // Tạo Mã Đơn hàng tạm
  generateTempId(): string {
    // Tạo ra một chuỗi ngẫu nhiên 6 ký tự viết hoa
    return Math.random().toString(36).substring(2, 8).toUpperCase();
  }

  // Xử lý thanh toán qua VietQR
  // 1. Cấu hình ngân hàng thụ hưởng của cửa hàng
  bankConfig = {
    bankId: 'MB',
    accountNo: '0123456789',
    accountName: 'NGUYEN VAN A'
  };

  // 2. Trạng thái phương thức thanh toán
  paymentMethod = signal<'Cash' | 'Transfer'>('Cash');

  // 3. Tự động sinh Link ảnh QR (Tự động cập nhật khi finalAmount thay đổi)
  qrCodeUrl = computed(() => {
    if (this.paymentMethod() !== 'Transfer' || this.cart.finalAmount() === 0) {
      return null;
    }
    
    const amount = this.cart.finalAmount();
    // Tạo nội dung CK có chứa mã đơn tạm thời
    const memo = `Thanh toan don hang ${this.generateTempId()}`; 
    
    // Sử dụng template 'compact2' của VietQR để tối ưu in đen trắng trên máy in nhiệt
    const baseUrl = `https://img.vietqr.io/image/${this.bankConfig.bankId}-${this.bankConfig.accountNo}-compact2.png`;
    
    return `${baseUrl}?amount=${amount}&addInfo=${encodeURIComponent(memo)}&accountName=${encodeURIComponent(this.bankConfig.accountName)}`;
  });

  // Xử lý tìm kiếm Debounce
  private searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject.pipe(
      debounceTime(400),
      distinctUntilChanged()
    ).subscribe(term => {
      this.searchQuery.set(term);
      this.loadProducts();
    });
  }

  ngOnInit() {
    this.loadInitialData();
  }

  loadInitialData() {
    this.categoryService.getAllCategories().subscribe(res => this.categories.set(res));
    this.loadCustomers();
    this.loadProducts();
  }

  loadProducts() {
    this.isLoading.set(true);
    const params: ProductQueryRequest = {
      page: 1,
      pageSize: 100,
      search: this.searchQuery(),
      categoryId: this.selectedCategoryId(),
      isDescending: true
    };
    this.productService.getAdvanced(params).subscribe(res => {
      this.products.set(res.items);
      this.isLoading.set(false);
    });
  }

  loadCustomers() {
    this.isLoading.set(true);
    this.customerService.getAllCustomers()
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.customers.set(res);
        },
        error: (err) => {
          this.customers.set([]);
        }
      })
  }

  onSearch(event: any) {
    this.searchSubject.next(event.target.value);
  }

  filterByCategory(id: number | null) {
    this.selectedCategoryId.set(id);
    this.loadProducts();
  }

  // Phím tắt bàn phím
  @HostListener('window:keydown.f12', ['$event'])
  handleF12(event: any) {
    event.preventDefault();
    this.onCheckout();
  }

  onCheckout() {
    if (this.cart.items().length === 0) return;

    const checkoutData: CheckoutRequest = {
      customerId: this.selectedCustomer()?.id,
      discountAmount: this.cart.discount(),
      paymentMethod: this.paymentMethod(),
      notes: this.paymentMethod() === 'Transfer' ? 'Chuyển khoản VietQR' : 'Giao dịch tại quầy',
      orderDetails: this.cart.items().map(i => ({
        productId: i.productId,
        quantity: i.quantity,
        unitPrice: i.price
      }))
    };

    this.orderService.checkout(checkoutData).subscribe({
      next: (res) => {
        //console.log(res); 
        this.toast.success(`Thanh toán thành công! Hóa đơn: ${res.orderId}`);       

        // 1. Chụp lại dữ liệu giỏ hàng hiện tại để đưa vào hóa đơn
        this.orderIdToPrint = res.orderId || 'HD' + Date.now().toString().slice(-6); // Giả lập ID nếu API chưa trả về
        this.printedCartItems = this.cart.items().map(item => ({ ...item })); // Deep copy mảng
        this.printedTotalAmount = this.cart.totalAmount();
        this.printedDiscount = this.cart.discount();
        this.printedFinalAmount = this.cart.finalAmount();
        this.printDate = new Date();

        // 2. Dọn dẹp màn hình bán hàng
        this.cart.clearCart();
        this.selectedCustomer?.set(null);
        this.loadProducts();

        // 3. Đợi Angular cập nhật UI phần hóa đơn (100ms) rồi mới gọi lệnh In
        setTimeout(() => {
          window.print();
        }, 500);        
      },
      error: (err) => this.toast.error(err.error?.message || 'Có lỗi xảy ra trong quá trình Thanh toán!')
    });
  }

  onSearchCustomer(event: any) {
    const term = event.target.value;

    if (term.length === 0) {
      this.customerSearchResults.set([]); // Ẩn danh sách tìm kiếm
      return;
    }

    const searchResults = this.customers().filter(c =>
      c.fullName.toLocaleLowerCase().includes(term) ||
      c.phoneNumber.toLocaleLowerCase().includes(term)
    );
    this.customerSearchResults.set(searchResults);
  }

  selectCustomer(customer: Customer) {
    this.selectedCustomer.set(customer);
    this.customerSearchResults.set([]); // Ẩn danh sách tìm kiếm
  }

  removeCustomer() {
    this.selectedCustomer.set(null);
  }

  onCreateCustomer() {
    this.selectedCustomer.set(null);
    this.isOpenModalCustomer.set(true);
  }

  onCloseModalCustomer() {
    this.isOpenModalCustomer.set(false);
  }

  onHandleSaveCustomer(data: any) {
    this.isLoading.set(true);

    const payload = {
      ...data
    }

    // Insert
    this.customerService.createCustomer(payload)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res: Customer) => {
          this.toast.success("Thêm mới thành công");
          this.onCloseModalCustomer();
          this.loadCustomers();

          this.selectedCustomer.set(res);

        },
        error: (err) => {
          this.toast.error(err.error?.message || err.message);
        }
      })
  }
}