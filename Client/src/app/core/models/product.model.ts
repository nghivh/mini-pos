export interface ProductResponse{
    id: number;
    categoryId: number;
    categoryName: string;
    barcode: string;
    productName: string;
    price: number;
    costPrice: number;
    stockQuantity: number;
    isActive: boolean;
}

export interface ProductUpsert{
    id: number;
    categoryId: number;
    barcode: string;
    productName: string;
    price: number;
    costPrice: number;
    stockQuantity: number;
    isActive: boolean;
}

export interface ProductQueryRequest{
    search?: string;
    categoryId?: number;
    minPrice?: number;
    maxPrice?: number;
    sortBy?: string;
    isDescending: boolean;
    page: number;
    pageSize: number;
}