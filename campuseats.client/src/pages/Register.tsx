import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { apiClient } from '../utils/apiClient';

const Register: React.FC = () => {
  const [formData, setFormData] = useState({
    username: '',
    email: '',
    password: '',
    confirmPassword: ''
  });
  const [error, setError] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const navigate = useNavigate();
  const { isAuthenticated } = useAuth();

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

    // Validate passwords match
    if (formData.password !== formData.confirmPassword) {
      setError('Parolele nu se potrivesc');
      return;
    }

    // Validate password strength
    if (formData.password.length < 8) {
      setError('Parola trebuie sa aiba minim 8 caractere');
      return;
    }

    setIsLoading(true);

    try {
      // Send only username, email, password and confirmPassword
      const response = await apiClient('/api/user/register', {
        method: 'POST',
        body: JSON.stringify({
          username: formData.username,
          email: formData.email,
          password: formData.password,
          confirmPassword: formData.confirmPassword
        })
      });

      if (response.ok) {
        console.log('Registration successful');
        navigate('/login');
      } else {
        const data = await response.text();
        setError(data || 'Registration failed');
      }
    } catch (err) {
      setError('An error occurred. Please try again.');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple-500 via-pink-500 to-red-500 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Card Container */}
        <div className="bg-white rounded-2xl shadow-2xl p-8 backdrop-blur-sm">
          {/* Header */}
          <div className="text-center mb-8">
            <h1 className="text-4xl font-bold bg-gradient-to-r from-purple-600 to-pink-600 bg-clip-text text-transparent mb-2">
              CampusEats
            </h1>
            <h2 className="text-2xl font-semibold text-gray-800">Creeaza-ti cont</h2>
            <p className="text-gray-600 text-sm mt-2">Alatura-te comunității noastre de studenți</p>
          </div>

          {/* Error Message */}
          {error && (
            <div className="bg-red-50 border-l-4 border-red-500 text-red-700 px-4 py-3 rounded-lg mb-6 animate-pulse">
              <p className="font-semibold">Eroare</p>
              <p className="text-sm">{error}</p>
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="space-y-4">
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
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-purple-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Alege username-ul"
                required
              />
            </div>

            {/* Email Input */}
            <div>
              <label className="block text-gray-700 font-semibold text-sm mb-2">
                Email
              </label>
              <input
                type="email"
                name="email"
                value={formData.email}
                onChange={handleChange}
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-purple-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Introdu email-ul"
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
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-purple-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Minim 8 caractere"
                required
              />
              <p className="text-gray-500 text-xs mt-1">Parola trebuie sa contina litere, numere si caractere speciale</p>
            </div>

            {/* Confirm Password Input */}
            <div>
              <label className="block text-gray-700 font-semibold text-sm mb-2">
                Confirma parola
              </label>
              <input
                type="password"
                name="confirmPassword"
                value={formData.confirmPassword}
                onChange={handleChange}
                className="w-full px-4 py-3 border-2 border-gray-200 rounded-lg focus:border-purple-500 focus:outline-none transition duration-300 placeholder-gray-400"
                placeholder="Reintroduceti parola"
                required
              />
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isLoading}
              className="w-full bg-gradient-to-r from-purple-500 to-pink-500 text-white font-semibold py-3 rounded-lg hover:from-purple-600 hover:to-pink-600 transition duration-300 transform hover:scale-105 disabled:opacity-50 disabled:cursor-not-allowed shadow-lg mt-6"
            >
              {isLoading ? 'Se creaza contul...' : 'Creeaza cont'}
            </button>
          </form>

          {/* Divider */}
          <div className="flex items-center my-6">
            <div className="flex-1 h-px bg-gray-200"></div>
            <span className="px-3 text-gray-500 text-sm">sau</span>
            <div className="flex-1 h-px bg-gray-200"></div>
          </div>

          {/* Login Link */}
          <p className="text-center text-gray-600 text-sm">
            Ai deja cont?{' '}
            <Link
              to="/login"
              className="text-purple-600 font-semibold hover:text-purple-700 transition duration-300"
            >
              Conecteaza-te acum
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

export default Register;
