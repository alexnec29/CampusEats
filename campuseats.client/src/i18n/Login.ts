import { Language } from '../context/LanguageContext';

export const loginTranslations: Record<
  Language,
  {
    title: string;
    subtitle: string;
    usernameLabel: string;
    usernamePlaceholder: string;
    passwordLabel: string;
    passwordPlaceholder: string;
    submit: string;
    noAccountText: string;
    registerLinkText: string;
  }
> = {
  ro: {
    title: 'Bine ai venit inapoi!',
    subtitle: 'Intra in contul tau CampusEats',
    usernameLabel: 'Nume de utilizator',
    usernamePlaceholder: 'Introdu username-ul',
    passwordLabel: 'Parola',
    passwordPlaceholder: 'Introdu parola',
    submit: 'Intra in cont',
    noAccountText: 'Nu ai cont?',
    registerLinkText: 'Inregistreaza-te',
  },
  en: {
    title: 'Welcome back!',
    subtitle: 'Log into your CampusEats account',
    usernameLabel: 'Username',
    usernamePlaceholder: 'Enter your username',
    passwordLabel: 'Password',
    passwordPlaceholder: 'Enter your password',
    submit: 'Login',
    noAccountText: 'Don’t have an account?',
    registerLinkText: 'Sign up',
  },
};