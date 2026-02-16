"use client";

import { useAuth } from "./AuthContext";
import { useCurrentDaySummaries } from "./hooks/weatherHooks";
import WeatherRow from "./WeatherRow";
import { trackLocationService } from "./services";

const LocationsList = ({
  onSelectLocation,
  activeTab = 'favorites',
  onTabChange,
}: {
  onSelectLocation: (locationId: string, locationName: string) => void;
  activeTab?: string;
  onTabChange?: (tab: string) => void;
}) => {
  const { user } = useAuth();
  const {
    data: weatherSummaries,
    isLoading,
    error,
    refetch, // Add refetch function
  } = useCurrentDaySummaries(user?.id || "");

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
          <p className="text-red-500">
            Error loading weather data: {error.message}
          </p>
        </div>
      </div>
    );
  }

  // Filter locations based on active tab
  const filteredSummaries =
    activeTab === "favorites"
      ? weatherSummaries?.filter((summary) => summary.isFavorite)
      : weatherSummaries;

  const handleFavoriteClick = async (trackedLocationId: string) => {
    if (!user?.id || !trackedLocationId) return;

    // Find the current summary to get the current favorite status
    const currentSummary = weatherSummaries?.find(
      (summary) => summary.id === trackedLocationId,
    );
    if (!currentSummary) return;

    // Determine the new favorite status
    const newFavoriteStatus = !currentSummary.isFavorite;
    
    // Update the tracked location with the new favorite status
    await trackLocationService.updateTrackLocation(
      user.id,
      trackedLocationId, // Use the tracked location ID (the record ID)
      { isFavorite: newFavoriteStatus },
    );

    // If the location is now a favorite and we're in the 'all' tab, switch to 'favorites'
    if (newFavoriteStatus && activeTab === 'all' && onTabChange) {
      onTabChange('favorites');
    }

    // Refetch the data to update the UI
    refetch();
  };

  const handleEditClick = (locationId: string, locationName: string) => {
    // Placeholder for edit functionality
    alert(`Edit functionality for ${locationName} would go here`);
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Location rows */}
      <div className="space-y-4">
        {filteredSummaries?.map((summary) => (
          <div key={summary.locationId} className="cursor-default">
            <WeatherRow
              day={summary.locationName}
              rain={`${Math.round(summary.rain)}%`}
              maxTemp={summary.maxTemp.toString()}
              minTemp={summary.minTemp.toString()}
              trackedLocationId={summary.id} // Use the tracked location ID
              isFavorite={summary.isFavorite}
              userId={user?.id}
              showActions={true} // Show heart and 3 dots menu in weather list
              onHeartClick={() => handleFavoriteClick(summary.id)}
              onViewMoreClick={() =>
                onSelectLocation(summary.locationId, summary.locationName)
              }
              onEditClick={() =>
                handleEditClick(summary.locationId, summary.locationName)
              }
            />
          </div>
        ))}
        {(!filteredSummaries || filteredSummaries.length === 0) && (
          <div className="text-center py-4 text-gray-500">
            <p>
              {activeTab === "favorites"
                ? "No favorite locations yet. Mark a location as favorite to see it here."
                : "No locations tracked yet. Add a location to see weather forecasts."}
            </p>
          </div>
        )}
      </div>
    </div>
  );
};

export default LocationsList;
