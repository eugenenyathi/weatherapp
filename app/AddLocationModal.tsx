'use client';

import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { useCreateLocation } from './hooks/locationHooks';
import { useCreateTrackLocation } from './hooks/trackLocationHooks';

interface Location {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  country: string;
}

interface AddLocationModalProps {
  isOpen: boolean;
  onClose: () => void;
  userId: string;
}

const AddLocationModal = ({
  isOpen,
  onClose,
  userId
}: AddLocationModalProps) => {
  const [searchTerm, setSearchTerm] = useState('');
  const inputRef = useRef<HTMLInputElement>(null);
  const queryClient = useQueryClient();
  
  const createLocationMutation = useCreateLocation();
  const createTrackLocationMutation = useCreateTrackLocation();

  // Fetch location suggestions from OpenWeatherMap API
  const { data: suggestions = [], isLoading } = useQuery({
    queryKey: ['geocoding', searchTerm],
    queryFn: async () => {
      if (!searchTerm.trim()) return [];

      const API_KEY = process.env.NEXT_PUBLIC_OPENWEATHER_API_KEY;
      const response = await axios.get(
        `https://api.openweathermap.org/geo/1.0/direct?q=${encodeURIComponent(searchTerm)}&limit=10&appid=${API_KEY}`
      );

      // Transform the API response to match our Location interface
      return response.data.map((item: any, index: number) => ({
        id: `${item.lat}-${item.lon}-${index}`,
        name: item.name,
        latitude: item.lat,
        longitude: item.lon,
        country: item.country
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

  const handleSelectLocation = async (location: Location) => {
    try {
      // First, create the location in the backend
      const createdLocation = await createLocationMutation.mutateAsync({
        name: location.name,
        latitude: location.latitude,
        longitude: location.longitude,
        country: location.country
      });

      // Then, create a track location record for this user
      await createTrackLocationMutation.mutateAsync({
        userId,
        trackLocationData: {
          locationId: createdLocation.id,
          displayName: createdLocation.name
        }
      });

      // Invalidate and refetch the current day summaries to show the new location
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', userId] });

      // Close the modal
      onClose();
    } catch (error) {
      console.error('Error adding location:', error);
      // Optionally show an error message to the user
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-white bg-opacity-60 flex items-center justify-center z-50 p-4">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-4 md:p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-lg md:text-xl font-bold text-gray-800">Add Location</h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700 text-lg"
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
            className="w-full px-3 md:px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-800"
            disabled={createLocationMutation.isPending || createTrackLocationMutation.isPending}
          />
        </div>

        {(createLocationMutation.isPending || createTrackLocationMutation.isPending) && (
          <div className="text-center py-4">
            <p className="text-gray-700 text-sm md:text-base">Adding location...</p>
          </div>
        )}

        {!(createLocationMutation.isPending || createTrackLocationMutation.isPending) && isLoading && (
          <div className="text-center py-4">
            <p className="text-gray-700 text-sm md:text-base">Searching...</p>
          </div>
        )}

        {!(createLocationMutation.isPending || createTrackLocationMutation.isPending) && !isLoading && suggestions.length > 0 && (
          <div className="border border-gray-200 rounded-md max-h-48 md:max-h-60 overflow-y-auto mb-4">
            {suggestions.map((location) => (
              <div
                key={location.id}
                className="p-2 md:p-3 border-b border-gray-200 hover:bg-gray-50 cursor-pointer"
                onClick={() => handleSelectLocation(location)}
              >
                <div className="font-medium text-gray-800 text-sm md:text-base">{location.name}</div>
                <div className="text-xs md:text-sm text-gray-600">{location.country}</div>
              </div>
            ))}
          </div>
        )}

        {!(createLocationMutation.isPending || createTrackLocationMutation.isPending) && !isLoading && searchTerm && suggestions.length === 0 && (
          <div className="text-center py-4 text-gray-600 mb-4">
            <p className="text-sm md:text-base">No locations found</p>
          </div>
        )}

        {createLocationMutation.error && (
          <div className="text-center py-2 text-red-500 text-xs md:text-sm">
            Error: {createLocationMutation.error.message}
          </div>
        )}

        {createTrackLocationMutation.error && (
          <div className="text-center py-2 text-red-500 text-xs md:text-sm">
            Error: {createTrackLocationMutation.error.message}
          </div>
        )}
      </div>
    </div>
  );
};

export default AddLocationModal;