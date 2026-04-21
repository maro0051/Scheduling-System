import React, { useState, useEffect } from 'react';
import { swapAPI, shiftsAPI } from '../services/api';
import { useAuth } from '../contexts/AuthContext';
import toast from 'react-hot-toast';

const SwapRequests = () => {
  const [requests, setRequests] = useState([]);
  const [myShifts, setMyShifts] = useState([]);
  const [availableShifts, setAvailableShifts] = useState([]);
  const [showForm, setShowForm] = useState(false);
  const [loading, setLoading] = useState(true);
  const [formData, setFormData] = useState({
    requestorShiftId: '',
    requestedShiftId: '',
    reason: '',
  });
  const { user, isManager } = useAuth();

  useEffect(() => {
    fetchRequests();
    if (!isManager) {
      fetchMyShifts();
      fetchAvailableShifts();
    }
  }, []);

  const fetchRequests = async () => {
    try {
      setLoading(true);
      let response;
      if (isManager) {
        response = await swapAPI.getAllRequests();
      } else {
        response = await swapAPI.getMyRequests();
      }
      setRequests(response.data);
    } catch (error) {
      toast.error('Error fetching swap requests');
    } finally {
      setLoading(false);
    }
  };

  const fetchMyShifts = async () => {
    try {
      const response = await shiftsAPI.getAvailableShiftsForSwap();
      setMyShifts(response.data);
    } catch (error) {
      console.error('Error fetching my shifts:', error);
    }
  };

  const fetchAvailableShifts = async () => {
    try {
      const response = await shiftsAPI.getAllShifts();
      // Filter out current user's shifts
      const otherShifts = response.data.filter(shift => shift.userId !== user?.id);
      setAvailableShifts(otherShifts);
    } catch (error) {
      console.error('Error fetching available shifts:', error);
    }
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await swapAPI.createRequest(
        formData.requestorShiftId,
        formData.requestedShiftId,
        formData.reason
      );
      toast.success('Swap request created successfully');
      fetchRequests();
      setShowForm(false);
      resetForm();
    } catch (error) {
      toast.error(error.response?.data?.message || 'Error creating swap request');
    }
  };

  const handleStatusUpdate = async (id, status) => {
    try {
      await swapAPI.updateStatus(id, status);
      toast.success(`Request ${status.toLowerCase()}`);
      fetchRequests();
    } catch (error) {
      toast.error('Error updating request');
    }
  };

  const handleCancel = async (id) => {
    if (window.confirm('Are you sure you want to cancel this request?')) {
      try {
        await swapAPI.cancelRequest(id);
        toast.success('Request cancelled');
        fetchRequests();
      } catch (error) {
        toast.error('Error cancelling request');
      }
    }
  };

  const resetForm = () => {
    setFormData({
      requestorShiftId: '',
      requestedShiftId: '',
      reason: '',
    });
  };

  const getStatusColor = (status) => {
    switch (status) {
      case 'Pending': return 'bg-yellow-100 text-yellow-800';
      case 'Approved': return 'bg-green-100 text-green-800';
      case 'Declined': return 'bg-red-100 text-red-800';
      case 'Cancelled': return 'bg-gray-100 text-gray-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  };

  if (loading) {
    return <div className="text-center py-8">Loading...</div>;
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Shift Swap Requests</h1>
        {!isManager && (
          <button
            onClick={() => setShowForm(!showForm)}
            className="btn-primary"
          >
            {showForm ? 'Cancel' : 'Request Swap'}
          </button>
        )}
      </div>

      {showForm && !isManager && (
        <div className="card mb-6">
          <h2 className="text-xl font-semibold mb-4">Request a Shift Swap</h2>
          <form onSubmit={handleSubmit} className="space-y-4">
            <div>
              <label className="input-label">Your Shift to Give Away</label>
              <select
                value={formData.requestorShiftId}
                onChange={(e) => setFormData({ ...formData, requestorShiftId: e.target.value })}
                className="input-field"
                required
              >
                <option value="">Select your shift</option>
                {myShifts.map((shift) => (
                  <option key={shift.id} value={shift.id}>
                    {new Date(shift.shiftDate).toLocaleDateString()} - {shift.shiftType} 
                    ({shift.startTime.substring(0,5)}-{shift.endTime.substring(0,5)})
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="input-label">Shift You Want to Take</label>
              <select
                value={formData.requestedShiftId}
                onChange={(e) => setFormData({ ...formData, requestedShiftId: e.target.value })}
                className="input-field"
                required
              >
                <option value="">Select target shift</option>
                {availableShifts.map((shift) => (
                  <option key={shift.id} value={shift.id}>
                    {shift.userName} - {new Date(shift.shiftDate).toLocaleDateString()} - {shift.shiftType}
                    ({shift.startTime.substring(0,5)}-{shift.endTime.substring(0,5)})
                  </option>
                ))}
              </select>
            </div>
            <div>
              <label className="input-label">Reason for Swap (Optional)</label>
              <textarea
                value={formData.reason}
                onChange={(e) => setFormData({ ...formData, reason: e.target.value })}
                className="input-field"
                rows="3"
                placeholder="Why do you want to swap this shift?"
              />
            </div>
            <div className="flex space-x-2">
              <button type="submit" className="btn-primary">Submit Request</button>
              <button type="button" onClick={() => setShowForm(false)} className="btn-secondary">Cancel</button>
            </div>
          </form>
        </div>
      )}

      <div className="space-y-4">
        {requests.length === 0 && (
          <div className="card text-center py-8">
            <p className="text-gray-500">No swap requests found</p>
          </div>
        )}
        
        {requests.map((request) => (
          <div key={request.id} className="card">
            <div className="flex justify-between items-start">
              <div className="flex-1">
                <div className="flex items-center space-x-2 mb-3">
                  <span className={`px-2 py-1 rounded-full text-xs font-semibold ${getStatusColor(request.status)}`}>
                    {request.status}
                  </span>
                  <span className="text-sm text-gray-500">
                    {new Date(request.createdAt).toLocaleString()}
                  </span>
                </div>
                
                <p className="text-gray-800 mb-2">
                  <span className="font-semibold">{request.requestorName}</span> wants to swap with{' '}
                  <span className="font-semibold">{request.requestedUserName}</span>
                </p>
                
                {request.reason && (
                  <p className="text-sm text-gray-600 mb-3">
                    <span className="font-medium">Reason:</span> {request.reason}
                  </p>
                )}
                
                {request.requestorShift && request.requestedShift && (
                  <div className="mt-3 p-3 bg-gray-50 rounded-lg">
                    <div className="text-sm">
                      <p className="font-medium text-gray-700">Swap Details:</p>
                      <p className="text-gray-600 mt-1">
                        🟢 Give away: {new Date(request.requestorShift.shiftDate).toLocaleDateString()} - {request.requestorShift.shiftType}
                      </p>
                      <p className="text-gray-600">
                        🔵 Take: {new Date(request.requestedShift.shiftDate).toLocaleDateString()} - {request.requestedShift.shiftType}
                      </p>
                    </div>
                  </div>
                )}
              </div>
              
              <div className="flex space-x-2 ml-4">
                {request.status === 'Pending' && (
                  <>
                    {(isManager || request.requestedUserId === user?.id) && (
                      <>
                        <button
                          onClick={() => handleStatusUpdate(request.id, 'Approved')}
                          className="btn-success text-sm px-3 py-1"
                        >
                          Approve
                        </button>
                        <button
                          onClick={() => handleStatusUpdate(request.id, 'Declined')}
                          className="btn-danger text-sm px-3 py-1"
                        >
                          Decline
                        </button>
                      </>
                    )}
                    {request.requestorId === user?.id && (
                      <button
                        onClick={() => handleCancel(request.id)}
                        className="btn-secondary text-sm px-3 py-1"
                      >
                        Cancel
                      </button>
                    )}
                  </>
                )}
              </div>
            </div>
          </div>
        ))}
      </div>
    </div>
  );
};

export default SwapRequests;