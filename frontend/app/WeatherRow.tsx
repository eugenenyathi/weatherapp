import { useState } from 'react';
import { trackLocationService } from './services';
import DropdownMenu from './components/DropdownMenu';

interface WeatherRowProps {
  day: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
  trackedLocationId?: string;
  isFavorite?: boolean;
  userId?: string;
  onFavoriteToggle?: (locationId: string, isFavorite: boolean) => void;
  showActions?: boolean; // Whether to show heart and 3 dots menu
  onHeartClick?: () => void;
  onViewMoreClick?: () => void;
  onEditClick?: () => void;
  onRemoveClick?: (locationId: string) => void;
  displayName?: string;
}

const WeatherRow = ({
  day,
  rain,
  maxTemp,
  minTemp,
  trackedLocationId,
  isFavorite = false,
  userId,
  onFavoriteToggle,
  showActions = false,
  onHeartClick,
  onViewMoreClick,
  onEditClick,
  onRemoveClick,
  displayName,
}: WeatherRowProps) => {
  const [showMenu, setShowMenu] = useState(false);

  // Extract just the temperature values without the '°C'
  const maxTempValue = maxTemp.replace("°C", "");
  const minTempValue = minTemp.replace("°C", "");

  const handleFavoriteClick = async () => {
    if (onHeartClick) {
      onHeartClick();
      return;
    }

    if (!userId || !trackedLocationId) return;

    // Toggle the favorite status
    const newFavoriteStatus = !isFavorite;

    try {
      // Update the tracked location with the new favorite status
      await trackLocationService.updateTrackLocation(
        userId,
        trackedLocationId,
        {
          isFavorite: newFavoriteStatus,
        },
      );

      // Call the callback if provided
      if (onFavoriteToggle) {
        onFavoriteToggle(trackedLocationId, newFavoriteStatus);
      }
    } catch (error) {
      console.error("Error toggling favorite status:", error);
    }
  };

  const handleViewMoreClick = () => {
    if (onViewMoreClick) {
      onViewMoreClick();
    }
    setShowMenu(false);
  };

  const handleRemoveClick = async () => {
    if (!userId || !trackedLocationId) return;
    
    try {
      // Remove the tracked location
      await trackLocationService.deleteTrackLocation(userId, trackedLocationId);
      
      // Close the menu
      setShowMenu(false);
      
      // Call the callback if provided
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
    setShowMenu(false);
  };

  const closeMenu = () => {
    setShowMenu(false);
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
        
        {/* Heart and 3 dots menu section - only show if showActions is true */}
        {showActions && (
          <div className="flex items-center space-x-2">
            {/* Heart icon for favorite */}
            <button 
              onClick={handleFavoriteClick}
              className={`p-1 rounded-full ${isFavorite ? 'text-red-500' : 'text-gray-400 hover:text-gray-600'}`}
              aria-label={isFavorite ? "Remove from favorites" : "Add to favorites"}
            >
              {isFavorite ? (
                <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor" className="w-5 h-5">
                  <path d="M11.645 20.91l-.007-.003-.022-.012a15.247 15.247 0 01-.383-.218 25.18 25.18 0 01-4.244-3.17C4.688 15.36 2.25 12.174 2.25 8.25 2.25 5.322 4.714 3 7.688 3A5.5 5.5 0 0112 5.052 5.5 5.5 0 0116.313 3c2.973 0 5.437 2.322 5.437 5.25 0 3.925-2.438 7.111-4.739 9.256a25.175 25.175 0 01-4.244 3.17 15.247 15.247 0 01-.383.219l-.022.012-.007.004-.003.001a.752.752 0 01-.704 0l-.003-.001z" />
                </svg>
              ) : (
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" strokeWidth={1.5} stroke="currentColor" className="w-5 h-5">
                  <path strokeLinecap="round" strokeLinejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
                </svg>
              )}
            </button>

            {/* 3 dots menu */}
            <DropdownMenu
              trigger={
                <button 
                  className="p-1 rounded-full text-gray-400 hover:text-gray-600"
                  aria-label="More options"
                >
                  <svg
                    xmlns="http://www.w3.org/2000/svg"
                    fill="none"
                    viewBox="0 0 24 24"
                    strokeWidth={1.5}
                    stroke="currentColor"
                    className="w-5 h-5"
                  >
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      d="M12 6.75a.75.75 0 110-1.5.75.75 0 010 1.5zM12 12.75a.75.75 0 110-1.5.75.75 0 010 1.5zM12 18.75a.75.75 0 110-1.5.75.75 0 010 1.5z"
                    />
                  </svg>
                </button>
              }
              items={[
                {
                  label: 'View More',
                  onClick: handleViewMoreClick,
                  variant: 'default'
                },
                {
                  label: 'Remove',
                  onClick: handleRemoveClick,
                  variant: 'destructive'
                },
                {
                  label: 'Edit',
                  onClick: handleEditClick,
                  variant: 'default'
                }
              ]}
            />
          </div>
        )}
      </div>
    </div>
  );
};

export default WeatherRow;