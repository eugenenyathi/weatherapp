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
import { userPreferenceService } from "../services/UserPreferenceService";
import { useUpdateUserPreference, useCreateUserPreference } from "../hooks/userPreferenceHooks";
import type { UserPreference, UserPreferenceRequest } from "../types";

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
  const updatePreference = useUpdateUserPreference();
  const createPreference = useCreateUserPreference();

  const [preferredUnit, setPreferredUnit] = useState<"Metric" | "Imperial">("Metric");
  const [refreshInterval, setRefreshInterval] = useState<string>("30");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [existingPreference, setExistingPreference] = useState<UserPreference | null>(null);

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
        setExistingPreference(preference);
        setPreferredUnit(preference.preferredUnit);
        setRefreshInterval(preference.refreshInterval.toString());
      } else {
        // No existing preference, set defaults for creation
        setExistingPreference(null);
        setPreferredUnit("Metric");
        setRefreshInterval("30");
      }
    } catch (err: any) {
      setError(err.message || "Failed to load preferences");
      // On error, assume no preference exists and use defaults
      setExistingPreference(null);
      setPreferredUnit("Metric");
      setRefreshInterval("30");
    } finally {
      setIsLoading(false);
    }
  };

  const handleSave = async () => {
    setError(null);

    const request: UserPreferenceRequest = {
      preferredUnit: preferredUnit,
      refreshInterval: parseInt(refreshInterval, 10),
    };

    try {
      if (existingPreference) {
        // Update existing preference
        await updatePreference.mutateAsync({
          userId,
          preferenceId: existingPreference.id,
          preferenceData: request,
        });
      } else {
        // Create new preference
        await createPreference.mutateAsync({
          userId,
          preferenceData: request,
        });
      }

      onClose();
    } catch (err: any) {
      setError(err.message || "Failed to save preferences");
    }
  };

  const isSaving = updatePreference.isPending || createPreference.isPending;

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
            disabled={isSaving}
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
