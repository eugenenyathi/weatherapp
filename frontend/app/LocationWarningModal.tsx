'use client';

import { Location } from './AddLocationModal'; // Import the Location interface
import { useRouter } from 'next/navigation';

interface LocationWarningModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  onCancel: () => void;
}

const LocationWarningModal = ({
  isOpen,
  onClose,
  onConfirm,
  onCancel
}: LocationWarningModalProps) => {
  const router = useRouter();

  if (!isOpen) return null;

  const handleGoToAuth = () => {
    onClose(); // Close the modals first
    router.push('/login'); // Then navigate to login
  };

  return (
    <div className="fixed inset-0 bg-white bg-opacity-60 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold text-gray-800">Notice</h2>
          <button
            onClick={onCancel}
            className="text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>

        <div className="mb-6">
          <p className="text-gray-700 mb-4">
            Any locations you add will only be saved temporarily. When you close your browser or clear your cookies, these locations will be lost.
          </p>
          <p className="text-gray-700">
            Sign up or log in to save your locations permanently.
          </p>
        </div>

        <div className="flex flex-col sm:flex-row gap-3">
          <button
            onClick={onConfirm}
            className="px-4 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 focus:outline-none focus:ring-2 focus:ring-blue-500 flex-1"
          >
            Add Location
          </button>
          <button
            onClick={handleGoToAuth} // Navigate to login page
            className="px-4 py-2 bg-gray-300 text-gray-800 rounded-md hover:bg-gray-400 focus:outline-none focus:ring-2 focus:ring-gray-500 flex-1"
          >
            Sign Up / Log In
          </button>
        </div>
      </div>
    </div>
  );
};

export default LocationWarningModal;