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
    orderDate: string;
    lastUpdatedDate?: string;
    itemCount: number;
    estimatedDeliveryTime: number;
    deliveryAddress?: string;
    paymentMethod?: string;
    kitchenStatus?: string;
}

export interface OrderItem {
    id: number;
    menuItemId: number;
    menuItem?: MenuItem;
    quantity: number;
    price: number;
    menuItemDescription?: string;
    imageUrl?: string;
    subtotal: number;
    addedAt: string;
    Desserts = 5
}
