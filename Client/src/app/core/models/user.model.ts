// =====DỮ LIỆU MOCK=====
export interface User{
    id: number;
    name: string;
    email: string;
    role: string;
    status: string;
    avatar?: string;
}

// Data giả để test
export const MOCK_USERS: User[] = Array.from({length: 50}, (_, i) => ({
    id: i + 1,
    name: `User ${i + 1}`,
    email: `user${i + 1}@company.com`,
    role: i % 3 === 0 ? 'Admin' : i % 3 === 1 ? 'Editor' : 'Viewer',
    status: Math.random() > 0.2 ? 'Active' : 'Inactive',
    avatar: `https://i.pravatar.cc/150?u=${i + 1}`
}));
// =====DỮ LIỆU MOCK=====

// =====DỮ LIỆU TỪ DB=====
export interface UserDto{
    id: number;
    userName: string;
    fullName: string;
    role: string;
    isActive: boolean;
    createdAt: Date | string;
}

export interface UserUpsert{
    id?: number;
    userName: string;
    fullName: string;
    password: string;
    role: string;
    isActive: boolean;
}