export interface DashboardSummary{
    totalRevenue: number;
    totalOrders: number;
    averageOrderValue: number;
    totalProductsSold: number;

    topProducts: any[],
    recentOrders: any[],
    chartData: any[],

    // Các chỉ số tăng trưởng (%)
    revenueGrowth: number;
    ordersGrowth: number;
    avgValueGrowth: number;
    productsSoldGrowth: number;
}
