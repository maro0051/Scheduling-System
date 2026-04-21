import React from 'react';
import Navbar from './Navbar';
import { Toaster } from 'react-hot-toast';

const Layout = ({ children }) => {
  return (
    <div className="min-h-screen bg-gray-50">
      <Navbar />
      <Toaster position="top-right" />
      <main className="max-w-7xl mx-auto px-4 py-8">
        {children}
      </main>
    </div>
  );
};

export default Layout;