'use client';

import { useAuth } from './AuthContext';
import { useRouter } from 'next/navigation';

interface HeaderProps {
  onAddLocationClick: () => void;
}

const Header = ({ onAddLocationClick }: HeaderProps) => {
  const { isLoggedIn, logout } = useAuth();
  const router = useRouter();

  const handleAddLocationButtonClick = () => {
    if (isLoggedIn) {
      onAddLocationClick();
    } else {
      // Redirect to login page
      router.push('/login');
    }
  };

  const handleUserIconClick = () => {
    if (isLoggedIn) {
      // Log out the user
      logout();
      router.push('/'); // Redirect to home after logout
    } else {
      // Navigate to login page
      router.push('/login');
    }
  };

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
          onClick={handleAddLocationButtonClick}
        >
          Add Location
        </button>
        <button 
          className="inline-flex items-center justify-center whitespace-nowrap rounded-sm px-3 py-1.5 text-sm font-medium transition-all bg-gray-100 text-gray-500 hover:bg-gray-200"
          onClick={handleUserIconClick}
        >
          {isLoggedIn ? 'Logout' : '👤'}
        </button>
      </div>
    </div>
  );
};

export default Header;