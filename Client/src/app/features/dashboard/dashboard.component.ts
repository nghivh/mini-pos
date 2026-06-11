import { Component, AfterViewInit, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@core/services/auth.service';
import Chart from 'chart.js/auto';
import { ButtonComponent } from '@shared/ui/button/button.component';
import { DashboardService } from '@core/services/dashboard.service';
import { DashboardSummary } from '@core/models/dashboard.model';
import { finalize } from 'rxjs';
import { TableComponent } from '@shared/ui/table/table.component';
import { ToastService } from '@core/services/toast.service';
import { FormsModule } from '@angular/forms';
import { NgApexchartsModule } from 'ng-apexcharts';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    NgApexchartsModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  dashboardService = inject(DashboardService);
  toast = inject(ToastService);

  // Lưu trữ dữ liệu Dashboard và trạng thái tải dữ liệu
  dashboardData = signal<DashboardSummary | null>(null);
  isLoading = signal(false);

  activeFilter = signal<'today' | 'week' | 'month' | 'custom'>('today');
  customStartDate = signal<string>('');
  customEndDate = signal<string>('');

  lastUpdated = signal<Date>(new Date());

  // Khai báo cấu hình giao diện biểu đồ
  chartOptions: any;

  chartData = [
      { "label": "01/06", "revenue": 1500000 },
      { "label": "02/06", "revenue": 2300000 },
      { "label": "03/06", "revenue": 1800000 },
      { "label": "04/06", "revenue": 3100000 }
    ];

  /*
  // Thống kê tổng quan
  totalRevenue = signal(15450000);
  totalOrders = signal(124);
  avgOrderValue = signal(124596);
  totalProductsSold = signal(342);

  // Danh sách sản phẩm bán chạy
  topProducts = signal([
    { name: 'Cà phê Sữa đá', sold: 145, revenue: 4350000 },
    { name: 'Trà Đào Cam Sả', sold: 98, revenue: 3430000 },
    { name: 'Bánh Croissant', sold: 56, revenue: 1680000 },
    { name: 'Sinh tố Bơ', sold: 43, revenue: 1935000 },
  ]);

  // Giao dịch gần nhất
  recentOrders = signal([
    { id: 'HD-00124', time: '10:24', customer: 'Khách lẻ', amount: 55000, status: 'Hoàn thành', method: 'Tiền mặt' },
    { id: 'HD-00123', time: '10:15', customer: 'Nguyễn Văn A', amount: 125000, status: 'Hoàn thành', method: 'Chuyển khoản' },
    { id: 'HD-00122', time: '09:45', customer: 'Trần Thị B', amount: 45000, status: 'Hoàn thành', method: 'Tiền mặt' },
    { id: 'HD-00121', time: '09:30', customer: 'Khách lẻ', amount: 80000, status: 'Hoàn thành', method: 'Chuyển khoản' },
  ]);
  */

  ngOnInit(): void {
    this.initChart(); // Khởi tạo giao diện chart trống
    this.setFilter('today');
    //this.loadDashboardData();
  }

  // --- 2. XỬ LÝ SỰ KIỆN NÚT BẤM ---
  setFilter(filter: 'today' | 'week' | 'month' | 'custom') {
    this.activeFilter.set(filter);

    if (filter !== 'custom') {
      // Clear lịch nếu chuyển về các bộ lọc nhanh
      this.customStartDate.set('');
      this.customEndDate.set('');

      // Gọi API ngay lập tức
      this.loadDashboardData();
    }
  }

  applyCustomFilter() {
    if (!this.customStartDate() || !this.customEndDate()) {
      this.toast.warning('Vui lòng chọn đầy đủ "Từ ngày" và "Đến ngày"');
      return;
    }

    // Lọc thủ công
    this.loadDashboardData();
  }

  loadDashboardData() {
    this.isLoading.set(true);

    // Lấy cặp ngày tương ứng với bộ lọc hiện tại
    const { startDate, endDate } = this.calculateDateRange();
    console.log(this.activeFilter(), startDate, endDate);

    this.dashboardService.getSummary(startDate, endDate)
      .pipe(
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (res) => {
          this.dashboardData.set(res);
          console.log(this.dashboardData());

          // Gán chartData
          this.chartOptions.series = [{
            name: 'Doanh thu',
            data: res.chartData.map(item => item.revenue)
          }];
          this.chartOptions.xaxis = {
            ...this.chartOptions.xaxis,
            categories: res.chartData.map(item => item.label)
          }
        },
        error: (err) => {
          console.error('Lỗi khi tải dữ liệu dashboard', err);
        }
      })
  }

  private calculateDateRange(): { startDate: string, endDate: string } {
    const filter = this.activeFilter();

    if (filter === 'custom') {
      return { startDate: this.customStartDate(), endDate: this.customEndDate() };
    }

    const today = new Date();
    let start = new Date();
    let end = new Date();

    if (filter === 'today') {
      // Bắt đầu và kết thúc đều là hôm nay
      start = today;
      end = today;
    }
    else if (filter === 'week') {
      // Tính từ Thứ 2 đến Chủ nhật tuần này
      const day = today.getDay();
      const diff = today.getDate() - day + (day === 0 ? -6 : 1); // Điều chỉnh về Thứ 2
      start = new Date(today.setDate(diff));
      end = new Date(start);
      end.setDate(start.getDate() + 6);
    }
    else if (filter === 'month') {
      // Tính từ ngày 1 đến ngày cuối cùng của tháng hiện tại
      start = new Date(today.getFullYear(), today.getMonth(), 1);
      end = new Date(today.getFullYear(), today.getMonth() + 1, 0);
    }

    // Format về chuẩn YYYY-MM-DD để gửi xuống Backend C#
    return {
      startDate: this.formatDateForApi(start),
      endDate: this.formatDateForApi(end)
    };
  }

  private formatDateForApi(date: Date): string {
    const yyyy = date.getFullYear();
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    const dd = String(date.getDate()).padStart(2, '0');
    return `${yyyy}-${mm}-${dd}`;
  }


  // Hàm thiết lập giao diện biểu đồ (Màu sắc, Font chữ, Trục X/Y)
  initChart() {
    this.chartOptions = {
      series: [
        {
          name: 'Doanh thu',
          data: [] // Dữ liệu sẽ được update khi gọi API
        }
      ],
      chart: {
        height: 200,
        type: 'area', // Biểu đồ vùng (có đổ màu mờ bên dưới)
        fontFamily: 'Inter, sans-serif',
        toolbar: { show: false },
        zoom: { enabled: false }
      },
      colors: ['#2563eb'], // Màu xanh blue-600 của Tailwind
      dataLabels: { enabled: false }, // Ẩn các con số trên điểm uốn để chart đỡ rối
      stroke: {
        curve: 'smooth', // Đường cong mềm mại
        width: 3
      },
      fill: {
        type: "gradient",
        gradient: {
          shadeIntensity: 1,
          opacityFrom: 0.4,
          opacityTo: 0.05,
          stops: [0, 90, 100]
        }
      },
      xaxis: {
        categories: [], // Nhãn trục X (Ngày/Giờ)
        axisBorder: { show: false },
        axisTicks: { show: false },
        labels: {
          style: { colors: '#94a3b8' } // Màu slate-400
        }
      },
      yaxis: {
        labels: {
          style: { colors: '#94a3b8' },
          formatter: (value: number) => {
            return value.toLocaleString('vi-VN') + 'đ'; // Format tiền Việt
          }
        }
      },
      grid: {
        borderColor: '#f1f5f9', // Đường kẻ ngang màu slate-100
        strokeDashArray: 4, // Nét đứt
      }
    };
  }
}
