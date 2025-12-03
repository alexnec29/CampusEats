import React from 'react';

const Header: React.FC = () => {
  return (
    <header className="p-4 bg-gray-50 border-b border-gray-200">
      <nav>
        <h1 className="text-xl font-bold text-gray-800">CampusEats</h1>
        {/* Aici vei adăuga link-urile de navigare */}
      </nav>
    </header>
  );
};

export default Header;
