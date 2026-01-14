import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';
import { useLanguage } from '../context/LanguageContext';
import { registerTranslations } from '../i18n/Register';

const Register: React.FC = () => {
  const { language } = useLanguage();
  const template = registerTranslations[language];
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState('');
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/home');
    }
  }, [isAuthenticated, navigate]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setFormData({
      ...formData,
      [e.target.name]: e.target.value
    });
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    try {
      const response = await apiClient('/api/user/register', {
        method: 'POST',
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        console.log('Registration successful');
        navigate('/login');
      } else {
        const data = await response.text();
        setError(data || 'Registration failed');
      }
    } catch (err) {
      console.error('Registration failed:', err);
      setError('An error occurred. Please try again.');
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 via-white to-purple-50 flex items-center justify-center p-4 page-transition">
      <div className="max-w-md w-full">
        <div className="bg-white rounded-2xl shadow-2xl p-8 md:p-10 transform transition-all duration-300 hover:shadow-3xl">
          <div className="text-center mb-8">
            <h2 className="text-4xl font-bold text-gray-900 mb-2">
              {template.title}
            </h2>
            <p className="text-gray-600">{template.subtitle}</p>
          </div>

          {error && (
            <div className="bg-red-50 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded mb-6 animate-fade-in">
              <p className="font-medium">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            <div>
              <label className="block text-gray-700 font-semibold mb-2">{template.usernameLabel}</label>
              <input
                type="text"
                name="username"
                value={formData.username}
                onChange={handleChange}
                className="w-full border-2 border-gray-300 p-3 rounded-lg focus:border-purple-500 focus:outline-none transition duration-200"
                placeholder={template.usernamePlaceholder}
                required
              />
            </div>
            <div>
              <label className="block text-gray-700 font-semibold mb-2">{template.emailLabel}</label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                className="w-full border-2 border-gray-300 p-3 rounded-lg focus:border-purple-500 focus:outline-none transition duration-200"
                placeholder={template.emailPlaceholder}
                required
              />
            </div>
            <div>
              <label className="block text-gray-700 font-semibold mb-2">{template.passwordLabel}</label>
              <input
                type="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                className="w-full border-2 border-gray-300 p-3 rounded-lg focus:border-purple-500 focus:outline-none transition duration-200"
                placeholder={template.passwordPlaceholder}
                required
              />
            </div>
            <div>
              <label className="block text-gray-700 font-semibold mb-2">{template.confirmPasswordLabel}</label>
              <input
                type="password"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                className="w-full border-2 border-gray-300 p-3 rounded-lg focus:border-purple-500 focus:outline-none transition duration-200"
                placeholder={template.confirmPasswordPlaceholder}
                required
              />
            </div>
            <button 
              type="submit" 
              className="w-full bg-gradient-to-r from-purple-600 to-blue-600 text-white font-bold p-4 rounded-lg hover:from-purple-700 hover:to-blue-700 transform hover:scale-105 transition duration-300 shadow-lg"
            >
              {template.submit}
            </button>
          </form>

          <div className="text-center mt-6">
            <p className="text-gray-600">
              {template.haveAccountText}{" "}
              <a href="/login" className="text-purple-600 hover:text-purple-800 font-semibold hover:underline">
                {template.loginLinkText}
              </a>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;
