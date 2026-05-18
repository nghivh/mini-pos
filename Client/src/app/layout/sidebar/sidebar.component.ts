import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LayoutService } from '@core/services/layout.service';

// Định nghĩa cấu trúc 1 item trong menu
export interface MenuItem {
  label: string;
  icon: string;
  link?: string;
  expanded?: boolean; // Biến để theo dõi menu cha đang mở hay đóng
  children?: MenuItem[]; // Danh sách menu con (nếu có)
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './sidebar.component.html',
  styles: [`:host { display: contents; }`]
})
export class SidebarComponent {
  protected layout = inject(LayoutService);

  // Mảng chứa cấu trúc Menu
  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: '📊', link: '/' },
    { label: 'User',  icon: '👥', link: '/user'},
    { label: 'Customer',  icon: '👥', link: '/customer'},
    { label: 'Category',  icon: '🗂️', link: '/category'},
    { label: 'Product',   icon: '📦', link: '/product'}
    // {
    //   label: 'Warehouse In', 
    //   icon: '🏭',
    //   expanded: false,
    //   children: [
    //     { label: 'Pending List', icon: '📋', link: '/wh-ins' },
    //     { label: 'Transfer WIP', icon: '🔄', link: '/wh-in-transfer-wip' },
    //     { label: 'Print Label', icon: '🖨️', link: '/wh-in-print-label' },
    //     { label: 'Pickup', icon: '🛒', link: '/wh-in-pickup' },
    //     { label: 'Transfer', icon: '🔄', link: '/wh-in-transfer' },
    //     { label: 'Storage', icon: '📥', link: '/wh-in-storage' },
    //     { label: 'Report', icon: '📊', link: '/wh-in-report' }
    //   ]
    // },
    // {
    //   label: 'Purchasing', 
    //   icon: '📦',
    //   expanded: false,
    //   children: [
    //     { label: 'Purchaser', icon: '👤', link: '/purs' },
    //     { label: 'Approval', icon: '🛂', link: '/purs-approval' }
    //   ]
    // },
    // {
    //   label: 'Warehouse Out', 
    //   icon: '🏭',
    //   expanded: false,
    //   children: [
    //     { label: 'Pending List', icon: '📋', link: '/wh-outs' },
    //     { label: 'Print Label', icon: '🖨️', link: '/wh-out-print-label' },
    //     { label: 'Pickup', icon: '🛒', link: '/wh-out-pickup' },
    //     { label: 'Transfer', icon: '🔄', link: '/wh-out-transfer' },
    //     { label: 'Packing', icon: '📦', link: '/wh-out-packing' },
    //     { label: 'Report', icon: '📊', link: '/wh-out-report' }
    //   ]
    // }
  ];

  // State quản lý Tooltip
  tooltip = signal<{ label: string; top: number; visible: boolean }>({
    label: '',
    top: 0,
    visible: false
  });

  // Khi chuột vào menu item
  showTooltip(event: MouseEvent, label: string) {
    if (!this.layout.isSidebarCollapsed()) return;

    const target = event.currentTarget as HTMLElement;
    const rect = target.getBoundingClientRect();

    // Tính toán vị trí top (lấy giữa item)
    // rect.top là vị trí so với viewport
    this.tooltip.set({
      label,
      top: rect.top + (rect.height - 30) / 2, // 30 là chiều cao ước lượng của tooltip
      visible: true
    });
  }

  // Khi chuột rời khỏi menu item
  hideTooltip() {
    this.tooltip.set({ ...this.tooltip(), visible: false });
  }

  // Hàm xử lý khi click vào Menu Cha
  toggleMenu(item: MenuItem) {
    if (item.children) {
      item.expanded = !item.expanded;
    }
  }
}