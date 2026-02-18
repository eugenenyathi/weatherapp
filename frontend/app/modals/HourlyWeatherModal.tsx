'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Clock, Loader2, Droplets } from "lucide-react";
import { useHourlyForecast } from "../hooks/weatherHooks";

interface HourlyWeatherModalProps {
  isOpen: boolean;
  onClose: () => void;
  locationId: string;
  locationName: string;
  userId: string;
}

const HourlyWeatherModal = ({
  isOpen,
  onClose,
  locationId,
  locationName,
  userId,
}: HourlyWeatherModalProps) => {
  const { data: hourlyData, isLoading, error } = useHourlyForecast(locationId, userId);

  const formatHour = (dateTime: string) => {
    const date = new Date(dateTime);
    return date.toLocaleTimeString('en-US', { 
      hour: 'numeric',
      hour12: true 
    });
  };

  const formatDate = (dateTime: string) => {
    const date = new Date(dateTime);
    const today = new Date();
    const tomorrow = new Date(today);
    tomorrow.setDate(tomorrow.getDate() + 1);

    if (date.toDateString() === today.toDateString()) {
      return 'Today';
    } else if (date.toDateString() === tomorrow.toDateString()) {
      return 'Tomorrow';
    } else {
      return date.toLocaleDateString('en-US', { weekday: 'short', month: 'short', day: 'numeric' });
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md max-h-[80vh] overflow-hidden flex flex-col">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Clock className="w-5 h-5" />
            Hourly Weather - {locationName}
          </DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
            <span className="ml-2 text-sm text-muted-foreground">Loading hourly forecast...</span>
          </div>
        ) : error ? (
          <div className="text-center py-8">
            <p className="text-sm text-destructive">Error loading hourly forecast: {error.message}</p>
          </div>
        ) : (
          <div className="overflow-y-auto flex-1 -mx-6 px-6">
            <div className="space-y-2">
              {hourlyData?.hourlyForecasts.map((hour, index) => (
                <div
                  key={index}
                  className="flex items-center justify-between p-3 rounded-lg bg-gray-50 hover:bg-gray-100 transition-colors"
                >
                  <div className="flex items-center gap-3">
                    <div className="text-sm font-medium text-gray-600">
                      {formatDate(hour.dateTime)}
                    </div>
                    <div className="text-base font-semibold text-gray-800">
                      {formatHour(hour.dateTime)}
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <div className="text-lg font-bold text-gray-900">
                      {Math.round(hour.temp)}°
                    </div>
                    <div className="flex items-center gap-1 text-sm text-gray-600">
                      <Droplets className="w-4 h-4" />
                      {Math.round(hour.humidity)}%
                    </div>
                  </div>
                </div>
              ))}
            </div>
          </div>
        )}
      </DialogContent>
    </Dialog>
  );
};

export default HourlyWeatherModal;
