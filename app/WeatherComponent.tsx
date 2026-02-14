import WeatherRow from './WeatherRow';

interface Location {
  id: string;
  name: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
}

interface LocationsListProps {
  onSelectLocation: (locationId: string) => void;
}

const LocationsList = ({ onSelectLocation }: LocationsListProps) => {
  // Sample location data
  const locations: Location[] = [
    { id: '1', name: 'New York', rain: '20%', maxTemp: '25', minTemp: '15' },
    { id: '2', name: 'London', rain: '40%', maxTemp: '18', minTemp: '10' },
    { id: '3', name: 'Tokyo', rain: '10%', maxTemp: '22', minTemp: '16' },
    { id: '4', name: 'Sydney', rain: '5%', maxTemp: '24', minTemp: '17' },
    { id: '5', name: 'Paris', rain: '30%', maxTemp: '20', minTemp: '12' },
  ];

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Location rows */}
      <div className="space-y-4">
        {locations.map((location, index) => (
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