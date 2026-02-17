import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { locationService } from '../services';

// Location hooks
export const useCreateLocation = () => {
  const queryClient = useQueryClient();
  
  return useMutation({
    mutationFn: (locationData: { name: string; latitude: number; longitude: number; country: string }) => 
      locationService.createLocation(locationData),
    onSuccess: () => {
      // Invalidate and refetch related queries if needed
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries'] });
    },
  });
};