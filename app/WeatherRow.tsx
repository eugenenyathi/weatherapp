interface WeatherRowProps {
  day: string;
  rain: string;
  maxTemp: string;
  minTemp: string;
}

const WeatherRow = ({ day, rain, maxTemp, minTemp }: WeatherRowProps) => {
  return (
    <div className="flex items-center justify-between py-3 border-b border-gray-100 last:border-0">
      <div className="text-xl font-medium text-gray-800">{day}</div>
      <div className="flex justify-between space-x-8">
        <div className="text-xl text-gray-700">{rain}</div>
        <div className="text-xl text-gray-700">{maxTemp}</div>
        <div className="text-xl text-gray-700">{minTemp}</div>
      </div>
    </div>
  );
};

export default WeatherRow;