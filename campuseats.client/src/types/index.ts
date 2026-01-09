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
    loyaltyPointsUsed?: number;
    discountAmount?: number;
    orderItems: OrderItem[];
    orderDate: string;
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
    Placed = 2,
    Preparing = 3,
    Ready = 4,
    Completed = 5,
    Cancelled = 6,
    Paid = 7,
    PendingPayment = 8,
    FailedPayment = 9
}

export enum MenuCategory {
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,
    Snacks = 3,
    Drinks = 4,
    Desserts = 5
}
