'use client';

import { useAuth } from './AuthContext';
import { useFiveDayForecast } from './hooks/weatherHooks';
import WeatherRow from './WeatherRow';

interface ForecastComponentProps {
  locationId: string;
  locationName: string;
  onBack: () => void;
}

const ForecastComponent = ({ locationId, locationName, onBack }: ForecastComponentProps) => {
  const { user } = useAuth();
  const { data: forecastData, isLoading, error } = useFiveDayForecast(locationId, user?.id || '');

  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-gray-800">{locationName} 5-Day Forecast</h2>
          <button
            onClick={onBack}
            className="text-gray-600 hover:text-gray-900"
          >
            ← Back
          </button>
        </div>
        <div className="text-center py-4">
          <p>Loading forecast...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="flex items-center justify-between mb-4">
          <h2 className="text-lg font-bold text-gray-800">{locationName} 5-Day Forecast</h2>
          <button
            onClick={onBack}
            className="text-gray-600 hover:text-gray-900"
          >
            ← Back
          </button>
        </div>
        <div className="text-center py-4">
          <p className="text-red-500">Error loading forecast: {error.message}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-4 md:p-6">
      <div className="flex flex-col md:flex-row items-start md:items-center justify-between mb-4 gap-2">
        <h2 className="text-base md:text-lg font-bold text-gray-800">{locationName} 5-Day Forecast</h2>
        <button
          onClick={onBack}
          className="text-gray-600 hover:text-gray-900 text-sm md:text-base"
        >
          ← Back
        </button>
      </div>

      <div className="space-y-3 md:space-y-4">
        {forecastData?.fiveDayForecasts.map((day, index) => (
          <WeatherRow
            key={index}
            day={day.date}
            rain={`${Math.round(day.rain)}%`}
            maxTemp={day.maxTemp.toString()}
            minTemp={day.minTemp.toString()}
            isFavorite={false}
            showActions={false}
          />
        ))}
      </div>
    </div>
  );
};

export default ForecastComponent;