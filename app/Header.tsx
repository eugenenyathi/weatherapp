const Header = () => {
  return (
    <div className="flex items-center justify-between p-4">
      <div className="flex items-center space-x-2">
        {/* Weather icon - using a sun icon as placeholder */}
        <div className="text-4xl">☀️</div>
        <h1 className="text-4xl font-bold text-black">Weather</h1>
      </div>
      <div className="flex items-center">
        {/* User icon - using a person icon as placeholder */}
        <div className="text-2xl">👤</div>
      </div>
    </div>
  );
};

export default Header;