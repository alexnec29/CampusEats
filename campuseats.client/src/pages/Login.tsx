import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';

const Login: React.FC = () => {
  const [formData, setFormData] = useState({
    username: '',
    password: ''
  });
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { isAuthenticated, checkAuthStatus } = useAuth();

  useEffect(() => {
    if (isAuthenticated) {
      navigate('/');
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
    setIsLoading(true);

    try {
      const response = await apiClient('/api/user/login', {
        method: 'POST',
        body: JSON.stringify(formData)
      });

      if (response.ok) {
        console.log('Login successful');
        await checkAuthStatus(); 
        navigate('/');
      } else {
        const data = await response.text();
        setError(data || 'Login failed');
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-500 via-blue-600 to-purple-700 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Card Container */}
        <div className="bg-white rounded-2xl shadow-2xl p-8 backdrop-blur-sm">
          {/* Header */}
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold bg-gradient-to-r from-blue-600 to-purple-600 bg-clip-text text-transparent mb-2">
              CampusEats
            </h1>
            <h2 className="text-2xl font-semibold text-gray-800">Bine ai venit!</h2>
            <p className="text-gray-600 text-sm mt-2">Acceseaza-ti contul pentru a continua</p>
          </div>

          {/* Error Message */}
          {error && (
            <div className="bg-red-50 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded-lg mb-6 animate-pulse">
              <p className="font-semibold">Eroare</p>
              <p className="text-sm">{error}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Username Input */}
            <div>
              <label className="block text-gray-700 font-semibold text-sm mb-2">
                Utilizator
              </label>
              <input
                type="text"
                name="username"
                value={formData.username}
                onChange={handleChange}
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-blue-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Introdu username-ul"
                required
              />
            </div>

            {/* Password Input */}
            <div>
              <label className="block text-gray-700 font-semibold text-sm mb-2">
                Parola
              </label>
              <input
                type="password"
                name="password"
                value={formData.password}
                onChange={handleChange}
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-blue-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Introdu parola"
                required
              />
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gradient-to-r from-blue-500 to-blue-600 text-white font-semibold py-3 rounded-lg hover:from-blue-600 hover:to-blue-700 transition duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg"
            >
              {isLoading ? 'Se conecteaza...' : 'Conectare'}
            </button>
          </form>

          {/* Divider */}
          <div className="flex items-center my-6">
            <div className="flex-1 h-px bg-gray-200"></div>
            <span className="px-3 text-gray-500 text-sm">sau</span>
            <div className="flex-1 h-px bg-gray-200"></div>
          </div>

          {/* Register Link */}
          <p className="text-center text-gray-600 text-sm">
            Nu ai cont?{' '}
            <Link
              to="/register"
              className="text-blue-600 font-semibold hover:text-blue-700 transition duration-300"
            >
              Creeaza-ti cont acum
            </Link>
          </p>
        </div>

        {/* Footer */}
        <p className="text-center text-white text-sm mt-8 opacity-75">
          © 2024 CampusEats. Toti drepturi rezervate.
        </p>
      </div>
    </div>
  );
};

export default Login;
