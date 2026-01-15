import { sidebarTranslations } from './Sidebar';

describe('Sidebar Translations', () => {
    it('should have translations for both languages', () => {
        expect(sidebarTranslations).toHaveProperty('ro');
        expect(sidebarTranslations).toHaveProperty('en');
    });

    it('should have correct structure for "ro"', () => {
        expect(sidebarTranslations.ro).toHaveProperty('navigationTitle', 'Navigare');
        expect(sidebarTranslations.ro).toHaveProperty('administration', 'Administrare');
        expect(sidebarTranslations.ro).toHaveProperty('myAccount', 'Contul meu');
        expect(sidebarTranslations.ro).toHaveProperty('links');
        expect(sidebarTranslations.ro.links).toHaveProperty('home', 'Acasa');
        expect(sidebarTranslations.ro.links).toHaveProperty('logout', 'Iesi din cont');
    });

    it('should have correct structure for "en"', () => {
        expect(sidebarTranslations.en).toHaveProperty('navigationTitle', 'Navigation');
        expect(sidebarTranslations.en).toHaveProperty('administration', 'Administration');
        expect(sidebarTranslations.en).toHaveProperty('myAccount', 'My Account');
        expect(sidebarTranslations.en).toHaveProperty('links');
        expect(sidebarTranslations.en.links).toHaveProperty('home', 'Home');
        expect(sidebarTranslations.en.links).toHaveProperty('logout', 'Logout');
    });
});
