'use client';

import { useState, useEffect } from 'react';

interface Location {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
}

interface AddLocationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAddLocation: (location: Location) => void;
}

const AddLocationModal = ({ isOpen, onClose, onAddLocation }: AddLocationModalProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const [suggestions, setSuggestions] = useState<Location[]>([]);
  const [isLoading, setIsLoading] = useState(false);

  // Mock data for location suggestions
  const mockLocations: Location[] = [
    { id: '1', name: 'New York', country: 'US', lat: 40.7128, lon: -74.0060 },
    { id: '2', name: 'London', country: 'UK', lat: 51.5074, lon: -0.1278 },
    { id: '3', name: 'Tokyo', country: 'JP', lat: 35.6762, lon: 139.6503 },
    { id: '4', name: 'Sydney', country: 'AU', lat: -33.8688, lon: 151.2093 },
    { id: '5', name: 'Paris', country: 'FR', lat: 48.8566, lon: 2.3522 },
    { id: '6', name: 'Berlin', country: 'DE', lat: 52.5200, lon: 13.4050 },
    { id: '7', name: 'Rome', country: 'IT', lat: 41.9028, lon: 12.4964 },
    { id: '8', name: 'Madrid', country: 'ES', lat: 40.4168, lon: -3.7038 },
  ];

  useEffect(() => {
    if (searchTerm.trim() === '') {
      setSuggestions([]);
      return;
    }

    setIsLoading(true);
    
    // Simulate API call delay
    const timer = setTimeout(() => {
      const filtered = mockLocations.filter(location => 
        location.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
        location.country.toLowerCase().includes(searchTerm.toLowerCase())
      );
      setSuggestions(filtered);
      setIsLoading(false);
    }, 300);

    return () => clearTimeout(timer);
  }, [searchTerm]);

  const handleSelectLocation = (location: Location) => {
    onAddLocation(location);
    setSearchTerm('');
    setSuggestions([]);
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold text-gray-800">Add Location</h2>
          <button 
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>
        
        <div className="mb-4">
          <input
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search for a city..."
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
          />
        </div>
        
        {isLoading && (
          <div className="text-center py-4">
            <p>Loading...</p>
          </div>
        )}
        
        {!isLoading && suggestions.length > 0 && (
          <div className="border border-gray-200 rounded-md max-h-60 overflow-y-auto">
            {suggestions.map((location) => (
              <div
                key={location.id}
                className="p-3 border-b border-gray-200 hover:bg-gray-50 cursor-pointer"
                onClick={() => handleSelectLocation(location)}
              >
                <div className="font-medium">{location.name}</div>
                <div className="text-sm text-gray-500">{location.country}</div>
              </div>
            ))}
          </div>
        )}
        
        {!isLoading && searchTerm && suggestions.length === 0 && (
          <div className="text-center py-4 text-gray-500">
            No locations found
          </div>
        )}
      </div>
    </div>
  );
};

export default AddLocationModal;