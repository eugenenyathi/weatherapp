'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Sun, Cloud, CloudRain, CloudSnow, CloudLightning, Wind } from "lucide-react";

interface TodayWeatherModalProps {
  isOpen: boolean;
  onClose: () => void;
  locationName: string;
  summary: string;
}

const getWeatherIcon = (summary: string) => {
  const lowerSummary = summary.toLowerCase();
  if (lowerSummary.includes('rain') || lowerSummary.includes('shower')) return <CloudRain className="w-16 h-16 text-blue-500" />;
  if (lowerSummary.includes('snow')) return <CloudSnow className="w-16 h-16 text-blue-300" />;
  if (lowerSummary.includes('thunder') || lowerSummary.includes('storm')) return <CloudLightning className="w-16 h-16 text-yellow-600" />;
  if (lowerSummary.includes('cloud')) return <Cloud className="w-16 h-16 text-gray-400" />;
  if (lowerSummary.includes('clear') || lowerSummary.includes('sunny')) return <Sun className="w-16 h-16 text-yellow-500" />;
  if (lowerSummary.includes('wind')) return <Wind className="w-16 h-16 text-gray-500" />;
  return <Sun className="w-16 h-16 text-yellow-500" />;
};

const TodayWeatherModal = ({
  isOpen,
  onClose,
  locationName,
  summary,
}: TodayWeatherModalProps) => {
  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Sun className="w-5 h-5" />
            Today's Weather - {locationName}
          </DialogTitle>
        </DialogHeader>

        <div className="py-8">
          <div className="text-center">
            {getWeatherIcon(summary)}
            <p className="text-lg font-medium text-gray-800 mt-4">{summary}</p>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default TodayWeatherModal;
