"use client";

import { useState } from "react";
import { useAuth } from "./AuthContext";
import { useCurrentDaySummaries } from "./hooks/weatherHooks";
import WeatherRow from "./WeatherRow";
import { trackLocationService } from "./services";
import EditModal from "./components/EditModal";

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

  const [removingIds, setRemovingIds] = useState<Set<string>>(new Set());
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [editingLocation, setEditingLocation] = useState<{ id: string; displayName: string } | null>(null);

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
      ? weatherSummaries?.filter((summary) => summary.isFavorite && !removingIds.has(summary.id))
      : weatherSummaries?.filter((summary) => !removingIds.has(summary.id));

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

  const handleRemoveClick = async (trackedLocationId: string) => {
    if (!user?.id || !trackedLocationId) return;

    // Optimistically update the UI by adding the ID to the removing set
    setRemovingIds(prev => new Set(prev).add(trackedLocationId));

    try {
      // Remove the tracked location
      await trackLocationService.deleteTrackLocation(user.id, trackedLocationId);

      // Refetch the data to update the UI permanently
      refetch();
    } catch (error) {
      console.error('Error removing location:', error);
      // If there's an error, remove the ID from the removing set
      setRemovingIds(prev => {
        const newSet = new Set(prev);
        newSet.delete(trackedLocationId);
        return newSet;
      });
    } finally {
      // Remove the ID from the removing set after the API call completes
      setRemovingIds(prev => {
        const newSet = new Set(prev);
        newSet.delete(trackedLocationId);
        return newSet;
      });
    }
  };

  const handleEditClick = (locationId: string, currentDisplayName: string) => {
    setEditingLocation({ id: locationId, displayName: currentDisplayName });
    setIsEditModalOpen(true);
  };

  const handleEditSave = async (newDisplayName: string) => {
    if (!editingLocation || !user?.id) return;

    try {
      // Update the tracked location with the new display name
      await trackLocationService.updateTrackLocation(
        user.id,
        editingLocation.id,
        { displayName: newDisplayName }
      );

      // Close the modal
      setIsEditModalOpen(false);
      setEditingLocation(null);

      // Refetch the data to update the UI
      refetch();
    } catch (error) {
      console.error('Error updating display name:', error);
    }
  };

  const handleEditModalClose = () => {
    setIsEditModalOpen(false);
    setEditingLocation(null);
  };

  return (
    <div className="bg-white rounded-lg shadow-md p-4 md:p-6">
      {/* Location rows */}
      <div className="space-y-3 md:space-y-4">
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
                handleEditClick(summary.id, summary.displayName || summary.locationName)
              }
              onRemoveClick={(locationId) => handleRemoveClick(locationId)}
              displayName={summary.displayName || summary.locationName}
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
        initialDisplayName={editingLocation?.displayName || ''}
        onSave={handleEditSave}
        onClose={handleEditModalClose}
      />
    </div>
  );
};

export default LocationsList;
