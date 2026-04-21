import React from 'react';
import { Link } from 'react-router-dom';

const Register = () => {
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <h2 className="mt-6 text-center text-3xl font-extrabold text-gray-900">
            Registration Disabled
          </h2>
          <div className="mt-4 p-4 bg-yellow-50 border border-yellow-200 rounded-lg">
            <p className="text-center text-yellow-800">
              Self-registration is disabled.
            </p>
            <p className="text-center text-sm text-yellow-700 mt-2">
              Please contact your system administrator to create an account.
            </p>
          </div>
          <div className="mt-6 text-center">
            <Link to="/login" className="text-primary-600 hover:text-primary-500">
              ← Back to Login
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Register;