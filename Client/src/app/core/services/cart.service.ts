import { computed, Injectable, signal } from '@angular/core';
import { CartItem } from '@core/models/order.model';
import { ProductResponse } from '@core/models/product.model';

@Injectable({
  providedIn: 'root',
})
export class CartService {
  // 1. State gốc: Danh sách các món trong giỏ
  cartItems = signal<CartItem[]>([]);

  // 2. State cho chiết khấu
  discount = signal<number>(0);

  // 3. Computed Signals: Tự động tính toán khi cartItems thay đổi

  // Tổng số lượng item trong giỏ (hiển thị ở icon giỏ hàng)
  totalQuantity = computed(() => {
    this.cartItems().reduce((sum, item) => sum + item.quantity, 0)
  });

  // Tổng tiền trước chiết khấu
  totalAmount = computed(() => {
    return this.cartItems().reduce((sum, item) => sum + (item.quantity * item.price), 0)
  });

  // Số tiền thực thu sau chiết khấu
  finalAmount = computed(() => {
    const total = this.totalAmount();
    const disc = this.discount();
    return total - disc > 0 ? total - disc : 0;
  })

  // Expose giỏ hàng ra ngoài dưới dạng read-only
  items = this.cartItems.asReadonly();

  // --- CÁC HÀNH ĐỘNG (ACTIONS) ---

  // Thêm sản phẩm vào giỏ
  addToCart(product: ProductResponse) {
    this.cartItems.update(currentItems => {
      const existingItem = currentItems.find(i => i.productId === product.id);

      if (existingItem) {
        // Nếu đã có, tăng số lượng nhưng không vượt quá tồn kho
        return currentItems.map(item =>
          item.productId === product.id
            ? { ...item, quantity: Math.min(item.quantity + 1, item.maxStock) }
            : item
        );
      }

      // Nếu chưa có, thêm mới vào đầu danh sách
      const newItem: CartItem = {
        productId: product.id,
        productName: product.productName,
        quantity: 1,
        price: product.price,
        maxStock: product.stockQuantity
      };

      return [newItem, ...currentItems];
    });
  }

  // Tăng số lượng
  increaseQuantity(productId: number) {
    this.cartItems.update(items =>
      items.map(i => i.productId === productId
        ? { ...i, quantity: Math.min(i.quantity + 1, i.maxStock) }
        : i
      )
    );
  }

  // Giảm số lượng hoặc xóa nếu về 0
  decreaseQuantity(productId: number) {
    this.cartItems.update(currentItems => {
      const currentItem = currentItems.find(i => i.productId === productId);

      if (currentItem && currentItem.quantity > 1) {
        return currentItems.map(item =>
          item.productId === productId ? { ...item, quantity: item.quantity - 1 } : item
        );
      }

      return currentItems.filter(i => i.productId !== productId);
    })
  }

  // Xóa toàn bộ dòng sản phẩm
  removeItem(productId: number) {
    this.cartItems.update(currentItems =>
      currentItems.filter(i => i.productId !== productId)
    );
  }

  // Reset giỏ hàng sau khi thanh toán thành công
  clearCart() {
    this.cartItems.set([]);
    this.discount.set(0);
  }
}
