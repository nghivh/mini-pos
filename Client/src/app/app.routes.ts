import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { publicGuard } from '@core/guards/public.guard';
import { adminGuard } from '@core/guards/admin-guard';

export const routes: Routes = [
    // --- PUBLIC ROUTES (Login, Register...) ---
    {
        path: 'auth/login',
        //canActivate: [publicGuard], // Đã login thì cấm vào đây
        loadComponent: () => import('./features/auth/login/login.component').then((m) => m.LoginComponent),
    },

    // --- PROTECTED ROUTES (Dashboard, Profile...) ---
    {
        path: '',
        canActivate: [authGuard], // Chưa login thì cấm vào đây
        loadComponent: () =>
        import('./layout/main-layout/main-layout.component').then(m => m.MainLayoutComponent),

        children: [
            { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
            { path: 'dashboard', loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent) },
            { path: 'user', loadComponent: () => import('./features/user/user-list/user-list.component').then(m => m.UserListComponent),
                canActivate: [authGuard, adminGuard] // ← authGuard PHẢI đứng trước
             },
            { path: 'customer', loadComponent: () => import('./features/customer/customer-list/customer-list.component').then(m => m.CustomerListComponent)},
            { path: 'category', loadComponent: () => import('./features/category/category-list/category-list.component').then(m => m.CategoryListComponent)},
            { path: 'product', loadComponent: () => import('./features/product/product-list/product-list.component').then(m => m.ProductListComponent)},
            { path: 'order', loadComponent: () => import('./features/order/order-create/order-create.component').then(m => m.OrderCreateComponent) }
        ]
    },        

    // --- FALLBACK ---
    { path: '**', redirectTo: '' },
];
