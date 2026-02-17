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

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<{
    id: string;
    name: string;
  } | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isWarningModalOpen, setIsWarningModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("favorites");
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

  const handleRefreshClick = () => {
    // Refetch all active queries to refresh weather data
    queryClient.refetchQueries({ type: 'active' });
  };

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-200">
        <Header onAddLocationClick={handleAddLocationClick} onRefreshClick={handleRefreshClick} />
        <div className="flex items-center justify-center pt-24 w-full px-4">
          <div className="w-full max-w-4xl">
            <Tabs defaultValue="favorites" value={activeTab} onTabChange={setActiveTab}>
              {selectedLocation ? (
                <ForecastComponent
                  locationId={selectedLocation.id}
                  locationName={selectedLocation.name}
                  onBack={handleBack}
                />
              ) : (
                <LocationsList
                  onSelectLocation={handleSelectLocation}
                  activeTab={activeTab}
                  onTabChange={setActiveTab} // Pass the setActiveTab function to update the active tab
                />
              )}
            </Tabs>

            <AddLocationModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              userId={user?.id || ""}
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
