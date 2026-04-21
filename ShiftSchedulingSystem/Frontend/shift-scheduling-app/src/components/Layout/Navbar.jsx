import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { CalendarIcon, ArrowRightOnRectangleIcon } from '@heroicons/react/24/outline';

const Navbar = () => {
  const { user, isManager, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <nav className="bg-white shadow-lg">
      <div className="max-w-7xl mx-auto px-4">
        <div className="flex justify-between h-16">
          <div className="flex items-center">
            <Link to="/" className="flex items-center space-x-2">
              <CalendarIcon className="h-8 w-8 text-primary-600" />
              <span className="font-bold text-xl text-gray-800">ShiftScheduler</span>
            </Link>
            
            <div className="hidden md:flex ml-10 space-x-8">
              <Link
                to="/schedule"
                className="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
              >
                Schedule
              </Link>
              
              {isManager && (
                <Link
                  to="/manage-shifts"
                  className="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Manage Shifts
                </Link>
              )}
              {user?.role === 'Admin' && (
                <Link
                  to="/admin-dashboard"
                  className="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
                >
                  Admin Dashboard
                </Link>
              )}
              
              <Link
                to="/swap-requests"
                className="text-gray-700 hover:text-primary-600 px-3 py-2 rounded-md text-sm font-medium"
              >
                Swap Requests
              </Link>

            </div>
          </div>
          
          <div className="flex items-center space-x-4">
            <div className="text-sm text-gray-700">
              <span className="font-medium">{user?.firstName} {user?.lastName}</span>
              <span className="ml-2 text-xs bg-gray-100 px-2 py-1 rounded">
                {user?.role}
              </span>
            </div>
            
            <button
              onClick={handleLogout}
              className="flex items-center space-x-1 text-gray-600 hover:text-red-600"
            >
              <ArrowRightOnRectangleIcon className="h-5 w-5" />
              <span>Logout</span>
            </button>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default Navbar;