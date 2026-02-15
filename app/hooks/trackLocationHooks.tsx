import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { trackLocationService } from '../services';

// Track location hooks
export const useTrackedLocationByUserId = (userId: string) => {
  return useQuery({
    queryKey: ['trackedLocation', userId],
    queryFn: () => trackLocationService.getTrackedLocationByUserId(userId),
    enabled: !!userId,
  });
};

export const useCreateTrackLocation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ userId, trackLocationData }: { userId: string; trackLocationData: { locationId: string; isFavorite?: boolean; displayName?: string } }) => 
      trackLocationService.createTrackLocation(userId, trackLocationData),
    onSuccess: (_, variables) => {
      // Invalidate and refetch related queries
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['trackedLocation', variables.userId] });
    },
  });
};

export const useUpdateTrackLocation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ userId, trackedLocationId, trackLocationData }: { 
      userId: string; 
      trackedLocationId: string; 
      trackLocationData: { isFavorite?: boolean; displayName?: string } 
    }) => 
      trackLocationService.updateTrackLocation(userId, trackedLocationId, trackLocationData),
    onSuccess: (_, variables) => {
      // Invalidate and refetch related queries
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['trackedLocation', variables.userId] });
    },
  });
};

export const useDeleteTrackLocation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: ({ userId, trackedLocationId }: { userId: string; trackedLocationId: string }) => 
      trackLocationService.deleteTrackLocation(userId, trackedLocationId),
    onSuccess: (_, variables) => {
      // Invalidate and refetch related queries
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['trackedLocation', variables.userId] });
    },
  });
};