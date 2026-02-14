import WeatherRow from './WeatherRow';

interface ForecastDay {
  day: string;
  date: string;
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
    { day: 'Today', date: 'Feb 14', rain: '20%', maxTemp: '25', minTemp: '15' },
    { day: 'Tomorrow', date: 'Feb 15', rain: '10%', maxTemp: '22', minTemp: '12' },
    { day: 'Monday', date: 'Feb 16', rain: '5%', maxTemp: '24', minTemp: '14' },
    { day: 'Tuesday', date: 'Feb 17', rain: '60%', maxTemp: '19', minTemp: '10' },
    { day: 'Wednesday', date: 'Feb 18', rain: '30%', maxTemp: '21', minTemp: '13' },
  ];

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      <div className="flex items-center mb-4">
        <button 
          onClick={onBack}
          className="mr-3 text-gray-600 hover:text-gray-900"
        >
          ← Back
        </button>
        <h2 className="text-xl font-bold text-gray-800">{locationName} 5-Day Forecast</h2>
      </div>
      
      <div className="space-y-4">
        {forecast.map((day, index) => (
          <WeatherRow 
            key={index}
            day={`${day.day} (${day.date})`} 
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