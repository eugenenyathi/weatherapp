import { useState } from 'react';

interface EditModalProps {
  isOpen: boolean;
  initialDisplayName: string;
  onSave: (displayName: string) => void;
  onClose: () => void;
}

const EditModal = ({ isOpen, initialDisplayName, onSave, onClose }: EditModalProps) => {
  const [displayName, setDisplayName] = useState(initialDisplayName);
  const [error, setError] = useState('');

  if (!isOpen) return null;

  const handleSave = () => {
    if (displayName.trim().length < 3) {
      setError('Display name must be at least 3 characters');
      return;
    }
    
    onSave(displayName);
  };

  return (
    <div className="fixed inset-0 bg-white bg-opacity-60 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-md p-6">
        <div className="flex justify-between items-center mb-4">
          <h2 className="text-xl font-bold text-gray-800">Edit Display Name</h2>
          <button
            onClick={onClose}
            className="text-gray-500 hover:text-gray-700"
          >
            ✕
          </button>
        </div>
        
        <div className="mb-4">
          <label htmlFor="displayName" className="block text-sm font-medium text-gray-700 mb-1">
            Display Name
          </label>
          <input
            id="displayName"
            type="text"
            value={displayName}
            onChange={(e) => {
              setDisplayName(e.target.value);
              if (error) setError(''); // Clear error when user types
            }}
            className={`w-full px-4 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-800 ${
              error ? 'border-red-500' : ''
            }`}
            placeholder="Enter display name"
          />
          {error && <p className="mt-1 text-sm text-red-600">{error}</p>}
          <p className="mt-1 text-xs text-gray-500">
            Display name must be at least 3 characters
          </p>
        </div>
        
        <div className="flex justify-end space-x-3">
          <button
            type="button"
            onClick={onClose}
            className="px-4 py-2 text-sm font-medium text-gray-700 bg-gray-100 rounded-md hover:bg-gray-200"
          >
            Cancel
          </button>
          <button
            type="button"
            onClick={handleSave}
            disabled={displayName.trim().length < 3}
            className={`px-4 py-2 text-sm font-medium text-white rounded-md ${
              displayName.trim().length >= 3
                ? 'bg-blue-600 hover:bg-blue-700'
                : 'bg-blue-400 cursor-not-allowed'
            }`}
          >
            Save
          </button>
        </div>
      </div>
    </div>
  );
};

export default EditModal;