import WeatherRow from './WeatherRow';

const WeatherComponent = () => {
  // Sample weather data
  const weatherData = [
    { day: 'Today', rain: '20%', maxTemp: '25', minTemp: '15' },
    { day: 'Monday', rain: '10%', maxTemp: '22', minTemp: '12' },
    { day: 'Tuesday', rain: '5%', maxTemp: '24', minTemp: '14' },
    { day: 'Wednesday', rain: '60%', maxTemp: '19', minTemp: '10' },
    { day: 'Thursday', rain: '30%', maxTemp: '21', minTemp: '13' },
    { day: 'Friday', rain: '5%', maxTemp: '26', minTemp: '16' },
  ];

  return (
    <div className="bg-white rounded-lg shadow-md p-6">
      {/* Weather forecast rows */}
      <div className="space-y-4">
        {weatherData.map((data, index) => (
          <WeatherRow 
            key={index} 
            day={data.day} 
            rain={data.rain} 
            maxTemp={data.maxTemp} 
            minTemp={data.minTemp} 
          />
        ))}
      </div>
    </div>
  );
};

export default WeatherComponent;