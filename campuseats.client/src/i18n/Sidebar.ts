import { Language } from '../context/LanguageContext';

export const sidebarTranslations: Record<
  Language,
  {
    navigationTitle: string;
    administration: string;
    myAccount: string;
    links: {
      home: string;
      menu: string;
      orders: string;
      kitchenDashboard: string;
      addNewMenuItem: string;
      adminDashboard: string;
      profile: string;
      logout: string;
    };
  }
> = {
  ro: {
    navigationTitle: 'Navigare',
    administration: 'Administrare',
    myAccount: 'Contul meu',
    links: {
      home: 'Acasa',
      menu: 'Meniu',
      orders: 'Comenzi',
      kitchenDashboard: 'Panou administrare bucatarie',
      addNewMenuItem: 'Adauga un item nou in meniu',
      adminDashboard: 'Panou pentru administrator',
      profile: 'Profil',
      logout: 'Iesi din cont',
    },
  },
  en: {
    navigationTitle: 'Navigation',
    administration: 'Administration',
    myAccount: 'My Account',
    links: {
      home: 'Home',
      menu: 'Menu',
      orders: 'Orders',
      kitchenDashboard: 'Kitchen Dashboard',
      addNewMenuItem: 'Add new menu item',
      adminDashboard: 'Admin Dashboard',
      profile: 'Profile',
      logout: 'Logout',
    },
  },
};