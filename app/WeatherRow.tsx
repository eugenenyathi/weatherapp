interface WeatherRowProps {
  day: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
}

const WeatherRow = ({ day, rain, maxTemp, minTemp }: WeatherRowProps) => {
  // Extract just the temperature values without the '°C'
  const maxTempValue = maxTemp.replace('°C', '');
  const minTempValue = minTemp.replace('°C', '');
  
  return (
    <div className="flex items-center justify-between py-2 border-b border-gray-100 last:border-0">
      <div className="text-lg font-medium text-gray-800">{day}</div>
      <div className="flex justify-between space-x-6">
        <div className="flex items-center text-lg text-gray-700">
          <span className="mr-1">🌧️</span>{rain}
        </div>
        <div className="text-lg text-gray-700">{maxTempValue}°</div>
        <div className="text-lg text-gray-700">{minTempValue}°</div>
      </div>
    </div>
  );
};

export default WeatherRow;