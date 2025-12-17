import React, { useState } from 'react';
import { Link } from 'react-router-dom';
import { UtensilsCrossed, ShoppingBag, Clock, Award, Globe } from 'lucide-react';

const Landing: React.FC = () => {
    const [language, setLanguage] = useState<'ro' | 'en'>('ro');

    const translations = {
        ro: {
            welcome: 'Bine ai venit la',
            subtitle: 'Comandă mâncare delicioasă direct din campus, rapid și simplu!',
            login: 'Login',
            features: {
                menu: { title: 'Meniu Variat', desc: 'Alege din sute de preparate delicioase disponibile în campus' },
                delivery: { title: 'Livrare Rapidă', desc: 'Primește comanda ta în cel mai scurt timp posibil' },
                order: { title: 'Comandă Ușor', desc: 'Interfață simplă și intuitivă pentru comenzi rapide' },
                loyalty: { title: 'Puncte Loialitate', desc: 'Câștigă puncte cu fiecare comandă și obține reduceri' }
            },
            howItWorks: {
                title: 'Cum funcționează?',
                step1: { title: 'Înregistrează-te', desc: 'Creează un cont în câteva secunde și explorează meniul' },
                step2: { title: 'Alege Mâncarea', desc: 'Navighează prin meniu și adaugă produsele favorite în coș' },
                step3: { title: 'Primește Comanda', desc: 'Plătește online și primește comanda ta rapid' }
            },
            cta: {
                title: 'Gata să începi?',
                subtitle: 'Înregistrează-te acum și bucură-te de mâncare delicioasă!',
                button: 'Înregistrează-te Gratuit'
            }
        },
        en: {
            welcome: 'Welcome to',
            subtitle: 'Order delicious food directly from campus, fast and simple!',
            login: 'Login',
            features: {
                menu: { title: 'Varied Menu', desc: 'Choose from hundreds of delicious dishes available on campus' },
                delivery: { title: 'Fast Delivery', desc: 'Receive your order in the shortest time possible' },
                order: { title: 'Easy Order', desc: 'Simple and intuitive interface for quick orders' },
                loyalty: { title: 'Loyalty Points', desc: 'Earn points with every order and get discounts' }
            },
            howItWorks: {
                title: 'How does it work?',
                step1: { title: 'Register', desc: 'Create an account in seconds and explore the menu' },
                step2: { title: 'Choose Food', desc: 'Browse the menu and add your favorite products to cart' },
                step3: { title: 'Get Your Order', desc: 'Pay online and receive your order quickly' }
            },
            cta: {
                title: 'Ready to start?',
                subtitle: 'Register now and enjoy delicious food!',
                button: 'Sign Up Free'
            }
        }
    };

    const t = translations[language];

    return (
        <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50">
            {/* Language Selector */}
            <div className="absolute top-4 left-4 z-10">
                <div className="bg-white rounded-lg shadow-md p-2 flex items-center gap-2">
                    <Globe className="w-5 h-5 text-gray-600" />
                    <button
                        onClick={() => setLanguage('ro')}
                        className={`px-3 py-1 rounded transition ${
                            language === 'ro' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
                        }`}
                    >
                        RO
                    </button>
                    <button
                        onClick={() => setLanguage('en')}
                        className={`px-3 py-1 rounded transition ${
                            language === 'en' ? 'bg-blue-600 text-white' : 'text-gray-600 hover:bg-gray-100'
                        }`}
                    >
                        EN
                    </button>
                </div>
            </div>

            {/* Hero Section */}
            <div className="container mx-auto px-4 py-16">
                <div className="text-center mb-16">
                    <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6 animate-fade-in">
                        {t.welcome} <span className="text-blue-600">CampusEats</span>
                    </h1>
                    <p className="text-xl md:text-2xl text-gray-600 mb-8 max-w-3xl mx-auto animate-fade-in-delay">
                        {t.subtitle}
                    </p>
                    
                    {/* CTA Button - Only Login */}
                    <div className="flex justify-center items-center animate-fade-in-delay-2">
                        <Link
                            to="/login"
                            className="px-10 py-4 bg-blue-600 text-white font-semibold rounded-lg shadow-lg hover:bg-blue-700 transition duration-300 transform hover:scale-105"
                        >
                            {t.login}
                        </Link>
                    </div>
                </div>

                {/* Features Section */}
                <div className="grid md:grid-cols-2 lg:grid-cols-4 gap-8 mt-20">
                    {/* Feature 1 */}
                    <div className="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition duration-300 transform hover:-translate-y-2 animate-slide-up">
                        <div className="flex justify-center mb-4">
                            <div className="bg-blue-100 p-4 rounded-full">
                                <UtensilsCrossed className="w-8 h-8 text-blue-600" />
                            </div>
                        </div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2 text-center">
                            {t.features.menu.title}
                        </h3>
                        <p className="text-gray-600 text-center">
                            {t.features.menu.desc}
                        </p>
                    </div>

                    {/* Feature 2 */}
                    <div className="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition duration-300 transform hover:-translate-y-2 animate-slide-up-delay">
                        <div className="flex justify-center mb-4">
                            <div className="bg-green-100 p-4 rounded-full">
                                <Clock className="w-8 h-8 text-green-600" />
                            </div>
                        </div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2 text-center">
                            {t.features.delivery.title}
                        </h3>
                        <p className="text-gray-600 text-center">
                            {t.features.delivery.desc}
                        </p>
                    </div>

                    {/* Feature 3 */}
                    <div className="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition duration-300 transform hover:-translate-y-2 animate-slide-up-delay-2">
                        <div className="flex justify-center mb-4">
                            <div className="bg-purple-100 p-4 rounded-full">
                                <ShoppingBag className="w-8 h-8 text-purple-600" />
                            </div>
                        </div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2 text-center">
                            {t.features.order.title}
                        </h3>
                        <p className="text-gray-600 text-center">
                            {t.features.order.desc}
                        </p>
                    </div>

                    {/* Feature 4 */}
                    <div className="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition duration-300 transform hover:-translate-y-2 animate-slide-up-delay-3">
                        <div className="flex justify-center mb-4">
                            <div className="bg-yellow-100 p-4 rounded-full">
                                <Award className="w-8 h-8 text-yellow-600" />
                            </div>
                        </div>
                        <h3 className="text-xl font-bold text-gray-900 mb-2 text-center">
                            {t.features.loyalty.title}
                        </h3>
                        <p className="text-gray-600 text-center">
                            {t.features.loyalty.desc}
                        </p>
                    </div>
                </div>

                {/* How it works Section */}
                <div className="mt-20 bg-white rounded-2xl shadow-xl p-8 md:p-12 animate-fade-in-slow">
                    <h2 className="text-3xl md:text-4xl font-bold text-center text-gray-900 mb-12">
                        {t.howItWorks.title}
                    </h2>
                    <div className="grid md:grid-cols-3 gap-8">
                        <div className="text-center">
                            <div className="bg-blue-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                1
                            </div>
                            <h3 className="text-xl font-bold mb-2">{t.howItWorks.step1.title}</h3>
                            <p className="text-gray-600">
                                {t.howItWorks.step1.desc}
                            </p>
                        </div>
                        <div className="text-center">
                            <div className="bg-purple-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                2
                            </div>
                            <h3 className="text-xl font-bold mb-2">{t.howItWorks.step2.title}</h3>
                            <p className="text-gray-600">
                                {t.howItWorks.step2.desc}
                            </p>
                        </div>
                        <div className="text-center">
                            <div className="bg-green-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                3
                            </div>
                            <h3 className="text-xl font-bold mb-2">{t.howItWorks.step3.title}</h3>
                            <p className="text-gray-600">
                                {t.howItWorks.step3.desc}
                            </p>
                        </div>
                    </div>
                </div>

                {/* Final CTA */}
                <div className="mt-16 text-center animate-fade-in-slow">
                    <h2 className="text-3xl font-bold text-gray-900 mb-4">
                        {t.cta.title}
                    </h2>
                    <p className="text-xl text-gray-600 mb-8">
                        {t.cta.subtitle}
                    </p>
                    <Link
                        to="/register"
                        className="inline-block px-10 py-4 bg-gradient-to-r from-blue-600 to-purple-600 text-white font-bold rounded-lg shadow-lg hover:from-blue-700 hover:to-purple-700 transition duration-300 transform hover:scale-105"
                    >
                        {t.cta.button}
                    </Link>
                </div>
            </div>
        </div>
    );
};

export default Landing;
