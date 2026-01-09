import React from 'react';
import { Globe } from 'lucide-react';
import { useLanguage } from '../context/LanguageContext';

const LanguageSelector: React.FC = () => {
  const { language, setLanguage } = useLanguage();

  return (
    <div className="fixed top-1 right-2 z-50">
      <div className="flex items-center gap-2 group">
        <div
          className="
            bg-white rounded-lg shadow-lg p-2 flex items-center gap-2
            opacity-0 pointer-events-none
            group-hover:opacity-100 group-hover:pointer-events-auto
            transition-all duration-200
          "
        >
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

        <button className="bg-white rounded-full shadow-md p-2 flex items-center justify-center">
          <Globe className="w-5 h-5 text-gray-600" />
        </button>
      </div>
    </div>
  );
};

export default LanguageSelector;