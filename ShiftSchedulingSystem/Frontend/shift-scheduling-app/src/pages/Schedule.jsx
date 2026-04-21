import React, { useState, useEffect } from 'react';
import { shiftsAPI } from '../services/api';
import { format, startOfWeek, addDays } from 'date-fns';
import { ChevronLeftIcon, ChevronRightIcon } from '@heroicons/react/24/outline';

const Schedule = () => {
  const [schedule, setSchedule] = useState(null);
  const [currentWeek, setCurrentWeek] = useState(new Date());
  const [loading, setLoading] = useState(true);

  const weekDays = ['Monday', 'Tuesday', 'Wednesday', 'Thursday', 'Friday', 'Saturday', 'Sunday'];

  useEffect(() => {
    fetchSchedule();
  }, [currentWeek]);

  const fetchSchedule = async () => {
    try {
      setLoading(true);
      const startOfCurrentWeek = startOfWeek(currentWeek, { weekStartsOn: 1 });
      const response = await shiftsAPI.getWeeklySchedule(startOfCurrentWeek.toISOString());
      setSchedule(response.data);
    } catch (error) {
      console.error('Error fetching schedule:', error);
    } finally {
      setLoading(false);
    }
  };

  const previousWeek = () => {
    setCurrentWeek(prev => addDays(prev, -7));
  };

  const nextWeek = () => {
    setCurrentWeek(prev => addDays(prev, 7));
  };

  if (loading) {
    return <div className="text-center py-8">Loading schedule...</div>;
  }

  return (
    <div>
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-3xl font-bold text-gray-900">Weekly Schedule</h1>
        <div className="flex items-center space-x-4">
          <button onClick={previousWeek} className="btn-secondary">
            <ChevronLeftIcon className="h-5 w-5" />
          </button>
          <span className="text-lg font-semibold">
            {format(currentWeek, 'MMMM d, yyyy')}
          </span>
          <button onClick={nextWeek} className="btn-secondary">
            <ChevronRightIcon className="h-5 w-5" />
          </button>
        </div>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-7 gap-4">
        {weekDays.map((day, index) => {
          const currentDate = startOfWeek(currentWeek, { weekStartsOn: 1 });
          const date = addDays(currentDate, index);
          const dateKey = format(date, 'yyyy-MM-dd');
          const dayShifts = schedule?.shiftsByDay[dateKey] || [];

          return (
            <div key={day} className="card">
              <h3 className="font-semibold text-lg mb-2">{day}</h3>
              <p className="text-sm text-gray-500 mb-3">{format(date, 'MMM d')}</p>
              <div className="space-y-2">
                {dayShifts.map((shift) => (
                  <div key={shift.id} className="bg-gray-50 p-2 rounded border border-gray-200">
                    <p className="font-medium text-sm">{shift.userName}</p>
                    <p className="text-xs text-gray-600">
                      {shift.startTime.substring(0, 5)} - {shift.endTime.substring(0, 5)}
                    </p>
                    <p className="text-xs text-primary-600 font-medium">{shift.shiftType}</p>
                    {shift.department && (
                      <p className="text-xs text-gray-500">{shift.department}</p>
                    )}
                  </div>
                ))}
                {dayShifts.length === 0 && (
                  <p className="text-xs text-gray-400 text-center py-4">No shifts scheduled</p>
                )}
              </div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

export default Schedule;