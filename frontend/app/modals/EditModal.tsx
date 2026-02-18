import { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Pencil } from "lucide-react";

interface EditModalProps {
  isOpen: boolean;
  initialDisplayName: string;
  onSave: (displayName: string) => void;
  onClose: () => void;
}

const EditModal = ({ isOpen, initialDisplayName, onSave, onClose }: EditModalProps) => {
  const [displayName, setDisplayName] = useState(initialDisplayName);
  const [error, setError] = useState('');

  const handleSave = () => {
    if (displayName.trim().length < 3) {
      setError('Display name must be at least 3 characters');
      return;
    }

    onSave(displayName);
  };

  return (
    <Dialog open={isOpen} onOpenChange={(open) => !open && onClose()}>
      <DialogContent className="sm:max-w-md">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2">
            <Pencil className="w-5 h-5" />
            Edit Display Name
          </DialogTitle>
        </DialogHeader>

        <div className="space-y-2">
          <Label htmlFor="displayName">Display Name</Label>
          <Input
            id="displayName"
            type="text"
            value={displayName}
            onChange={(e) => {
              setDisplayName(e.target.value);
              if (error) setError('');
            }}
            className={error ? 'border-destructive' : ''}
            placeholder="Enter display name"
          />
          {error && <p className="text-sm text-destructive">{error}</p>}
          <p className="text-xs text-muted-foreground">
            Display name must be at least 3 characters
          </p>
        </div>

        <DialogFooter>
          <Button variant="outline" onClick={onClose}>
            Cancel
          </Button>
          <Button
            onClick={handleSave}
            disabled={displayName.trim().length < 3}
          >
            Save
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};

export default EditModal;
