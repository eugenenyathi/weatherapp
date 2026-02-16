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

      <div className="space-y-4">
        {forecastData?.fiveDayForecasts.map((day, index) => (
          <WeatherRow
            key={index}
            day={day.date} // Using the date from the API
            rain={`${Math.round(day.rain)}%`}
            maxTemp={day.maxTemp.toString()}
            minTemp={day.minTemp.toString()}
            locationId={locationId}
            isFavorite={false} // Default to false for forecast days
            userId={user?.id}
            showActions={false} // Don't show heart and 3 dots menu in forecast
          />
        ))}
      </div>
    </div>
  );
};

export default ForecastComponent;