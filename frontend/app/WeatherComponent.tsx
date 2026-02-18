"use client";

import { useState } from "react";
import { useAuth } from "./AuthContext";
import { useCurrentDaySummaries } from "./hooks/weatherHooks";
import WeatherRow from "./WeatherRow";
import { trackLocationService } from "./services";
import EditModal from "./components/EditModal";
import { Droplets, ThermometerSun, CloudRain, Heart, MoreVertical } from "lucide-react";

const LocationsList = ({
  onSelectLocation,
  activeTab = "favorites",
  onTabChange,
  onHourlyWeatherClick,
  onForecastClick,
  onTodayClick,
}: {
  onSelectLocation: (locationId: string, locationName: string) => void;
  activeTab?: string;
  onTabChange?: (tab: string) => void;
  onHourlyWeatherClick?: (locationId: string, locationName: string) => void;
  onForecastClick?: (locationId: string, locationName: string) => void;
  onTodayClick?: (locationId: string, locationName: string, summary: string) => void;
}) => {
  const { user } = useAuth();
  const {
    data: weatherSummaries,
    isLoading,
    error,
    refetch, // Add refetch function
  } = useCurrentDaySummaries(user?.id || "");

  const [removingIds, setRemovingIds] = useState<Set<string>>(new Set());
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingLocation, setEditingLocation] = useState<{
    id: string;
    displayName: string;
  } | null>(null);

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
      ? weatherSummaries?.filter(
          (summary) => summary.isFavorite && !removingIds.has(summary.id),
        )
      : weatherSummaries?.filter((summary) => !removingIds.has(summary.id));

  const handleFavoriteClick = async (trackedLocationId: string) => {
    console.log("Heart clicked for tracked location ID:", trackedLocationId);
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
    if (newFavoriteStatus && activeTab === "all" && onTabChange) {
      onTabChange("favorites");
    }

    // Refetch the data to update the UI
    refetch();
  };

  const handleRemoveClick = async (trackedLocationId: string) => {
    console.log("Remove clicked for tracked location ID:", trackedLocationId);
    if (!user?.id || !trackedLocationId) return;

    // Optimistically update the UI by adding the ID to the removing set
    setRemovingIds((prev) => new Set(prev).add(trackedLocationId));

    try {
      // Remove the tracked location
      await trackLocationService.deleteTrackLocation(
        user.id,
        trackedLocationId,
      );

      // Refetch the data to update the UI permanently
      refetch();
    } catch (error) {
      console.error("Error removing location:", error);
      // If there's an error, remove the ID from the removing set
      setRemovingIds((prev) => {
        const newSet = new Set(prev);
        newSet.delete(trackedLocationId);
        return newSet;
      });
    } finally {
      // Remove the ID from the removing set after the API call completes
      setRemovingIds((prev) => {
        const newSet = new Set(prev);
        newSet.delete(trackedLocationId);
        return newSet;
      });
    }
  };

  const handleEditClick = (locationId: string, displayName: string) => {
    setEditingLocation({ id: locationId, displayName });
    setIsEditModalOpen(true);
  };

  const handleEditSave = async (newDisplayName: string) => {
    if (!editingLocation || !user?.id) return;

    try {
      // Update the tracked location with the new display name
      await trackLocationService.updateTrackLocation(
        user.id,
        editingLocation.id,
        { displayName: newDisplayName },
      );

      // Close the modal
      setIsEditModalOpen(false);
      setEditingLocation(null);

      // Refetch the data to update the UI
      refetch();
    } catch (error) {
      console.error("Error updating display name:", error);
    }
  };

  const handleEditModalClose = () => {
    setIsEditModalOpen(false);
    setEditingLocation(null);
  };

  // Get the last synced time from the first location (they should all be similar)
  const lastSyncedTime = filteredSummaries?.[0]?.lastSyncedAt;
  const formatLastSynced = (dateString?: string) => {
    if (!dateString) return "Never";
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    
    if (diffMins < 1) return "Just now";
    if (diffMins === 1) return "1 minute ago";
    if (diffMins < 60) return `${diffMins} minutes ago`;
    
    const diffHours = Math.floor(diffMins / 60);
    if (diffHours === 1) return "1 hour ago";
    if (diffHours < 24) return `${diffHours} hours ago`;
    
    return date.toLocaleDateString('en-US', { month: 'short', day: 'numeric', hour: 'numeric', minute: '2-digit' });
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-4 md:p-6">
      {/* Header with column labels */}
      <div className="flex flex-col md:flex-row items-start md:items-center justify-between py-3 border-b border-gray-200 mb-2 gap-2 md:gap-0">
        <div className="text-base md:text-lg font-medium text-gray-800 w-full md:w-auto">Location</div>
        <div className="flex items-center justify-between w-full md:w-auto space-x-2 md:space-x-4">
          <div className="flex items-center text-base md:text-lg text-gray-600 gap-1">
            <CloudRain className="w-4 h-4 md:w-5 md:h-5" />
            <span className="text-sm md:text-base">Rain</span>
          </div>
          <div className="flex items-center text-base md:text-lg text-gray-600 gap-1">
            <ThermometerSun className="w-4 h-4 md:w-5 md:h-5" />
            <span className="text-sm md:text-base">High</span>
          </div>
          <div className="flex items-center text-base md:text-lg text-gray-600 gap-1">
            <Droplets className="w-4 h-4 md:w-5 md:h-5" />
            <span className="text-sm md:text-base">Low</span>
          </div>
          <div className="flex items-center justify-center w-7 md:w-9">
            <Heart className="w-4 h-4 md:w-5 md:h-5 text-gray-400" />
          </div>
          <div className="flex items-center justify-center w-7 md:w-9">
            <MoreVertical className="w-4 h-4 md:w-5 md:h-5 text-gray-400" />
          </div>
        </div>
      </div>

      {/* Last synced indicator */}
      {lastSyncedTime && (
        <div className="text-xs text-gray-500 text-right mb-2">
          Last synced: {formatLastSynced(lastSyncedTime)}
        </div>
      )}

      {/* Location rows */}
      <div className="space-y-3 md:space-y-4">
        {filteredSummaries?.map((summary) => (
          <div key={summary.locationId} className="cursor-default">
            <WeatherRow
              day={summary.locationName}
              rain={`${Math.round(summary.rain)}%`}
              maxTemp={summary.maxTemp.toString()}
              minTemp={summary.minTemp.toString()}
              trackedLocationId={summary.id}
              locationId={summary.locationId}
              isFavorite={summary.isFavorite}
              userId={user?.id}
              showActions={true}
              locationName={summary.locationName}
              onHeartClick={() => handleFavoriteClick(summary.id)}
              onHourlyWeatherClick={onHourlyWeatherClick}
              onForecastClick={onForecastClick}
              onTodayClick={onTodayClick}
              onEditClick={handleEditClick}
              onRemoveClick={(locationId) => handleRemoveClick(locationId)}
              displayName={summary.displayName || summary.locationName}
              summary={summary.summary || "No summary available"}
            />
          </div>
        ))}
        {(!filteredSummaries || filteredSummaries.length === 0) && (
          <div className="text-center py-4 text-gray-500">
            <p className="text-sm md:text-base">
              {activeTab === "favorites"
                ? "No favorite locations yet. Mark a location as favorite to see it here."
                : "No locations tracked yet. Add a location to see weather forecasts."}
            </p>
          </div>
        )}
      </div>

      {/* Edit Modal */}
      <EditModal
        isOpen={isEditModalOpen}
        initialDisplayName={editingLocation?.displayName || ""}
        onSave={handleEditSave}
        onClose={handleEditModalClose}
      />
    </div>
  );
};

export default LocationsList;
