import axios from 'axios';

const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5272/api';

// Create axios instance
const api = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Add token to every request
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('token');
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Handle 401 unauthorized responses
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
      window.location.href = '/login';
    }
    return Promise.reject(error);
  }
);

// Auth endpoints
export const authAPI = {
  login: (email, password) => api.post('/auth/login', { email, password }),
  register: (data) => api.post('/auth/register', data),
  getProfile: (userId) => api.get(`/auth/profile/${userId}`),
};

// Shifts endpoints
export const shiftsAPI = {
  getAllShifts: () => api.get('/shifts'),
  getShiftById: (id) => api.get(`/shifts/${id}`),
  getUserShifts: (userId) => api.get(`/shifts/user/${userId}`),
  getWeeklySchedule: (weekStartDate) => api.get('/shifts/weekly', { params: { weekStartDate } }),
  createShift: (data) => api.post('/shifts', data),
  updateShift: (id, data) => api.put(`/shifts/${id}`, { ...data, id }),
  deleteShift: (id) => api.delete(`/shifts/${id}`),
  getAvailableShiftsForSwap: () => api.get('/shifts/available-for-swap'),
  bulkCreateShifts: (shifts, overlapExisting) => api.post('/shifts/bulk', { shifts, overlapExisting }),
};

// Shift Swap endpoints
export const swapAPI = {
  getAllRequests: () => api.get('/shiftswaps'),
  getMyRequests: () => api.get('/shiftswaps/my-requests'),
  getPendingForMe: () => api.get('/shiftswaps/pending-for-me'),
  createRequest: (requestorShiftId, requestedShiftId, reason) => 
    api.post('/shiftswaps', { requestorShiftId, requestedShiftId, reason }),
  updateStatus: (id, status) => api.put(`/shiftswaps/${id}/status`, { id, status }),
  cancelRequest: (id) => api.delete(`/shiftswaps/${id}`),
};

export default api;