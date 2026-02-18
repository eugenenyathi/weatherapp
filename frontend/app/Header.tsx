"use client";

import { useAuth } from "./AuthContext";
import { useRouter } from "next/navigation";
import { Button } from "@/components/ui/button";
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { Sun, RefreshCw, MapPinPlus, User, Settings, LogOut } from "lucide-react";

interface HeaderProps {
  onAddLocationClick: () => void;
  onRefreshClick?: () => void;
  onPreferencesClick?: () => void;
}

const Header = ({ onAddLocationClick, onRefreshClick, onPreferencesClick }: HeaderProps) => {
  const { isLoggedIn, user, logout } = useAuth();
  const router = useRouter();

  const handleAddLocationButtonClick = () => {
    onAddLocationClick();
  };

  const handlePreferencesClick = () => {
    if (onPreferencesClick) {
      onPreferencesClick();
    }
  };

  const handleLogoutClick = () => {
    logout();
    router.push("/");
  };

  const handleLoginClick = () => {
    router.push("/login");
  };

  const handleRegisterClick = () => {
    router.push("/register");
  };

  return (
    <div className="fixed top-2 left-1/2 transform -translate-x-1/2 w-full max-w-4xl z-10 flex items-center justify-between py-4 px-4">
      <div className="flex items-center space-x-2">
        <Sun className="w-6 h-6 md:w-8 md:h-8 lg:w-10 lg:h-10 text-yellow-500" />
        <h1 className="text-lg md:text-xl lg:text-4xl font-bold text-black">Weather</h1>
      </div>
      <div className="flex items-center space-x-2 md:space-x-4">
        {/* Refresh button - hidden on mobile, shown on larger screens */}
        <Button
          variant="outline"
          size="sm"
          className="hidden md:inline-flex cursor-pointer"
          onClick={onRefreshClick}
          title="Refresh"
        >
          <RefreshCw className="w-4 h-4" />
        </Button>
        {/* Refresh icon - shown only on mobile */}
        <Button
          variant="outline"
          size="icon"
          className="md:hidden cursor-pointer"
          onClick={onRefreshClick}
          title="Refresh"
        >
          <RefreshCw className="w-4 h-4" />
        </Button>
        {/* Add Location button - shown only on larger screens */}
        <Button
          variant="outline"
          size="sm"
          className="hidden md:inline-flex cursor-pointer"
          onClick={handleAddLocationButtonClick}
        >
          <MapPinPlus className="w-4 h-4 mr-2" />
          Add Location
        </Button>
        {/* Add Location icon - shown only on mobile */}
        <Button
          variant="outline"
          size="icon"
          className="md:hidden cursor-pointer"
          onClick={handleAddLocationButtonClick}
          title="Add Location"
        >
          <MapPinPlus className="w-4 h-4" />
        </Button>
        {/* User dropdown - shown on all screens */}
        <DropdownMenu>
          <DropdownMenuTrigger asChild>
            <Button variant="outline" size="sm" className="cursor-pointer">
              {isLoggedIn ? (
                <>
                  <User className="w-4 h-4 mr-2" />
                  {user?.name}
                </>
              ) : (
                <User className="w-4 h-4" />
              )}
            </Button>
          </DropdownMenuTrigger>
          <DropdownMenuContent align="end" className="w-40">
            {isLoggedIn ? (
              <>
                <DropdownMenuItem onClick={handlePreferencesClick}>
                  <Settings className="w-4 h-4 mr-2" />
                  Preferences
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleLogoutClick}>
                  <LogOut className="w-4 h-4 mr-2" />
                  Logout
                </DropdownMenuItem>
              </>
            ) : (
              <>
                <DropdownMenuItem onClick={handleLoginClick}>
                  <User className="w-4 h-4 mr-2" />
                  Login
                </DropdownMenuItem>
                <DropdownMenuItem onClick={handleRegisterClick}>
                  <Settings className="w-4 h-4 mr-2" />
                  Register
                </DropdownMenuItem>
              </>
            )}
          </DropdownMenuContent>
        </DropdownMenu>
      </div>
    </div>
  );
};

export default Header;
