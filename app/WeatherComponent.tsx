import WeatherRow from './WeatherRow';

interface SavedLocation {
  id: string;
  name: string;
  lat: number;
  lon: number;
}

interface LocationsListProps {
  onSelectLocation: (locationId: string) => void;
  savedLocations: SavedLocation[];
}

const LocationsList = ({ onSelectLocation, savedLocations }: LocationsListProps) => {
  // Generate sample weather data for the saved locations
  const locationsWithWeather = savedLocations.map((location, index) => ({
    id: location.id,
    name: location.name,
    rain: `${Math.floor(Math.random() * 60)}%`, // Random rain percentage
    maxTemp: `${Math.floor(15 + Math.random() * 15)}`, // Random max temp between 15-30
    minTemp: `${Math.floor(5 + Math.random() * 10)}`  // Random min temp between 5-15
  }));

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Location rows */}
      <div className="space-y-4">
        {locationsWithWeather.map((location) => (
          <div
            key={location.id}
            onClick={() => onSelectLocation(location.id)}
            className="cursor-pointer"
          >
            <WeatherRow
              day={location.name}
              rain={location.rain}
              maxTemp={location.maxTemp}
              minTemp={location.minTemp}
            />
          </div>
        ))}
      </div>
    </div>
  );
};

export default LocationsList;