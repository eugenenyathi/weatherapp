'use client';

import { useState, useEffect } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Input } from "@/components/ui/input";
import { Settings, Loader2 } from "lucide-react";
import {
  userPreferenceService,
  UserPreference,
  UserPreferenceRequest,
} from "./services/UserPreferenceService";

interface UserPreferenceModalProps {
  isOpen: boolean;
  onClose: () => void;
  userId: string;
}

const UserPreferenceModal = ({
  isOpen,
  onClose,
  userId,
}: UserPreferenceModalProps) => {
  const [preferredUnit, setPreferredUnit] = useState<"Metric" | "Imperial" | "">("");
  const [refreshInterval, setRefreshInterval] = useState<string>("");
  const [isLoading, setIsLoading] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen && userId) {
      fetchUserPreference();
    }
  }, [isOpen, userId]);

  const fetchUserPreference = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const preference = await userPreferenceService.getUserPreference(userId);
      if (preference) {
        setPreferredUnit(preference.preferredUnit);
        setRefreshInterval(preference.refreshInterval.toString());
      } else {
        // No existing preference, set defaults
        setPreferredUnit("Metric");
        setRefreshInterval("30");
      }
    } catch (err: any) {
      setError(err.message || "Failed to load preferences");
      // Set defaults on error
      setPreferredUnit("Metric");
      setRefreshInterval("30");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    setIsSaving(true);
    setError(null);

    const request: UserPreferenceRequest = {
      ...(preferredUnit && { preferredUnit: preferredUnit as "Metric" | "Imperial" }),
      ...(refreshInterval && { refreshInterval: parseInt(refreshInterval, 10) }),
    };

    try {
      const preference = await userPreferenceService.getUserPreference(userId);
      
      if (preference) {
        await userPreferenceService.updateUserPreference(userId, preference.id, request);
      } else {
        await userPreferenceService.createUserPreference(userId, request);
      }
      
      onClose();
    } catch (err: any) {
      setError(err.message || "Failed to save preferences");
    } finally {
      setIsSaving(false);
    }
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Settings className="w-5 h-5" />
            User Preferences
          </DialogTitle>
        </DialogHeader>

        {isLoading ? (
          <div className="flex items-center justify-center py-8">
            <Loader2 className="w-6 h-6 animate-spin text-muted-foreground" />
            <span className="ml-2 text-sm text-muted-foreground">Loading preferences...</span>
          </div>
        ) : (
          <div className="space-y-4 py-4">
            <div className="space-y-2">
              <Label htmlFor="preferredUnit">Preferred Unit</Label>
              <Select
                value={preferredUnit}
                onValueChange={(value: "Metric" | "Imperial") => setPreferredUnit(value)}
              >
                <SelectTrigger id="preferredUnit">
                  <SelectValue placeholder="Select unit" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="Metric">Metric (°C)</SelectItem>
                  <SelectItem value="Imperial">Imperial (°F)</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div className="space-y-2">
              <Label htmlFor="refreshInterval">Refresh Interval (minutes)</Label>
              <Input
                id="refreshInterval"
                type="number"
                min="1"
                max="120"
                value={refreshInterval}
                onChange={(e) => setRefreshInterval(e.target.value)}
                placeholder="30"
              />
              <p className="text-xs text-muted-foreground">
                How often to refresh weather data (1-120 minutes)
              </p>
            </div>

            {error && (
              <div className="text-sm text-destructive">{error}</div>
            )}
          </div>
        )}

        <DialogFooter>
          <Button variant="outline" onClick={onClose} disabled={isSaving}>
            Cancel
          </Button>
          <Button 
            onClick={handleSave} 
            disabled={isSaving || !preferredUnit || !refreshInterval}
          >
            {isSaving ? (
              <>
                <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                Saving...
              </>
            ) : (
              "Save"
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default UserPreferenceModal;
