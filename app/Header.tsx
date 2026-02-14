interface HeaderProps {
  onAddLocationClick: () => void;
  onUserIconClick: () => void;
}

const Header = ({ onAddLocationClick, onUserIconClick }: HeaderProps) => {
  return (
    <div className="flex items-center justify-between p-4">
      <div className="flex items-center space-x-2">
        {/* Weather icon - using a sun icon as placeholder */}
        <div className="text-4xl">☀️</div>
        <h1 className="text-4xl font-bold text-black">Weather</h1>
      </div>
      <div className="flex items-center space-x-4">
        <button 
          className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
          onClick={onAddLocationClick}
        >
          Add Location
        </button>
        <button 
          className="text-2xl"
          onClick={onUserIconClick}
        >
          👤
        </button>
      </div>
    </div>
  );
};

export default Header;