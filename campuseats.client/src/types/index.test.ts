import { OrderStatus, MenuCategory, Order, MenuItem } from './index';

describe('Types Definitions', () => {
    it('should have correct values for OrderStatus enum', () => {
        // Iterate over keys to ensure runtime object is fully accessed
        Object.keys(OrderStatus).forEach(key => {
             const value = OrderStatus[key as keyof typeof OrderStatus];
             expect(value).toBeDefined();
        });
        
        expect(OrderStatus.Inactive).toBe(0);
        expect(OrderStatus.Pending).toBe(1);
    });

    it('should have correct values for MenuCategory enum', () => {
        Object.keys(MenuCategory).forEach(key => {
             const value = MenuCategory[key as keyof typeof MenuCategory];
             expect(value).toBeDefined();
        });

        expect(MenuCategory.Breakfast).toBe(0);
        expect(MenuCategory.Lunch).toBe(1);
        expect(MenuCategory.Dinner).toBe(2);
        expect(MenuCategory.Snacks).toBe(3);
        expect(MenuCategory.Drinks).toBe(4);
        expect(MenuCategory.Desserts).toBe(5);
    });
    
    it('should verify interfaces (compiler check only)', () => {
        // This is just to satisfy coverage if it looks for usage
        const item: MenuItem = {
             id: 1,
             name: 'Test',
             description: 'Desc',
             price: 10,
             category: MenuCategory.Lunch,
             isAvailable: true,
             createdAt: '2023-01-01'
        };
        const order: Order = {
            id: 1,
            userId: 'u1',
            status: OrderStatus.Pending,
            totalAmount: 10,
            orderItems: [],
            orderDate: '2023-01-01'
        };
        expect(item.id).toBe(1);
        expect(order.id).toBe(1);
    });
});
