import React, { useState, useEffect } from 'react';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';
import api from '../services/api';

const AdminDashboard = () => {
  const [users, setUsers] = useState([]);
  const [stats, setStats] = useState(null);
  const [showCreateForm, setShowCreateForm] = useState(false);
  const [loading, setLoading] = useState(false);
  const [formData, setFormData] = useState({
    email: '',
    firstName: '',
    lastName: '',
    role: 'Employee',
    phoneNumber: '',
    password: '',
    sendEmail: true,
  });
  const { user } = useAuth();

  useEffect(() => {
    fetchUsers();
    fetchStats();
  }, []);

  const fetchUsers = async () => {
    try {
      const response = await api.get('/admin/users');
      setUsers(response.data);
    } catch (error) {
      toast.error('Error fetching users');
    }
  };

  const fetchStats = async () => {
    try {
      const response = await api.get('/admin/stats');
      setStats(response.data);
    } catch (error) {
      console.error('Error fetching stats:', error);
    }
  };

  const handleCreateUser = async (e) => {
    e.preventDefault();
    setLoading(true);
    try {
      const response = await api.post('/admin/create-user', formData);
      toast.success(response.data.message);
      
      if (response.data.user.temporaryPassword) {
        if (formData.sendEmail) {
          toast.success(`Login credentials sent to ${formData.email}`, {
            duration: 5000,
          });
        } else {
          toast.info(`Temporary Password: ${response.data.user.temporaryPassword}`, {
            duration: 10000,
          });
        }
      }
      
      setShowCreateForm(false);
      resetForm();
      fetchUsers();
      fetchStats();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Error creating user');
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async (userId, userEmail) => {
    if (window.confirm(`Reset password for ${userEmail}? They will receive an email with new credentials.`)) {
      try {
        const response = await api.post(`/admin/users/${userId}/reset-password`);
        toast.success(response.data.message);
        fetchUsers();
      } catch (error) {
        toast.error('Error resetting password');
      }
    }
  };

  const handleResendWelcomeEmail = async (userId, userEmail, userName) => {
    if (window.confirm(`Resend welcome email to ${userEmail}?`)) {
      try {
        const response = await api.post(`/admin/users/${userId}/resend-welcome-email`);
        toast.success(response.data.message);
        if (response.data.temporaryPassword) {
          toast.info(`New temporary password: ${response.data.temporaryPassword}`, {
            duration: 8000,
          });
        }
      } catch (error) {
        toast.error('Error sending email');
      }
    }
  };

  const handleDeleteUser = async (userId, userEmail, userRole) => {
    if (userRole === 'Admin') {
      toast.error('Cannot delete Admin users');
      return;
    }
    
    if (window.confirm(`Delete ${userEmail}? This action cannot be undone.`)) {
      try {
        await api.delete(`/admin/users/${userId}`);
        toast.success('User deleted successfully');
        fetchUsers();
        fetchStats();
      } catch (error) {
        toast.error(error.response?.data?.message || 'Error deleting user');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      email: '',
      firstName: '',
      lastName: '',
      role: 'Employee',
      phoneNumber: '',
      password: '',
      sendEmail: true,
    });
  };

  const getRoleBadgeColor = (role) => {
    switch (role) {
      case 'Admin': return 'bg-purple-100 text-purple-800';
      case 'Manager': return 'bg-blue-100 text-blue-800';
      case 'Employee': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Admin Dashboard</h1>
          <p className="text-sm text-gray-600 mt-1">Manage users, create accounts, and send credentials via email</p>
        </div>
        <button
          onClick={() => setShowCreateForm(!showCreateForm)}
          className="btn-primary"
        >
          {showCreateForm ? 'Cancel' : '+ Create User'}
        </button>
      </div>

      {/* Statistics Cards */}
      {stats && (
        <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
          <div className="card">
            <p className="text-sm text-gray-500">Total Users</p>
            <p className="text-2xl font-bold text-gray-900">{stats.totalUsers}</p>
          </div>
          <div className="card">
            <p className="text-sm text-gray-500">Admins</p>
            <p className="text-2xl font-bold text-purple-600">{stats.totalAdmins}</p>
          </div>
          <div className="card">
            <p className="text-sm text-gray-500">Managers</p>
            <p className="text-2xl font-bold text-blue-600">{stats.totalManagers}</p>
          </div>
          <div className="card">
            <p className="text-sm text-gray-500">Employees</p>
            <p className="text-2xl font-bold text-green-600">{stats.totalEmployees}</p>
          </div>
          <div className="card">
            <p className="text-sm text-gray-500">Active Users</p>
            <p className="text-2xl font-bold text-gray-900">{stats.activeUsers}</p>
          </div>
        </div>
      )}

      {/* Create User Form */}
      {showCreateForm && (
        <div className="card">
          <h2 className="text-xl font-semibold mb-4">Create New User</h2>
          <form onSubmit={handleCreateUser} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="input-label">First Name *</label>
                <input
                  type="text"
                  value={formData.firstName}
                  onChange={(e) => setFormData({ ...formData, firstName: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Last Name *</label>
                <input
                  type="text"
                  value={formData.lastName}
                  onChange={(e) => setFormData({ ...formData, lastName: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Email *</label>
                <input
                  type="email"
                  value={formData.email}
                  onChange={(e) => setFormData({ ...formData, email: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Role *</label>
                <select
                  value={formData.role}
                  onChange={(e) => setFormData({ ...formData, role: e.target.value })}
                  className="input-field"
                  required
                >
                  <option value="Employee">Employee</option>
                  <option value="Manager">Manager</option>
                </select>
              </div>
              <div>
                <label className="input-label">Phone Number</label>
                <input
                  type="tel"
                  value={formData.phoneNumber}
                  onChange={(e) => setFormData({ ...formData, phoneNumber: e.target.value })}
                  className="input-field"
                />
              </div>
              <div>
                <label className="input-label">Password (leave empty for auto-generate)</label>
                <input
                  type="text"
                  value={formData.password}
                  onChange={(e) => setFormData({ ...formData, password: e.target.value })}
                  className="input-field"
                />
              </div>
            </div>
            
            {/* Email Notification Toggle */}
            <div className="flex items-center space-x-3">
              <input
                type="checkbox"
                id="sendEmail"
                checked={formData.sendEmail}
                onChange={(e) => setFormData({ ...formData, sendEmail: e.target.checked })}
                className="w-4 h-4 text-primary-600 border-gray-300 rounded focus:ring-primary-500"
              />
              <label htmlFor="sendEmail" className="text-sm text-gray-700">
                Send login credentials via email
              </label>
            </div>
            <p className="text-xs text-gray-500">
              {formData.sendEmail 
                ? "User will receive an email with their login credentials" 
                : "You will need to provide the password to the user manually"}
            </p>
            
            <div className="flex space-x-2">
              <button type="submit" disabled={loading} className="btn-primary">
                {loading ? 'Creating...' : 'Create User'}
              </button>
              <button type="button" onClick={() => setShowCreateForm(false)} className="btn-secondary">
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Users Table */}
      <div className="card">
        <h2 className="text-xl font-semibold mb-4">All Users</h2>
        <div className="overflow-x-auto">
          <table className="min-w-full">
            <thead className="bg-gray-50">
              <tr>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">ID</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Name</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Email</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Role</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Phone</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Status</th>
                <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-gray-200">
              {users.map((u) => (
                <tr key={u.id}>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-900">{u.id}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <div className="text-sm font-medium text-gray-900">
                      {u.firstName} {u.lastName}
                    </div>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{u.email}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getRoleBadgeColor(u.role)}`}>
                      {u.role}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{u.phoneNumber || '-'}</td>
                  <td className="px-6 py-4 whitespace-nowrap">
                    <span className={`px-2 py-1 rounded-full text-xs font-semibold ${u.isActive ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}`}>
                      {u.isActive ? 'Active' : 'Inactive'}
                    </span>
                  </td>
                  <td className="px-6 py-4 whitespace-nowrap text-sm space-x-2">
                    <button
                      onClick={() => handleResetPassword(u.id, u.email)}
                      className="text-blue-600 hover:text-blue-900"
                      title="Reset password and send email"
                    >
                      Reset Pwd
                    </button>
                    {u.role !== 'Admin' && (
                      <>
                        <button
                          onClick={() => handleResendWelcomeEmail(u.id, u.email, u.firstName)}
                          className="text-green-600 hover:text-green-900"
                          title="Resend welcome email with new password"
                        >
                          Resend Email
                        </button>
                        <button
                          onClick={() => handleDeleteUser(u.id, u.email, u.role)}
                          className="text-red-600 hover:text-red-900"
                        >
                          Delete
                        </button>
                      </>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
};

export default AdminDashboard;