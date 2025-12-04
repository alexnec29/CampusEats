import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Login from './pages/Login';
import Register from './pages/Register';
import Menu from './pages/Menu';
import Orders from './pages/Orders';
import AddMenuItem from './pages/AddMenuItem';
import { AuthProvider } from './context/AuthContext';

function App() {
  return (
    <AuthProvider>
      <Router>
        <Layout>
          <Routes>
            <Route path="/" element={
              <div>
                <h2>Bine ai venit la CampusEats!</h2>
                <p>Aici va fi conținutul principal al paginii tale.</p>
              </div>
            } />
            <Route path="/login" element={<Login />} />
            <Route path="/register" element={<Register />} />
            <Route path="/menu" element={<Menu />} />
            <Route path="/orders" element={<Orders />} />
            <Route path="/add-menu-item" element={<AddMenuItem />} />
          </Routes>
        </Layout>
      </Router>
    </AuthProvider>
  );
}

export default App;
