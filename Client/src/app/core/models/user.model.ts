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