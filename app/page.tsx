'use client';

import { useState } from 'react';
import LocationsList from './WeatherComponent';
import ForecastComponent from './ForecastComponent';
import Tabs from './Tabs';

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
        <header className="flex items-center justify-between p-4">
          <div className="flex items-center space-x-2">
            {/* Weather icon - using a sun icon as placeholder */}
            <div className="text-4xl">☀️</div>
            <h1 className="text-4xl font-bold text-black">Weather</h1>
          </div>
        </header>
        
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
