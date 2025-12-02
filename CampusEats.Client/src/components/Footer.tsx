import React from 'react';

const Footer: React.FC = () => {
  return (
    <footer className="p-4 bg-gray-50 border-t border-gray-200 mt-auto">
      <p className="text-center text-gray-600">&copy; {new Date().getFullYear()} CampusEats. Toate drepturile rezervate.</p>
    </footer>
  );
};

export default Footer;
