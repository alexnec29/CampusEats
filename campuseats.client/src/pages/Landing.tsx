import React from 'react';
import { Link } from 'react-router-dom';
import { UtensilsCrossed, ShoppingBag, Clock, Award } from 'lucide-react';
import { useLanguage } from '../context/LanguageContext';
import { landingTranslations } from '../i18n/Landing';

const Landing: React.FC = () => {
    const { language } = useLanguage();
    const template = landingTranslations[language];

    return (
        <div className="min-h-screen relative">
            {/* Background Image with Overlay */}
            <div
                className="absolute inset-0 bg-cover bg-center bg-no-repeat"
                style={{
                    backgroundImage: 'url(/images/campus-cafeteria.jpg)',
                }}
            >
                <div className="absolute inset-0 bg-gradient-to-br from-blue-900/85 via-blue-800/80 to-purple-900/85"></div>
            </div>

            {/* Content */}
            <div className="relative z-10">


                {/* Hero Section */}
                <div className="container mx-auto px-4 py-16">
                    <div className="text-center mb-16">
                        <h1 className="text-5xl md:text-6xl font-bold text-white mb-6 animate-fade-in drop-shadow-2xl">
                            {template.welcome} <span className="text-yellow-400">CampusEats</span>
                        </h1>
                        <p className="text-xl md:text-2xl text-gray-100 mb-8 max-w-3xl mx-auto animate-fade-in-delay drop-shadow-lg">
                            {template.subtitle}!
                        </p>

                        {/* CTA Button - Only Login */}
                        <div className="flex justify-center items-center animate-fade-in-delay-2">
                            <Link
                                to="/login"
                                className="px-10 py-4 bg-yellow-500 text-gray-900 font-bold rounded-lg shadow-2xl hover:bg-yellow-400 transition duration-300 transform hover:scale-105"
                            >
                                {template.login}
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
                                {template.features.menu.title}
                            </h3>
                            <p className="text-gray-600 text-center">
                                {template.features.menu.desc}
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
                                {template.features.delivery.title}
                            </h3>
                            <p className="text-gray-600 text-center">
                                {template.features.delivery.desc}
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
                                {template.features.order.title}
                            </h3>
                            <p className="text-gray-600 text-center">
                                {template.features.order.desc}
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
                                {template.features.loyalty.title}
                            </h3>
                            <p className="text-gray-600 text-center">
                                {template.features.loyalty.desc}
                            </p>
                        </div>
                    </div>

                    {/* How it works Section */}
                    <div className="mt-20 bg-white rounded-2xl shadow-xl p-8 md:p-12 animate-fade-in-slow">
                        <h2 className="text-3xl md:text-4xl font-bold text-center text-gray-900 mb-12">
                            {template.howItWorks.title}?
                        </h2>
                        <div className="grid md:grid-cols-3 gap-8">
                            <div className="text-center">
                                <div className="bg-blue-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                    1
                                </div>
                                <h3 className="text-xl font-bold mb-2">{template.howItWorks.step1.title}</h3>
                                <p className="text-gray-600">
                                    {template.howItWorks.step1.desc}
                                </p>
                            </div>
                            <div className="text-center">
                                <div className="bg-purple-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                    2
                                </div>
                                <h3 className="text-xl font-bold mb-2">{template.howItWorks.step2.title}</h3>
                                <p className="text-gray-600">
                                    {template.howItWorks.step2.desc}
                                </p>
                            </div>
                            <div className="text-center">
                                <div className="bg-green-600 text-white w-12 h-12 rounded-full flex items-center justify-center text-xl font-bold mx-auto mb-4">
                                    3
                                </div>
                                <h3 className="text-xl font-bold mb-2">{template.howItWorks.step3.title}</h3>
                                <p className="text-gray-600">
                                    {template.howItWorks.step3.desc}
                                </p>
                            </div>
                        </div>
                    </div>

                    {/* Final CTA */}
                    <div className="mt-16 text-center animate-fade-in-slow">
                        <h2 className="text-3xl font-bold text-white mb-4 drop-shadow-lg">
                            {template.cta.title}?
                        </h2>
                        <p className="text-xl text-gray-100 mb-8 drop-shadow-md">
                            {template.cta.subtitle}!
                        </p>
                        <Link
                            to="/register"
                            className="inline-block px-10 py-4 bg-yellow-500 text-gray-900 font-bold rounded-lg shadow-2xl hover:bg-yellow-400 transition duration-300 transform hover:scale-105"
                        >
                            {template.cta.button}
                        </Link>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default Landing;
