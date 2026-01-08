import { MenuCategory, OrderStatus, MenuItem, Order, OrderItem } from './index';

describe('Type Definitions', () => {
  describe('MenuCategory Enum', () => {
    test('has correct values', () => {
      expect(MenuCategory.Breakfast).toBe(0);
      expect(MenuCategory.Lunch).toBe(1);
      expect(MenuCategory.Dinner).toBe(2);
      expect(MenuCategory.Snacks).toBe(3);
      expect(MenuCategory.Drinks).toBe(4);
      expect(MenuCategory.Desserts).toBe(5);
    });

    test('can be compared', () => {
      const category = MenuCategory.Lunch;
      expect(category).toBe(MenuCategory.Lunch);
      expect(category).not.toBe(MenuCategory.Breakfast);
    });

    test('can be used as object keys', () => {
      const categoryNames: Record<MenuCategory, string> = {
        [MenuCategory.Breakfast]: 'Breakfast',
        [MenuCategory.Lunch]: 'Lunch',
        [MenuCategory.Dinner]: 'Dinner',
        [MenuCategory.Snacks]: 'Snacks',
        [MenuCategory.Drinks]: 'Drinks',
        [MenuCategory.Desserts]: 'Desserts',
      };

      expect(categoryNames[MenuCategory.Breakfast]).toBe('Breakfast');
      expect(categoryNames[MenuCategory.Desserts]).toBe('Desserts');
    });
  });

  describe('OrderStatus Enum', () => {
    test('has correct values', () => {
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
    });

    test('can be compared', () => {
      const status = OrderStatus.Preparing;
      expect(status).toBe(OrderStatus.Preparing);
      expect(status).not.toBe(OrderStatus.Completed);
    });

    test('progression makes sense', () => {
      expect(OrderStatus.Pending).toBeLessThan(OrderStatus.Placed);
      expect(OrderStatus.Placed).toBeLessThan(OrderStatus.Preparing);
      expect(OrderStatus.Preparing).toBeLessThan(OrderStatus.Ready);
      expect(OrderStatus.Ready).toBeLessThan(OrderStatus.Completed);
    });

    test('can be used in conditionals', () => {
      const isActive = (status: OrderStatus) => {
        return status !== OrderStatus.Inactive && 
               status !== OrderStatus.Completed && 
               status !== OrderStatus.Cancelled;
      };

      expect(isActive(OrderStatus.Pending)).toBe(true);
      expect(isActive(OrderStatus.Preparing)).toBe(true);
      expect(isActive(OrderStatus.Completed)).toBe(false);
      expect(isActive(OrderStatus.Cancelled)).toBe(false);
      expect(isActive(OrderStatus.Inactive)).toBe(false);
    });
  });

  describe('MenuItem Interface', () => {
    test('can create valid menu item object', () => {
      const menuItem: MenuItem = {
        id: 1,
        name: 'Pizza',
        description: 'Delicious pizza',
        price: 9.99,
        category: MenuCategory.Lunch,
        imageUrl: 'https://example.com/pizza.jpg',
        isAvailable: true,
        createdAt: new Date().toISOString(),
      };

      expect(menuItem.name).toBe('Pizza');
      expect(menuItem.price).toBe(9.99);
      expect(menuItem.category).toBe(MenuCategory.Lunch);
      expect(menuItem.isAvailable).toBe(true);
    });

    test('imageUrl is optional', () => {
      const menuItem: MenuItem = {
        id: 1,
        name: 'Burger',
        description: 'Tasty burger',
        price: 5.99,
        category: MenuCategory.Lunch,
        isAvailable: true,
        createdAt: new Date().toISOString(),
      };

      expect(menuItem.imageUrl).toBeUndefined();
    });
  });

  describe('Order Interface', () => {
    test('can create valid order object', () => {
      const order: Order = {
        id: 1,
        userId: 'user123',
        status: OrderStatus.Pending,
        totalAmount: 19.98,
        orderItems: [],
        orderDate: new Date().toISOString(),
      };

      expect(order.userId).toBe('user123');
      expect(order.status).toBe(OrderStatus.Pending);
      expect(order.totalAmount).toBe(19.98);
      expect(order.orderItems).toEqual([]);
    });

    test('notes field is optional', () => {
      const order: Order = {
        id: 1,
        userId: 'user123',
        status: OrderStatus.Placed,
        totalAmount: 10.00,
        orderItems: [],
        orderDate: new Date().toISOString(),
      };

      expect(order.notes).toBeUndefined();
    });
  });

  describe('OrderItem Interface', () => {
    test('can create valid order item object', () => {
      const orderItem: OrderItem = {
        id: 1,
        menuItemId: 5,
        quantity: 2,
        price: 9.99,
      };

      expect(orderItem.menuItemId).toBe(5);
      expect(orderItem.quantity).toBe(2);
      expect(orderItem.price).toBe(9.99);
    });

    test('menuItem field is optional', () => {
      const orderItem: OrderItem = {
        id: 1,
        menuItemId: 5,
        quantity: 1,
        price: 5.99,
      };

      expect(orderItem.menuItem).toBeUndefined();
    });
  });
});
