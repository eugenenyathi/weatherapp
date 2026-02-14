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
          className="inline-flex items-center justify-center whitespace-nowrap rounded-sm px-3 py-1.5 text-sm font-medium transition-all bg-gray-100 text-gray-500 hover:bg-gray-200"
          onClick={onAddLocationClick}
        >
          Add Location
        </button>
        <button 
          className="inline-flex items-center justify-center whitespace-nowrap rounded-sm px-3 py-1.5 text-sm font-medium transition-all bg-gray-100 text-gray-500 hover:bg-gray-200"
          onClick={onUserIconClick}
        >
          👤
        </button>
      </div>
    </div>
  );
};

export default Header;