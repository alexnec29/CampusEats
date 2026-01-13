import { Language } from '../context/LanguageContext';

export const landingTranslations: Record<Language, {
  welcome: string;
  subtitle: string;
  login: string;
  features: {
    menu: { title: string; desc: string };
    delivery: { title: string; desc: string };
    order: { title: string; desc: string };
    loyalty: { title: string; desc: string };
  };
  howItWorks: {
    title: string;
    step1: { title: string; desc: string };
    step2: { title: string; desc: string };
    step3: { title: string; desc: string };
  };
  cta: {
    title: string;
    subtitle: string;
    button: string;
  };
}> = {
  ro: {
    welcome: 'Bine ai venit la',
    subtitle: 'Comanda mancare delicioasa direct din campus, rapid si simplu',
    login: 'Intra in cont',
    features: {
      menu: { title: 'Meniu variat', desc: 'Alege din sute de preparate delicioase disponibile in campus' },
      delivery: { title: 'Livrare rapida', desc: 'Primeste comanda in cel mai scurt timp posibil' },
      order: { title: 'Comanda usor', desc: 'Interfata simpla si intuitiva pentru comenzi rapide' },
      loyalty: { title: 'Puncte loialitate', desc: 'Castiga puncte cu fiecare comanda si obtine reduceri' },
    },
    howItWorks: {
      title: 'Cum funcționeaza',
      step1: { title: 'Inregistreaza-te', desc: 'Creeaza un cont in câteva secunde si exploreaza meniul' },
      step2: { title: 'Alege mancarea', desc: 'Navigheaza prin meniu si adauga produsele favorite in cos' },
      step3: { title: 'Primeste comanda', desc: 'Plateste online si primeste comanda rapid' },
    },
    cta: {
      title: 'Gata sa incepi',
      subtitle: 'Inregistreaza-te acum si bucura-te de mancare delicioasa',
      button: 'Inregistreaza-te gratuit',
    },
  },
  en: {
    welcome: 'Welcome to',
    subtitle: 'Order delicious food directly from campus, fast and simple',
    login: 'Login',
    features: {
      menu: { title: 'Varied Menu', desc: 'Choose from hundreds of delicious dishes available on campus' },
      delivery: { title: 'Fast Delivery', desc: 'Receive your order in the shortest time possible' },
      order: { title: 'Easy To Order', desc: 'Simple and intuitive interface for quick orders' },
      loyalty: { title: 'Loyalty Points', desc: 'Earn points with every order and get discounts' },
    },
    howItWorks: {
      title: 'How does it work',
      step1: { title: 'Register', desc: 'Create an account in seconds and explore the menu' },
      step2: { title: 'Choose Food', desc: 'Browse the menu and add your favorite products to cart' },
      step3: { title: 'Get Your Order', desc: 'Pay online and receive your order quickly' },
    },
    cta: {
      title: 'Ready to start',
      subtitle: 'Register now and enjoy delicious food',
      button: 'Sign Up For Free',
    },
  },
};