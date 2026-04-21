import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './contexts/AuthContext';
import Layout from './components/Layout/Layout';
import Login from './pages/Login';

import Schedule from './pages/Schedule';
import ManageShifts from './pages/ManageShifts';
import SwapRequests from './pages/SwapRequests';
import AdminDashboard from './pages/AdminDashboard';

const ProtectedRoute = ({ children }) => {
  const { isAuthenticated } = useAuth();
  return isAuthenticated ? children : <Navigate to="/login" />;
};

const ManagerRoute = ({ children }) => {
  const { isAuthenticated, isManager } = useAuth();
  return isAuthenticated && isManager ? children : <Navigate to="/schedule" />;
};
const AdminRoute = ({ children }) => {
  const { isAuthenticated, user } = useAuth();
  return isAuthenticated && user?.role === 'Admin' ? children : <Navigate to="/schedule" />;
}

function AppRoutes() {
  const { isAuthenticated, loading } = useAuth();

  if (loading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="text-center">
          <div className="text-xl text-gray-600">Loading...</div>
        </div>
      </div>
    );
  }

  return (
    <Routes>
      <Route path="/login" element={!isAuthenticated ? <Login /> : <Navigate to="/schedule" />} />
      
      
      <Route path="/schedule" element={
        <ProtectedRoute>
          <Layout>
            <Schedule />
          </Layout>
        </ProtectedRoute>
      } />
        <Route path="/admin-dashboard" element={
          <AdminRoute>
            <Layout>
              <AdminDashboard />
            </Layout>
          </AdminRoute>
        } />
      <Route path="/manage-shifts" element={
        <ManagerRoute>
          <Layout>
            <ManageShifts />
          </Layout>
        </ManagerRoute>
      } />
      
      <Route path="/swap-requests" element={
        <ProtectedRoute>
          <Layout>
            <SwapRequests />
          </Layout>
        </ProtectedRoute>
      } />
      
      <Route path="/" element={<Navigate to="/schedule" />} />
    </Routes>
  );
}

function App() {
  return (
    <Router>
      <AuthProvider>
        <AppRoutes />
      </AuthProvider>
    </Router>
  );
}

export default App;