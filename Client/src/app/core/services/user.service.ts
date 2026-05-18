import { Injectable } from '@angular/core';
import { MOCK_USERS, User } from '@core/models/user.model';
import { Observable, of, throwError } from 'rxjs';
import { delay } from 'rxjs/operators';

@Injectable({
  providedIn: 'root'
})
export class UserService {
  // Biến lưu trữ data trong bộ nhớ (giả lập Database)
  private users: User[] = [...MOCK_USERS];
 
  // 1. Get List (Lấy tất cả)
  getUsers(): Observable<User[]> {
    // Trả về bản sao của mảng để tránh tham chiếu trực tiếp
    return of([...this.users]).pipe(delay(500));
  }

  // 2. Get By ID (Lấy chi tiết 1 user)
  getUserById(id: number): Observable<User | undefined>{
    const user = this.users.find(u => u.id === id);
    return of(user).pipe(delay(300));
  }

  // 3. Create (Thêm mới)
  createUser(user: Partial<User>): Observable<User>{
    // Logic tự sinh ID: Lấy ID lớn nhất hiện tại + 1
    const maxId = this.users.length > 0 ? Math.max(...this.users.map(u => u.id)) : 0;

    const newUser: User = {
      ...user,
      id: maxId + 1,
      // Giá trị mặc định nếu thiếu
      role: user.role || 'Viewer',
      status: user.status || 'Active',
      avatar: user.avatar || ''
    } as User;

    // Thêm vào đầu danh sách (để thấy ngay trên UI)
    this.users = [newUser, ...this.users];

    return of(newUser).pipe(delay(800));
  }

  // 4. Update (Cập nhật)
  updateUser(id: number, data: Partial<User>): Observable<User>{
    const index = this.users.findIndex(u => u.id === id);

    if(index !== -1){
      // Merge dữ liệu cũ và mới
      const updateUser = {...this.users[index], ...data};

      // Cập nhật lại vào mảng gốc
      this.users[index] = updateUser;

      // Tạo mảng mới để trigger change detection nếu cần (best practice)
      this.users = [...this.users];

      return of(updateUser).pipe(delay(800));
    }

    return throwError(() => new Error('User not found')).pipe(delay(500));
  }
  
  // 5. Delete (Xóa)
  deleteUser(id: number): Observable<boolean>{
    const initialLength = this.users.length;
    this.users = this.users.filter(u => u.id !== id);

    const isDeleted = this.users.length < initialLength;

    if(isDeleted){
      return of(true).pipe(delay(600));
    }
    else{
      return throwError(() => new Error('Delete failed')).pipe(delay(500));
    }    
  }

  // 6. Search (Tìm kiếm Server-side - Tùy chọn)
  searchUsers(term: string): Observable<User[]> {
    const lowerTerm = term.toLowerCase();
    const result = this.users.filter(u => 
      u.name.toLowerCase().includes(lowerTerm) || 
      u.email.toLowerCase().includes(lowerTerm)
    );
    return of(result).pipe(delay(400));
  }
}