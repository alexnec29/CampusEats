import { OrderStatus } from '../types';

export const ORDER_STATUS_LABELS: Record<OrderStatus, string> = {
    [OrderStatus.Inactive]: 'Inactive',
    [OrderStatus.Pending]: 'Cart',
    [OrderStatus.Placed]: 'Placed',
    [OrderStatus.Paid]: 'Paid',
    [OrderStatus.Preparing]: 'Preparing',
    [OrderStatus.Ready]: 'Ready',
    [OrderStatus.Completed]: 'Completed',
    [OrderStatus.Cancelled]: 'Cancelled',
    [OrderStatus.PendingPayment]: 'PendingPayment',
    [OrderStatus.FailedPayment]: 'FailedPayment',
};

export const ORDER_STATUS_COLORS: Record<OrderStatus, string> = {
    [OrderStatus.Inactive]: 'bg-gray-200 text-gray-600',
    [OrderStatus.Pending]: 'bg-yellow-100 text-yellow-800',
    [OrderStatus.Placed]: 'bg-blue-100 text-blue-800',
    [OrderStatus.Paid]: 'bg-green-100 text-green-800',
    [OrderStatus.Preparing]: 'bg-purple-100 text-purple-800',
    [OrderStatus.Ready]: 'bg-green-100 text-green-800',
    [OrderStatus.Completed]: 'bg-gray-100 text-gray-800',
    [OrderStatus.Cancelled]: 'bg-red-100 text-red-800',
    [OrderStatus.PendingPayment]: 'bg-yellow-100 text-yellow-800',
    [OrderStatus.FailedPayment]: 'bg-red-100 text-red-800',
};

export const getOrderStatusLabel = (status: OrderStatus): string => {
    return ORDER_STATUS_LABELS[status] || 'Unknown';
};

export const getOrderStatusColor = (status: OrderStatus): string => {
    return ORDER_STATUS_COLORS[status] || 'bg-gray-100 text-gray-800';
};
