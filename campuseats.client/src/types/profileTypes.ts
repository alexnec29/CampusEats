export interface UserInfo {
    username: string;
    role: string;
    email?: string;
    loyaltyPoints?: number;
}

export interface Address {
    street: string;
    building: string;
    city: string;
    county: string;
}

export interface WorkingHours {
    open: string;
    close: string;
}

export interface WeeklyWorkingHours {
    monday: WorkingHours;
    tuesday: WorkingHours;
    wednesday: WorkingHours;
    thursday: WorkingHours;
    friday: WorkingHours;
    saturday: WorkingHours;
    sunday: WorkingHours;
}

export interface BuyerProfile {
    lastName: string;
    firstName: string;
    age: number;
    deliveryAddress: Address;
}

export interface KitchenProfile {
    companyName: string;
    kitchenAddress: Address;
    weeklyWorkingHours: WeeklyWorkingHours;
}

export interface PasswordChangeData {
    currentPassword: string;
    newPassword: string;
    confirmNewPassword: string;
}
