'use client';

import { useRouter } from 'next/navigation';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { AlertCircle } from "lucide-react";

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

  const handleGoToAuth = () => {
    onClose();
    router.push('/login');
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onCancel()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <AlertCircle className="w-5 h-5" />
            Notice
          </DialogTitle>
        </DialogHeader>

        <DialogDescription className="text-gray-700">
          <p className="mb-4">
            Any locations you add will only be saved temporarily. When you close your browser or clear your cookies, these locations will be lost.
          </p>
          <p>
            Sign up or log in to save your locations permanently.
          </p>
        </DialogDescription>

        <DialogFooter className="flex-col sm:flex-row gap-2">
          <Button onClick={onConfirm} className="flex-1">
            Add Location
          </Button>
          <Button variant="outline" onClick={handleGoToAuth} className="flex-1">
            Sign Up / Log In
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default LocationWarningModal;
