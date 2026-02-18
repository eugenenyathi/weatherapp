'use client';

import { useState, useEffect, useRef } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import axios from 'axios';
import { useCreateLocation } from './hooks/locationHooks';
import { useCreateTrackLocation } from './hooks/trackLocationHooks';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { X, Loader2, MapPin } from "lucide-react";

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
  const [isRefreshing, setIsRefreshing] = useState(false);
  const inputRef = useRef<HTMLInputElement>(null);
  const queryClient = useQueryClient();

  const createLocationMutation = useCreateLocation();
  const createTrackLocationMutation = useCreateTrackLocation();

  // Fetch location suggestions from OpenWeatherMap API
  const { data: suggestions = [], isLoading, error: geocodingError } = useQuery({
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
    enabled: !!searchTerm.trim(),
    staleTime: 5 * 60 * 1000,
    retry: 2,
  });

  useEffect(() => {
    if (isOpen && inputRef.current) {
      setTimeout(() => {
        inputRef.current?.focus();
      }, 100);
    }
  }, [isOpen]);

  const handleSelectLocation = async (location: Location) => {
    try {
      setIsRefreshing(true);

      const createdLocation = await createLocationMutation.mutateAsync({
        name: location.name,
        latitude: location.latitude,
        longitude: location.longitude,
        country: location.country
      });

      await createTrackLocationMutation.mutateAsync({
        userId,
        trackLocationData: {
          locationId: createdLocation.id,
          displayName: createdLocation.name
        }
      });

      await queryClient.refetchQueries({
        queryKey: ['currentDaySummaries', userId],
        type: 'active'
      });

      setIsRefreshing(false);
      onClose();
    } catch (error) {
      console.error('Error adding location:', error);
      setIsRefreshing(false);
    }
  };

  const isPending = createLocationMutation.isPending || createTrackLocationMutation.isPending || isRefreshing;

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <MapPin className="w-5 h-5" />
            Add Location
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-4">
          <Input
            ref={inputRef}
            type="text"
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            placeholder="Search for a city..."
            disabled={isPending}
          />

          {isPending && (
            <div className="flex items-center justify-center py-4 text-sm text-muted-foreground">
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              {isRefreshing ? 'Refreshing weather data...' : 'Adding location...'}
            </div>
          )}

          {!isPending && isLoading && (
            <div className="flex items-center justify-center py-4 text-sm text-muted-foreground">
              <Loader2 className="w-4 h-4 mr-2 animate-spin" />
              Searching...
            </div>
          )}

          {!isPending && !isLoading && suggestions.length > 0 && (
            <div className="border rounded-md max-h-48 overflow-y-auto">
              {suggestions.map((location: Location) => (
                <Button
                  key={location.id}
                  variant="ghost"
                  className="w-full justify-start h-auto py-2 px-3 hover:bg-gray-50"
                  onClick={() => handleSelectLocation(location)}
                >
                  <div className="flex flex-col items-start">
                    <span className="font-medium text-sm">{location.name}</span>
                    <span className="text-xs text-muted-foreground">{location.country}</span>
                  </div>
                </Button>
              ))}
            </div>
          )}

          {!isPending && !isLoading && searchTerm && suggestions.length === 0 && (
            <div className="text-center py-4 text-sm text-muted-foreground">
              No locations found
            </div>
          )}

          {geocodingError && !isRefreshing && (
            <div className="text-center py-2 text-sm text-destructive">
              Error searching locations: {geocodingError.message}
            </div>
          )}

          {createLocationMutation.error && (
            <div className="text-center py-2 text-sm text-destructive">
              Error: {createLocationMutation.error.message}
            </div>
          )}

          {createTrackLocationMutation.error && (
            <div className="text-center py-2 text-sm text-destructive">
              Error: {createTrackLocationMutation.error.message}
            </div>
          )}
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default AddLocationModal;
