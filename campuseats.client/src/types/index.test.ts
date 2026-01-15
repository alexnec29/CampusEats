import { OrderStatus, MenuCategory } from './index';

describe('Types Definitions', () => {
    it('should have correct values for OrderStatus enum', () => {
        expect(OrderStatus.Inactive).toBe(0);
        expect(OrderStatus.Pending).toBe(1);
        expect(OrderStatus.Placed).toBe(2);
        expect(OrderStatus.Preparing).toBe(3);
        expect(OrderStatus.Ready).toBe(4);
        expect(OrderStatus.Completed).toBe(5);
        expect(OrderStatus.Cancelled).toBe(6);
        expect(OrderStatus.Paid).toBe(7);
        expect(OrderStatus.PendingPayment).toBe(8);
        expect(OrderStatus.FailedPayment).toBe(9);

        // Reverse mapping coverage
        expect(OrderStatus[0]).toBe('Inactive');
    });

    it('should have correct values for MenuCategory enum', () => {
        expect(MenuCategory.Breakfast).toBe(0);
        expect(MenuCategory.Lunch).toBe(1);
        expect(MenuCategory.Dinner).toBe(2);
        expect(MenuCategory.Snacks).toBe(3);
        expect(MenuCategory.Drinks).toBe(4);
        expect(MenuCategory.Desserts).toBe(5);

        // Reverse mapping coverage
        expect(MenuCategory[0]).toBe('Breakfast');
    });
});
