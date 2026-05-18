import { Component, AfterViewInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '@core/services/auth.service';
import Chart from 'chart.js/auto';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements AfterViewInit {
  today = new Date();

  stats = [
    { label: 'Categories', value: 12, icon: '🗂' },
    { label: 'Products', value: 180, icon: '🛒' },
    { label: 'Customers', value: 320, icon: '👥' },
    { label: 'Suppliers', value: 24, icon: '🏭' }
  ];

  activities = [
    { time: '1 min ago', text: 'New customer registered.' },
    { time: '5 min ago', text: 'Product "iPhone 15" updated.' },
    { time: '12 min ago', text: 'New order placed.' },
    { time: '1 hour ago', text: 'Supplier "TechZone" added.' }
  ];

  constructor(public auth: AuthService) {}

  ngAfterViewInit(): void {
    this.loadChart();
  }

  loadChart() {
    new Chart("salesChart", {
      type: 'line',
      data: {
        labels: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun'],
        datasets: [{
          label: 'Sales',
          data: [1200, 1500, 1000, 1800, 2200, 2100],
          borderWidth: 3,
          borderColor: '#1976d2',
          backgroundColor: 'rgba(25,118,210,0.2)',
          tension: 0.3,
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false
      }
    });
  }
}
