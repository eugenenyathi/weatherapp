import { useQuery, useMutation } from '@tanstack/react-query';
import { userPreferenceService } from '../services';

// User preference hooks
export const useUserPreference = (userId: string) => {
  return useQuery({
    queryKey: ['userPreference', userId],
    queryFn: () => userPreferenceService.getUserPreference(userId),
    enabled: !!userId,
  });
};

export const useCreateUserPreference = () => {
  return useMutation({
    mutationFn: ({ userId, preferenceData }: { userId: string; preferenceData: { temperatureUnit: 'Metric' | 'Imperial' } }) => 
      userPreferenceService.createUserPreference(userId, preferenceData),
  });
};

export const useUpdateUserPreference = () => {
  return useMutation({
    mutationFn: ({ userId, preferenceId, preferenceData }: { 
      userId: string; 
      preferenceId: string; 
      preferenceData: { temperatureUnit: 'Metric' | 'Imperial' } 
    }) => 
      userPreferenceService.updateUserPreference(userId, preferenceId, preferenceData),
  });
};

export const useDeleteUserPreference = () => {
  return useMutation({
    mutationFn: ({ userId, preferenceId }: { userId: string; preferenceId: string }) => 
      userPreferenceService.deleteUserPreference(userId, preferenceId),
  });
};