import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import Layout from './components/Layout';
import Login from './pages/Login';
import Register from './pages/Register';
import Profile from './pages/Profile';
import Home from './pages/Home';
import Menu from './pages/Menu';
import Orders from './pages/Orders';
import Cart from './pages/Cart';
import Payment from './pages/Payment';
import KitchenOrders from './pages/KitchenOrders';
import AddMenuItem from './pages/AddMenuItem';
import { AuthProvider } from './context/AuthContext';
import PrivateRoute from './components/PrivateRoute';
import PublicRoute from './components/PublicRoute';
import AdminDashboard from './pages/AdminDashboard';
import AdminUsers from './pages/AdminUsers';

function App() {
    return (
        <AuthProvider>
            <Router>
                <Layout>
                    <Routes>
                        <Route
                            path="/"
                            element={
                                <PrivateRoute>
                                    <Home />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/profile"
                            element={
                                <PrivateRoute>
                                    <Profile />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/menu"
                            element={
                                <PrivateRoute>
                                    <Menu />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/orders"
                            element={
                                <PrivateRoute>
                                    <Orders />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/cart"
                            element={
                                <PrivateRoute>
                                    <Cart />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/payment"
                            element={
                                <PrivateRoute>
                                    <Payment />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/kitchen-orders"
                            element={
                                <PrivateRoute>
                                    <KitchenOrders />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/add-menu-item"
                            element={
                                <PrivateRoute>
                                    <AddMenuItem />
                                </PrivateRoute>
                            }
                        />
                        <Route
                            path="/admin"
                            element={
                                <PrivateRoute>
                                    <AdminDashboard />
                                </PrivateRoute>
                            }
                        />

                        <Route
                            path="/admin/users"
                            element={
                                <PrivateRoute>
                                    <AdminUsers />
                                </PrivateRoute>
                            }
                        />

                        {/* Login/Register - only for public (not logged in) */}
                        <Route
                            path="/login"
                            element={
                                <PublicRoute>
                                    <Login />
                                </PublicRoute>
                            }
                        />
                        <Route
                            path="/register"
                            element={
                                <PublicRoute>
                                    <Register />
                                </PublicRoute>
                            }
                        />

                        {/* Catch-all */}
                        <Route
                            path="*"
                            element={
                                <div className="p-8 text-center">
                                    <h2 className="text-xl font-bold">Pagina nu există</h2>
                                    <p>Folosește meniul pentru a naviga.</p>
                                </div>
                            }
                        />
                    </Routes>
                </Layout>
            </Router>
        </AuthProvider>
    );
}

export default App;
