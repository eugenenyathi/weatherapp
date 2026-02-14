import WeatherComponent from './WeatherComponent';

export default function Home() {
  return (
    <div className="min-h-screen bg-gray-200 flex items-center justify-center">
      <div className="w-[70%]">
        <header className="flex items-center justify-between p-4">
          <div className="flex items-center space-x-2">
            {/* Weather icon - using a sun icon as placeholder */}
            <div className="text-4xl">☀️</div>
            <h1 className="text-4xl font-bold text-black">Weather</h1>
          </div>
        </header>
        
        <WeatherComponent />
      </div>
    </div>
  );
}
