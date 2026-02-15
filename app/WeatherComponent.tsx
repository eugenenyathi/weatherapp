'use client';

import { useAuth } from './AuthContext';
import { useCurrentDaySummaries } from './hooks/weatherHooks';
import WeatherRow from './WeatherRow';

const LocationsList = ({ onSelectLocation }: { onSelectLocation: (locationId: string, locationName: string) => void }) => {
  const { user } = useAuth();
  const { data: weatherSummaries, isLoading, error } = useCurrentDaySummaries(user?.id || '');

  if (isLoading) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="text-center py-4">
          <p>Loading weather data...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white rounded-lg shadow-md p-6">
        <div className="text-center py-4">
          <p className="text-red-500">Error loading weather data: {error.message}</p>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Location rows */}
      <div className="space-y-4">
        {weatherSummaries?.map((summary) => (
          <div
            key={summary.locationId}
            onClick={() => onSelectLocation(summary.locationId, summary.locationName)}
            className="cursor-pointer"
          >
            <WeatherRow
              day={summary.locationName}
              rain={`${Math.round(summary.rain)}%`}
              maxTemp={summary.maxTemp.toString()}
              minTemp={summary.minTemp.toString()}
            />
          </div>
        ))}
        {(!weatherSummaries || weatherSummaries.length === 0) && (
          <div className="text-center py-4 text-gray-500">
            <p>No locations tracked yet. Add a location to see weather forecasts.</p>
          </div>
        )}
      </div>
    </div>
  );
};

export default LocationsList;