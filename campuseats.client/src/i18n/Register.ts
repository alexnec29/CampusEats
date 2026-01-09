import { Language } from '../context/LanguageContext';

export const registerTranslations: Record<
  Language,
  {
    title: string;
    subtitle: string;
    usernameLabel: string;
    usernamePlaceholder: string;
    emailLabel: string;
    emailPlaceholder: string;
    passwordLabel: string;
    passwordPlaceholder: string;
    confirmPasswordLabel: string;
    confirmPasswordPlaceholder: string;
    submit: string;
    haveAccountText: string;
    loginLinkText: string;
  }
> = {
  ro: {
    title: 'Creeaza un cont',
    subtitle: 'Inregistreaza-te pentru CampusEats',
    usernameLabel: 'Nume de utilizator',
    usernamePlaceholder: 'Alege un nume de utilizator',
    emailLabel: 'Adresa de email',
    emailPlaceholder: 'adresa-de-email@exemplu.com',
    passwordLabel: 'Parola',
    passwordPlaceholder: 'Creeaza o parola',
    confirmPasswordLabel: 'Confirma parola',
    confirmPasswordPlaceholder: 'Confirma parola',
    submit: 'Inregistreaza-te',
    haveAccountText: 'Ai deja un cont?',
    loginLinkText: 'Intra in cont',
  },
  en: {
    title: 'Create an account',
    subtitle: 'Sign up for CampusEats',
    usernameLabel: 'Username',
    usernamePlaceholder: 'Choose a username',
    emailLabel: 'Email',
    emailPlaceholder: 'email-adress@example.com',
    passwordLabel: 'Password',
    passwordPlaceholder: 'Create a password',
    confirmPasswordLabel: 'Confirm Password',
    confirmPasswordPlaceholder: 'Confirm your password',
    submit: 'Sign up',
    haveAccountText: 'Already have an account?',
    loginLinkText: 'Login',
  },
};