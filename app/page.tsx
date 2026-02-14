'use client';

import { useState } from 'react';
import LocationsList from './WeatherComponent';
import ForecastComponent from './ForecastComponent';
import Tabs from './Tabs';
import Header from './Header';

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

  const handleSelectLocation = (locationId: string) => {
    setSelectedLocation(locationId);
  };

  const handleBack = () => {
    setSelectedLocation(null);
  };

  return (
    <div className="min-h-screen bg-gray-200 flex items-center justify-center">
      <div className="w-[70%]">
        <Header />
        
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
      </div>
    </div>
  );
}
