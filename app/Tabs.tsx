'use client';

import { useState } from 'react';

interface TabsProps {
  defaultValue: string;
  value?: string;
  children: React.ReactNode;
  onTabChange?: (tab: string) => void;
}

interface TabButtonProps {
  value: string;
  label: string;
  isActive: boolean;
  onClick: () => void;
}

const Tabs = ({ defaultValue, value, children, onTabChange }: TabsProps) => {
  const [internalActiveTab, setInternalActiveTab] = useState(defaultValue);
  const activeTab = value !== undefined ? value : internalActiveTab;

  const handleTabChange = (tab: string) => {
    setInternalActiveTab(tab);
    if (onTabChange) {
      onTabChange(tab);
    }
  };

  return (
    <div className="w-full">
      <div className="flex justify-center mb-4">
        <div className="inline-flex h-10 items-center justify-center rounded-md bg-gray-100 p-1 text-gray-500">
          <TabButton
            value="favorites"
            label="Favorites"
            isActive={activeTab === 'favorites'}
            onClick={() => handleTabChange('favorites')}
          />
          <TabButton
            value="all"
            label="All"
            isActive={activeTab === 'all'}
            onClick={() => handleTabChange('all')}
          />
        </div>
      </div>
      <div>
        {children}
      </div>
    </div>
  );
};

const TabButton = ({ value, label, isActive, onClick }: TabButtonProps) => {
  return (
    <button
      className={`inline-flex items-center justify-center whitespace-nowrap rounded-sm px-3 py-1.5 text-sm font-medium transition-all ${
        isActive
          ? 'bg-white text-gray-900 shadow-sm'
          : 'text-gray-500 hover:text-gray-700'
      }`}
      onClick={onClick}
    >
      {label}
    </button>
  );
};

export default Tabs;