export interface MenuItem {
    id: number;
    name: string;
    description: string;
    price: number;
    category: number; // Enum
    imageUrl?: string;
    isAvailable: boolean;
    createdAt: string;
}

export interface Order {
    id: number;
    userId: string;
    notes?: string;
    status: OrderStatus;
    totalAmount: number;
    orderItems: OrderItem[];
    createdAt: string;
}

export interface OrderItem {
    id: number;
    menuItemId: number;
    menuItem?: MenuItem;
    quantity: number;
    price: number;
}

export enum OrderStatus {
    Inactive = 0,
    Pending = 1,
    Preparing = 2,
    Ready = 3,
    Completed = 4,
    Cancelled = 5
}
