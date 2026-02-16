import WeatherRow from './WeatherRow';

interface ForecastDay {
  day: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
}

interface ForecastComponentProps {
  locationName: string;
  onBack: () => void;
}

const ForecastComponent = ({ locationName, onBack }: ForecastComponentProps) => {
  // Sample forecast data for the next 5 days
  const forecast: ForecastDay[] = [
    { day: 'Today', rain: '20%', maxTemp: '25', minTemp: '15' },
    { day: 'Tomorrow', rain: '10%', maxTemp: '22', minTemp: '12' },
    { day: 'Monday', rain: '5%', maxTemp: '24', minTemp: '14' },
    { day: 'Tuesday', rain: '60%', maxTemp: '19', minTemp: '10' },
    { day: 'Wednesday', rain: '30%', maxTemp: '21', minTemp: '13' },
  ];

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center justify-between mb-4">
        <h2 className="text-lg font-bold text-gray-800">{locationName} 5-Day Forecast</h2>
        <button 
          onClick={onBack}
          className="text-gray-600 hover:text-gray-900"
        >
          ← Back
        </button>
      </div>
      
      <div className="space-y-4">
        {forecast.map((day, index) => (
          <WeatherRow 
            key={index}
            day={day.day} 
            rain={day.rain} 
            maxTemp={day.maxTemp} 
            minTemp={day.minTemp} 
          />
        ))}
      </div>
    </div>
  );
};

export default ForecastComponent;