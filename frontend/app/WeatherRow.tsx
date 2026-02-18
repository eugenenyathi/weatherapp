import { useState } from 'react';
import { trackLocationService } from './services';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Heart, MoreVertical, Clock, Calendar, Sun, Trash2 } from "lucide-react";

interface WeatherRowProps {
  day: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
  trackedLocationId?: string;
  locationId?: string;
  isFavorite?: boolean;
  userId?: string;
  onFavoriteToggle?: (locationId: string, isFavorite: boolean) => void;
  showActions?: boolean;
  onHeartClick?: () => void;
  onHourlyWeatherClick?: (locationId: string, locationName: string) => void;
  onForecastClick?: (locationId: string, locationName: string) => void;
  onTodayClick?: (locationId: string, locationName: string, summary: string) => void;
  onEditClick?: () => void;
  onRemoveClick?: (locationId: string) => void;
  displayName?: string;
  locationName?: string;
  summary?: string;
}

const WeatherRow = ({
  day,
  rain,
  maxTemp,
  minTemp,
  trackedLocationId,
  locationId,
  isFavorite = false,
  userId,
  onFavoriteToggle,
  showActions = false,
  onHeartClick,
  onHourlyWeatherClick,
  onForecastClick,
  onTodayClick,
  onEditClick,
  onRemoveClick,
  displayName,
  locationName,
  summary,
}: WeatherRowProps) => {
  const maxTempValue = maxTemp.replace("°C", "");
  const minTempValue = minTemp.replace("°C", "");

  const handleFavoriteClick = async () => {
    if (onHeartClick) {
      onHeartClick();
      return;
    }

    if (!userId || !trackedLocationId) return;

    const newFavoriteStatus = !isFavorite;

    try {
      await trackLocationService.updateTrackLocation(
        userId,
        trackedLocationId,
        {
          isFavorite: newFavoriteStatus,
        },
      );

      if (onFavoriteToggle) {
        onFavoriteToggle(trackedLocationId, newFavoriteStatus);
      }
    } catch (error) {
      console.error("Error toggling favorite status:", error);
    }
  };

  const handleHourlyWeatherClick = () => {
    if (onHourlyWeatherClick && locationId && locationName) {
      onHourlyWeatherClick(locationId, locationName);
    }
  };

  const handleForecastClick = () => {
    if (onForecastClick && locationId && locationName) {
      onForecastClick(locationId, locationName);
    }
  };

  const handleTodayClick = () => {
    if (onTodayClick && locationId && locationName && summary) {
      onTodayClick(locationId, locationName, summary);
    }
  };

  const handleRemoveClick = async () => {
    if (!userId || !trackedLocationId) return;

    try {
      await trackLocationService.deleteTrackLocation(userId, trackedLocationId);

      if (onRemoveClick) {
        onRemoveClick(trackedLocationId);
      }
    } catch (error) {
      console.error('Error removing location:', error);
    }
  };

  const handleEditClick = () => {
    if (onEditClick) {
      onEditClick();
    }
  };

  return (
    <div className="flex flex-col md:flex-row items-start md:items-center justify-between py-3 border-b border-gray-100 last:border-0 gap-2 md:gap-0">
      <div className="text-base md:text-lg font-medium text-gray-800 w-full md:w-auto">{day}</div>
      <div className="flex items-center justify-between w-full md:w-auto space-x-2 md:space-x-4">
        <div className="flex items-center text-base md:text-lg text-gray-700">
          <span className="mr-1">🌧️</span>{rain}
        </div>
        <div className="text-base md:text-lg text-gray-700">{maxTempValue}°</div>
        <div className="text-base md:text-lg text-gray-700">{minTempValue}°</div>

        {showActions && (
          <div className="flex items-center space-x-2">
            <button
              onClick={handleFavoriteClick}
              className={`p-1 rounded-full transition-colors cursor-pointer ${
                isFavorite
                  ? 'text-red-500 hover:text-red-600'
                  : 'text-gray-400 hover:text-gray-600'
              }`}
              aria-label={isFavorite ? "Remove from favorites" : "Add to favorites"}
            >
              <Heart
                className="w-5 h-5"
                fill={isFavorite ? "currentColor" : "none"}
                stroke="currentColor"
              />
            </button>

            <DropdownMenu>
              <DropdownMenuTrigger asChild>
                <button
                  className="p-1 rounded-full text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
                  aria-label="More options"
                >
                  <MoreVertical className="w-5 h-5" />
                </button>
              </DropdownMenuTrigger>
              <DropdownMenuContent align="end">
                <DropdownMenuItem onClick={handleHourlyWeatherClick}>
                  <Clock className="w-4 h-4 mr-2" />
                  Hourly Weather
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleForecastClick}>
                  <Calendar className="w-4 h-4 mr-2" />
                  Forecast
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleTodayClick}>
                  <Sun className="w-4 h-4 mr-2" />
                  Today
                </DropdownMenuItem>
                <DropdownMenuItem
                  onClick={handleRemoveClick}
                  className="text-destructive focus:text-destructive focus:bg-destructive/10"
                >
                  <Trash2 className="w-4 h-4 mr-2" />
                  Remove
                </DropdownMenuItem>
              </DropdownMenuContent>
            </DropdownMenu>
          </div>
        )}
      </div>
    </div>
  );
};

export default WeatherRow;
