"use client";

import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useAuth } from "./AuthContext";
import LocationsList from "./WeatherComponent";
import ForecastComponent from "./ForecastComponent";
import Tabs from "./Tabs";
import Header from "./Header";
import AddLocationModal from "./AddLocationModal";
import LocationWarningModal from "./LocationWarningModal";

// Create a client
const queryClient = new QueryClient();

interface SavedLocation {
  id: string;
  name: string;
  lat: number;
  lon: number;
}

interface Location {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
}

const getLocationName = (
  locationId: string,
  savedLocations: SavedLocation[],
) => {
  const location = savedLocations.find((loc) => loc.id === locationId);
  return location?.name || "Unknown Location";
};

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isWarningModalOpen, setIsWarningModalOpen] = useState(false);
  const [savedLocations, setSavedLocations] = useState<SavedLocation[]>([]);
  const { isLoggedIn } = useAuth(); // Get the authentication status

  const handleSelectLocation = (locationId: string) => {
    setSelectedLocation(locationId);
  };

  const handleBack = () => {
    setSelectedLocation(null);
  };

  const handleAddLocationClick = () => {
    // Check if user is logged in
    if (!isLoggedIn) {
      // Show warning modal for unauthenticated users before opening the location modal
      setIsWarningModalOpen(true);
    } else {
      // If authenticated, open the location modal directly
      setIsModalOpen(true);
    }
  };

  const handleAddLocation = (location: Location) => {
    // Add location directly since user has already acknowledged the warning
    addLocationToState(location);
  };

  const addLocationToState = (location: Location) => {
    // Add the location to saved locations
    const newLocation: SavedLocation = {
      id: Date.now().toString(),
      name: location.name,
      lat: location.lat,
      lon: location.lon,
    };

    setSavedLocations((prev) => [...prev, newLocation]);
  };

  const handleConfirmAddLocation = () => {
    // User acknowledged the warning, now open the location modal
    setIsWarningModalOpen(false);
    setIsModalOpen(true); // Open the AddLocationModal
  };

  const handleCancelAddLocation = () => {
    setIsWarningModalOpen(false);
  };

  const handleRemoveLocation = (id: string) => {
    setSavedLocations((prev) => prev.filter((loc) => loc.id !== id));
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  const handleCloseWarningModal = () => {
    setIsWarningModalOpen(false);
    setIsModalOpen(false); // Close the main modal too when going to login
  };

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-200">
        <Header onAddLocationClick={handleAddLocationClick} />
        <div className="flex items-center justify-center pt-24">
          {" "}
          {/* Increased padding-top to account for fixed header with top-2 */}
          <div className="w-[70%]">
            {savedLocations.length > 0 ? (
              <Tabs defaultValue="all">
                {selectedLocation ? (
                  <ForecastComponent
                    locationName={getLocationName(
                      selectedLocation,
                      savedLocations,
                    )}
                    onBack={handleBack}
                  />
                ) : (
                  <LocationsList
                    onSelectLocation={handleSelectLocation}
                    savedLocations={savedLocations}
                  />
                )}
              </Tabs>
            ) : (
              <div className="p-8 text-center">
                <p className="text-gray-600">
                  Get started by adding your first location
                </p>
              </div>
            )}

            <AddLocationModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              onAddLocation={handleAddLocation}
              savedLocations={savedLocations}
              onRemoveLocation={handleRemoveLocation}
            />

            <LocationWarningModal
              isOpen={isWarningModalOpen}
              onClose={handleCloseWarningModal}
              onConfirm={handleConfirmAddLocation}
              onCancel={handleCancelAddLocation}
            />
          </div>
        </div>
      </div>
    </QueryClientProvider>
  );
}
