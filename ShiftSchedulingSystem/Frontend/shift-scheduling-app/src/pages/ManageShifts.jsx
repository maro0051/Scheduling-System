import React, { useState, useEffect } from 'react';
import { shiftsAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

const ManageShifts = () => {
  const [shifts, setShifts] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [editingShift, setEditingShift] = useState(null);
  const [formData, setFormData] = useState({
    userId: '',
    shiftDate: '',
    startTime: '',
    endTime: '',
    shiftType: 'Morning',
    department: '',
    notes: '',
  });
  const { user } = useAuth();

  useEffect(() => {
    fetchShifts();
  }, []);

  const fetchShifts = async () => {
    try {
      const response = await shiftsAPI.getAllShifts();
      setShifts(response.data);
    } catch (error) {
      toast.error('Error fetching shifts');
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      if (editingShift) {
        await shiftsAPI.updateShift(editingShift.id, formData);
        toast.success('Shift updated successfully');
      } else {
        await shiftsAPI.createShift(formData);
        toast.success('Shift created successfully');
      }
      fetchShifts();
      setShowForm(false);
      setEditingShift(null);
      resetForm();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Error saving shift');
    }
  };

  const handleDelete = async (id) => {
    if (window.confirm('Are you sure you want to delete this shift?')) {
      try {
        await shiftsAPI.deleteShift(id);
        toast.success('Shift deleted successfully');
        fetchShifts();
      } catch (error) {
        toast.error('Error deleting shift');
      }
    }
  };

  const handleEdit = (shift) => {
    setEditingShift(shift);
    setFormData({
      userId: shift.userId,
      shiftDate: shift.shiftDate.split('T')[0],
      startTime: shift.startTime,
      endTime: shift.endTime,
      shiftType: shift.shiftType,
      department: shift.department || '',
      notes: shift.notes || '',
    });
    setShowForm(true);
  };

  const resetForm = () => {
    setFormData({
      userId: '',
      shiftDate: '',
      startTime: '',
      endTime: '',
      shiftType: 'Morning',
      department: '',
      notes: '',
    });
  };

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Manage Shifts</h1>
        <button
          onClick={() => setShowForm(!showForm)}
          className="btn-primary"
        >
          {showForm ? 'Cancel' : 'Add Shift'}
        </button>
      </div>

      {showForm && (
        <div className="card mb-6">
          <h2 className="text-xl font-semibold mb-4">
            {editingShift ? 'Edit Shift' : 'Create New Shift'}
          </h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <label className="input-label">User ID</label>
                <input
                  type="number"
                  name="userId"
                  value={formData.userId}
                  onChange={(e) => setFormData({ ...formData, userId: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Shift Date</label>
                <input
                  type="date"
                  name="shiftDate"
                  value={formData.shiftDate}
                  onChange={(e) => setFormData({ ...formData, shiftDate: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Start Time</label>
                <input
                  type="time"
                  name="startTime"
                  value={formData.startTime}
                  onChange={(e) => setFormData({ ...formData, startTime: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">End Time</label>
                <input
                  type="time"
                  name="endTime"
                  value={formData.endTime}
                  onChange={(e) => setFormData({ ...formData, endTime: e.target.value })}
                  className="input-field"
                  required
                />
              </div>
              <div>
                <label className="input-label">Shift Type</label>
                <select
                  name="shiftType"
                  value={formData.shiftType}
                  onChange={(e) => setFormData({ ...formData, shiftType: e.target.value })}
                  className="input-field"
                >
                  <option value="Morning">Morning</option>
                  <option value="Afternoon">Afternoon</option>
                  <option value="Night">Night</option>
                </select>
              </div>
              <div>
                <label className="input-label">Department</label>
                <input
                  type="text"
                  name="department"
                  value={formData.department}
                  onChange={(e) => setFormData({ ...formData, department: e.target.value })}
                  className="input-field"
                />
              </div>
            </div>
            <div>
              <label className="input-label">Notes</label>
              <textarea
                name="notes"
                value={formData.notes}
                onChange={(e) => setFormData({ ...formData, notes: e.target.value })}
                className="input-field"
                rows="3"
              />
            </div>
            <div className="flex space-x-2">
              <button type="submit" className="btn-primary">
                {editingShift ? 'Update' : 'Create'} Shift
              </button>
              <button
                type="button"
                onClick={() => {
                  setShowForm(false);
                  setEditingShift(null);
                  resetForm();
                }}
                className="btn-secondary"
              >
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      <div className="overflow-x-auto">
        <table className="min-w-full bg-white rounded-lg overflow-hidden shadow">
          <thead className="bg-gray-100">
            <tr>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Employee</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Date</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Time</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Type</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Department</th>
              <th className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Actions</th>
            </tr>
          </thead>
          <tbody className="divide-y divide-gray-200">
            {shifts.map((shift) => (
              <tr key={shift.id}>
                <td className="px-6 py-4 whitespace-nowrap">{shift.userName}</td>
                <td className="px-6 py-4 whitespace-nowrap">{new Date(shift.shiftDate).toLocaleDateString()}</td>
                <td className="px-6 py-4 whitespace-nowrap">
                  {shift.startTime.substring(0, 5)} - {shift.endTime.substring(0, 5)}
                </td>
                <td className="px-6 py-4 whitespace-nowrap">{shift.shiftType}</td>
                <td className="px-6 py-4 whitespace-nowrap">{shift.department || '-'}</td>
                <td className="px-6 py-4 whitespace-nowrap space-x-2">
                  <button
                    onClick={() => handleEdit(shift)}
                    className="text-primary-600 hover:text-primary-900"
                  >
                    Edit
                  </button>
                  <button
                    onClick={() => handleDelete(shift.id)}
                    className="text-red-600 hover:text-red-900"
                  >
                    Delete
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default ManageShifts;