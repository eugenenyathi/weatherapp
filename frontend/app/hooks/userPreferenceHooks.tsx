import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { userPreferenceService } from '../services/UserPreferenceService';
import type { UserPreferenceRequest } from '../types';

// User preference hooks
export const useUserPreference = (userId: string) => {
  return useQuery({
    queryKey: ['userPreference', userId],
    queryFn: () => userPreferenceService.getUserPreference(userId),
    enabled: !!userId,
  });
};

export const useCreateUserPreference = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, preferenceData }: { userId: string; preferenceData: UserPreferenceRequest }) =>
      userPreferenceService.createUserPreference(userId, preferenceData),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['userPreference', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['fiveDayForecast', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['hourlyForecast', variables.userId] });
    },
  });
};

export const useUpdateUserPreference = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: ({ userId, preferenceId, preferenceData }: {
      userId: string;
      preferenceId: string;
      preferenceData: UserPreferenceRequest
    }) =>
      userPreferenceService.updateUserPreference(userId, preferenceId, preferenceData),
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ['userPreference', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['currentDaySummaries', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['fiveDayForecast', variables.userId] });
      queryClient.invalidateQueries({ queryKey: ['hourlyForecast', variables.userId] });
    },
  });
};
