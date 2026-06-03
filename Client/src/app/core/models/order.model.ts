export interface CartItem {
    productId: number;
    productName: string;
    quantity: number;
    price: number;
    maxStock: number;
}

export interface OrderDetail{
    productId: number;
    quantity: number;
    unitPrice: number;
}

export interface CheckoutRequest{
    customerId?: number;
    discountAmount: number;
    paymentMethod?: string;
    notes?: string;
    orderDetails: OrderDetail[]
}