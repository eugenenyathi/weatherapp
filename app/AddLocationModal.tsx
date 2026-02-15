'use client';

import { useState, useEffect, useRef } from 'react';
import { useQuery } from '@tanstack/react-query';
import axios from 'axios';

interface Location {
  id: string;
  name: string;
  country: string;
  lat: number;
  lon: number;
}

interface SavedLocation {
  id: string;
  name: string;
  lat: number;
  lon: number;
}

interface AddLocationModalProps {
  isOpen: boolean;
  onClose: () => void;
  onAddLocation: (location: Location) => void;
  savedLocations: SavedLocation[];
  onRemoveLocation: (id: string) => void;
}

const AddLocationModal = ({
  isOpen,
  onClose,
  onAddLocation,
  savedLocations,
  onRemoveLocation
}: AddLocationModalProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);

  // Fetch location suggestions from OpenWeatherMap API
  const { data: suggestions = [], isLoading } = useQuery({
    queryKey: ['geocoding', searchTerm],
    queryFn: async () => {
      if (!searchTerm.trim()) return [];

      // Note: You'll need to replace 'YOUR_API_KEY' with an actual OpenWeatherMap API key
      const API_KEY = process.env.NEXT_PUBLIC_OPENWEATHER_API_KEY;
      const response = await axios.get(
        `https://api.openweathermap.org/geo/1.0/direct?q=${encodeURIComponent(searchTerm)}&limit=10&appid=${API_KEY}`
      );

      // Transform the API response to match our Location interface
      return response.data.map((item: any, index: number) => ({
        id: `${item.lat}-${item.lon}-${index}`, // Create a unique ID
        name: item.name,
        country: item.country,
        lat: item.lat,
        lon: item.lon
      }));
    },
    enabled: !!searchTerm.trim(), // Only run the query if searchTerm is not empty
    staleTime: 5 * 60 * 1000, // Cache for 5 minutes
  });

  // Focus the input when the modal opens
  useEffect(() => {
    if (isOpen && inputRef.current) {
      // Small delay to ensure the modal is rendered before focusing
      setTimeout(() => {
        inputRef.current?.focus();
      }, 100);
    }
  }, [isOpen]);

  const handleSelectLocation = (location: Location) => {
    onAddLocation(location);
    setSearchTerm('');
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-white bg-opacity-60 flex items-center justify-center z-50">
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
            ref={inputRef}
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search for a city..."
            className="w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-800"
          />
        </div>

        {isLoading && (
          <div className="text-center py-4">
            <p className="text-gray-700">Loading...</p>
          </div>
        )}

        {!isLoading && suggestions.length > 0 && (
          <div className="border border-gray-200 rounded-md max-h-60 overflow-y-auto mb-4">
            {suggestions.map((location) => (
              <div
                key={location.id}
                className="p-3 border-b border-gray-200 hover:bg-gray-50 cursor-pointer"
                onClick={() => handleSelectLocation(location)}
              >
                <div className="font-medium text-gray-800">{location.name}</div>
                <div className="text-sm text-gray-600">{location.country}</div>
              </div>
            ))}
          </div>
        )}

        {!isLoading && searchTerm && suggestions.length === 0 && (
          <div className="text-center py-4 text-gray-600 mb-4">
            No locations found
          </div>
        )}

        {/* Display saved locations */}
        <div>
          <h3 className="font-medium text-gray-800 mb-2">Added Locations</h3>
          {savedLocations.length === 0 ? (
            <p className="text-gray-600 text-sm">No locations added yet</p>
          ) : (
            <div className="space-y-2 max-h-40 overflow-y-auto">
              {savedLocations.map((location) => (
                <div
                  key={location.id}
                  className="flex justify-between items-center p-2 border border-gray-200 rounded-md"
                >
                  <span className="text-gray-700">{location.name}</span>
                  <button
                    onClick={() => onRemoveLocation(location.id)}
                    className="text-red-500 hover:text-red-700"
                  >
                    Remove
                  </button>
                </div>
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AddLocationModal;