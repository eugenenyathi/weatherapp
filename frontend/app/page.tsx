"use client";

import { useState } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { useAuth } from "./AuthContext";
import LocationsList from "./WeatherComponent";
import ForecastComponent from "./ForecastComponent";
import Tabs from "./Tabs";
import Header from "./Header";
import { AddLocationModal, LocationWarningModal, UserPreferenceModal, HourlyWeatherModal, TodayWeatherModal } from "./modals";
import { toast } from "sonner";
import { weatherService } from "./services";

// Create a client
const queryClient = new QueryClient();

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<{
    id: string;
    name: string;
  } | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isWarningModalOpen, setIsWarningModalOpen] = useState(false);
  const [isPreferenceModalOpen, setIsPreferenceModalOpen] = useState(false);
  const [isHourlyModalOpen, setIsHourlyModalOpen] = useState(false);
  const [isTodayModalOpen, setIsTodayModalOpen] = useState(false);
  const [activeTab, setActiveTab] = useState("favorites");
  const { isLoggedIn, user } = useAuth();
  
  // State for hourly and today modals
  const [selectedLocationForModal, setSelectedLocationForModal] = useState<{
    locationId: string;
    locationName: string;
    summary?: string;
    minTemp?: string;
    maxTemp?: string;
    rain?: string;
  } | null>(null);

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

  const handleRefreshClick = async () => {
    if (!user?.id) return;
    
    try {
      const result = await weatherService.refreshWeatherData(user.id);
      
      if (result.success) {
        // Invalidate and refetch all weather queries
        await queryClient.invalidateQueries({ queryKey: ['currentDaySummaries'] });
        await queryClient.invalidateQueries({ queryKey: ['fiveDayForecast'] });
        await queryClient.invalidateQueries({ queryKey: ['hourlyForecast'] });
        
        toast.success("Weather data refreshed successfully!");
      } else {
        toast.error(result.message || "Failed to refresh weather data");
      }
    } catch (error: any) {
      toast.error(error.message || "Failed to refresh weather data");
    }
  };

  const handleOpenPreferences = () => {
    setIsPreferenceModalOpen(true);
  };

  const handleClosePreferenceModal = () => {
    setIsPreferenceModalOpen(false);
  };

  const handleHourlyWeatherClick = (locationId: string, locationName: string) => {
    setSelectedLocationForModal({ locationId, locationName });
    setIsHourlyModalOpen(true);
  };

  const handleForecastClick = (locationId: string, locationName: string) => {
    setSelectedLocation({ id: locationId, name: locationName });
  };

  const handleTodayClick = (locationId: string, locationName: string, summary: string) => {
    setSelectedLocationForModal({ locationId, locationName, summary });
    setIsTodayModalOpen(true);
  };

  const handleCloseHourlyModal = () => {
    setIsHourlyModalOpen(false);
    setSelectedLocationForModal(null);
  };

  const handleCloseTodayModal = () => {
    setIsTodayModalOpen(false);
    setSelectedLocationForModal(null);
  };

  const handleLocationAdded = () => {
    // Switch to "all" tab after adding a new location
    setActiveTab("all");
  };

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-200">
        <Header onAddLocationClick={handleAddLocationClick} onRefreshClick={handleRefreshClick} onPreferencesClick={handleOpenPreferences} />
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
                  onTabChange={setActiveTab}
                  onHourlyWeatherClick={handleHourlyWeatherClick}
                  onForecastClick={handleForecastClick}
                  onTodayClick={handleTodayClick}
                />
              )}
            </Tabs>

            <AddLocationModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              onAdded={handleLocationAdded}
              userId={user?.id || ""}
            />

            <LocationWarningModal
              isOpen={isWarningModalOpen}
              onClose={handleCloseWarningModal}
              onConfirm={handleConfirmAddLocation}
              onCancel={handleCancelAddLocation}
            />

            <UserPreferenceModal
              isOpen={isPreferenceModalOpen}
              onClose={handleClosePreferenceModal}
              userId={user?.id || ""}
            />

            {selectedLocationForModal && (
              <>
                <HourlyWeatherModal
                  isOpen={isHourlyModalOpen}
                  onClose={handleCloseHourlyModal}
                  locationId={selectedLocationForModal.locationId}
                  locationName={selectedLocationForModal.locationName}
                  userId={user?.id || ""}
                />
                <TodayWeatherModal
                  isOpen={isTodayModalOpen}
                  onClose={handleCloseTodayModal}
                  locationName={selectedLocationForModal.locationName}
                  summary={selectedLocationForModal.summary || ""}
                />
              </>
            )}
          </div>
        </div>
      </div>
    </QueryClientProvider>
  );
}
