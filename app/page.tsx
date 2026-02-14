'use client';

import { useState } from 'react';
import LocationsList from './WeatherComponent';
import ForecastComponent from './ForecastComponent';
import Tabs from './Tabs';
import Header from './Header';
import AddLocationModal from './AddLocationModal';

interface Location {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
}

const getLocationName = (locationId: string) => {
  const locations: { [key: string]: string } = {
    '1': 'New York',
    '2': 'London',
    '3': 'Tokyo',
    '4': 'Sydney',
    '5': 'Paris',
  };
  return locations[locationId] || 'Unknown Location';
};

export default function Home() {
  const [selectedLocation, setSelectedLocation] = useState<string | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);

  const handleSelectLocation = (locationId: string) => {
    setSelectedLocation(locationId);
  };

  const handleBack = () => {
    setSelectedLocation(null);
  };

  const handleAddLocationClick = () => {
    setIsModalOpen(true);
  };

  const handleUserIconClick = () => {
    alert('User profile clicked!');
  };

  const handleAddLocation = (location: Location) => {
    alert(`Added location: ${location.name}`);
    setIsModalOpen(false);
    // In a real app, you would add the location to your state/data
  };

  const handleCloseModal = () => {
    setIsModalOpen(false);
  };

  return (
    <div className="min-h-screen bg-gray-200 flex items-center justify-center">
      <div className="w-[70%]">
        <Header 
          onAddLocationClick={handleAddLocationClick}
          onUserIconClick={handleUserIconClick}
        />
        
        <Tabs defaultValue="all">
          {selectedLocation ? (
            <ForecastComponent 
              locationName={getLocationName(selectedLocation)} 
              onBack={handleBack} 
            />
          ) : (
            <LocationsList onSelectLocation={handleSelectLocation} />
          )}
        </Tabs>
        
        <AddLocationModal 
          isOpen={isModalOpen}
          onClose={handleCloseModal}
          onAddLocation={handleAddLocation}
        />
      </div>
    </div>
  );
}
