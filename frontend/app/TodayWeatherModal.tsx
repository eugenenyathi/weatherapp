'use client';

import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Sun, Droplets, Thermometer, Wind } from "lucide-react";

interface TodayWeatherModalProps {
  isOpen: boolean;
  onClose: () => void;
  locationName: string;
  summary: string;
  minTemp: string;
  maxTemp: string;
  rain: string;
}

const TodayWeatherModal = ({
  isOpen,
  onClose,
  locationName,
  summary,
  minTemp,
  maxTemp,
  rain,
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

        <div className="py-4">
          <div className="text-center mb-6">
            <Sun className="w-16 h-16 mx-auto text-yellow-500 mb-2" />
            <p className="text-lg font-medium text-gray-800">{summary}</p>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <div className="flex items-center gap-3 p-3 rounded-lg bg-gray-50">
              <Thermometer className="w-5 h-5 text-gray-600" />
              <div>
                <p className="text-xs text-gray-500">High</p>
                <p className="text-lg font-semibold text-gray-800">{maxTemp}</p>
              </div>
            </div>

            <div className="flex items-center gap-3 p-3 rounded-lg bg-gray-50">
              <Thermometer className="w-5 h-5 text-gray-600" />
              <div>
                <p className="text-xs text-gray-500">Low</p>
                <p className="text-lg font-semibold text-gray-800">{minTemp}</p>
              </div>
            </div>

            <div className="flex items-center gap-3 p-3 rounded-lg bg-gray-50">
              <Droplets className="w-5 h-5 text-gray-600" />
              <div>
                <p className="text-xs text-gray-500">Rain Chance</p>
                <p className="text-lg font-semibold text-gray-800">{rain}</p>
              </div>
            </div>

            <div className="flex items-center gap-3 p-3 rounded-lg bg-gray-50">
              <Sun className="w-5 h-5 text-gray-600" />
              <div>
                <p className="text-xs text-gray-500">Condition</p>
                <p className="text-sm font-semibold text-gray-800">{summary}</p>
              </div>
            </div>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
};

export default TodayWeatherModal;
