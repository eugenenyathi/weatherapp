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

interface Location {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
}

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<{ id: string; name: string } | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isWarningModalOpen, setIsWarningModalOpen] = useState(false);
  const { isLoggedIn, user } = useAuth(); // Get the authentication status and user info

  const handleSelectLocation = (locationId: string, locationName: string) => {
    setSelectedLocation({ id: locationId, name: locationName });
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
    // This will now be handled by the backend
  };

  const handleConfirmAddLocation = () => {
    // User acknowledged the warning, now open the location modal
    setIsWarningModalOpen(false);
    setIsModalOpen(true); // Open the AddLocationModal
  };

  const handleCancelAddLocation = () => {
    setIsWarningModalOpen(false);
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
            <Tabs defaultValue="all">
              {selectedLocation ? (
                <ForecastComponent
                  locationId={selectedLocation.id}
                  locationName={selectedLocation.name}
                  onBack={handleBack}
                />
              ) : (
                <LocationsList
                  onSelectLocation={handleSelectLocation}
                />
              )}
            </Tabs>

            <AddLocationModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              onAddLocation={handleAddLocation}
              userId={user?.id || ''}
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
