import { Language } from '../context/LanguageContext';

export const homeTranslations: Record<
  Language,
  {
    authRequiredTitle: string;
    authRequiredLogin: string;
    authRequiredOr: string;
    authRequiredRegister: string;
    userLoadError: string;
    welcomeTitle: string;
    welcomeSubtitle: string;
    quickActions: {
      menu: { title: string; description: string };
      orders: { title: string; description: string };
      profile: { title: string; description: string };
    };
    recentActivity: {
      title: string;
      description: string;
      hint: string;
    };
  }
> = {
  ro: {
    authRequiredTitle: 'Trebuie sa te loghezi',
    authRequiredLogin: 'Intra in cont',
    authRequiredOr: 'sau',
    authRequiredRegister: 'Inregistreaza-te',
    userLoadError: 'Nu am putut incarca datele utilizatorului',
    welcomeTitle: 'Bine ai venit',
    welcomeSubtitle: 'Bucura-te de experienta CampusEats',
    quickActions: {
      menu: {
        title: 'Meniu',
        description: 'Exploreaza preparatele disponibile',
      },
      orders: {
        title: 'Comenzile mele',
        description: 'Vezi istoricul comenzilor',
      },
      profile: {
        title: 'Profil',
        description: 'Gestioneaza contul tau',
      },
    },
    recentActivity: {
      title: 'Activitate recenta',
      description: '📊 Ultimele comenzi sau puncte castigate vor aparea aici',
      hint: 'Incepe sa comanzi pentru a vedea activitatea ta',
    },
  },
  en: {
    authRequiredTitle: 'You need to log in',
    authRequiredLogin: 'Login',
    authRequiredOr: 'or',
    authRequiredRegister: 'Register',
    userLoadError: 'We could not load the user data',
    welcomeTitle: 'Welcome',
    welcomeSubtitle: 'Enjoy the CampusEats experience',
    quickActions: {
      menu: {
        title: 'Menu',
        description: 'Browse the available dishes',
      },
      orders: {
        title: 'My Orders',
        description: 'View your order history',
      },
      profile: {
        title: 'Profile',
        description: 'Manage your account',
      },
    },
    recentActivity: {
      title: 'Recent activity',
      description: '📊 Your latest orders or earned points will appear here',
      hint: 'Start ordering to see your activity',
    },
  },
};