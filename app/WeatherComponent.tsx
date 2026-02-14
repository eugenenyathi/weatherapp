import WeatherRow from './WeatherRow';

const WeatherComponent = () => {
  // Sample weather data
  const weatherData = [
    { day: 'Today', rain: '20%', maxTemp: '25°C', minTemp: '15°C' },
    { day: 'Monday', rain: '10%', maxTemp: '22°C', minTemp: '12°C' },
    { day: 'Tuesday', rain: '5%', maxTemp: '24°C', minTemp: '14°C' },
    { day: 'Wednesday', rain: '60%', maxTemp: '19°C', minTemp: '10°C' },
    { day: 'Thursday', rain: '30%', maxTemp: '21°C', minTemp: '13°C' },
    { day: 'Friday', rain: '5%', maxTemp: '26°C', minTemp: '16°C' },
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