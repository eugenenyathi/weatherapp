'use client';

import { useState } from 'react';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import LocationsList from './WeatherComponent';
import ForecastComponent from './ForecastComponent';
import Tabs from './Tabs';
import Header from './Header';
import AddLocationModal from './AddLocationModal';

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

const getLocationName = (locationId: string, savedLocations: SavedLocation[]) => {
  const location = savedLocations.find(loc => loc.id === locationId);
  return location?.name || 'Unknown Location';
};

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [savedLocations, setSavedLocations] = useState<SavedLocation[]>([]);

  const handleSelectLocation = (locationId: string) => {
    setSelectedLocation(locationId);
  };

  const handleBack = () => {
    setSelectedLocation(null);
  };

  const handleAddLocationClick = () => {
    setIsModalOpen(true);
  };


  const handleAddLocation = (location: Location) => {
    // Add the location to saved locations
    const newLocation: SavedLocation = {
      id: Date.now().toString(),
      name: location.name,
      lat: location.lat,
      lon: location.lon
    };

    setSavedLocations(prev => [...prev, newLocation]);
  };

  const handleRemoveLocation = (id: string) => {
    setSavedLocations(prev => prev.filter(loc => loc.id !== id));
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  return (
    <QueryClientProvider client={queryClient}>
      <div className="min-h-screen bg-gray-200">
        <Header onAddLocationClick={() => setIsModalOpen(true)} />
        <div className="flex items-center justify-center pt-24"> {/* Increased padding-top to account for fixed header with top-2 */}
          <div className="w-[70%]">
            {savedLocations.length > 0 ? (
              <Tabs defaultValue="all">
                {selectedLocation ? (
                  <ForecastComponent
                    locationName={getLocationName(selectedLocation, savedLocations)}
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
                <p className="text-gray-600">Get started by adding your first location</p>
              </div>
            )}

            <AddLocationModal
              isOpen={isModalOpen}
              onClose={handleCloseModal}
              onAddLocation={handleAddLocation}
              savedLocations={savedLocations}
              onRemoveLocation={handleRemoveLocation}
            />
          </div>
        </div>
      </div>
    </QueryClientProvider>
  );
}
