import { useQuery } from '@tanstack/react-query';
import { weatherService } from '../services';

// Weather data hooks
export const useCurrentDaySummaries = (userId: string) => {
  return useQuery({
    queryKey: ['currentDaySummaries', userId],
    queryFn: () => weatherService.getCurrentDaySummariesForAllTrackedLocations(userId),
    enabled: !!userId,
  });
};

export const useFiveDayForecast = (locationId: string, userId: string) => {
  return useQuery({
    queryKey: ['fiveDayForecast', locationId, userId],
    queryFn: () => weatherService.getFiveDayForecastForLocation(locationId, userId),
    enabled: !!locationId && !!userId,
  });
};

export const useHourlyForecast = (locationId: string, userId: string) => {
  return useQuery({
    queryKey: ['hourlyForecast', locationId, userId],
    queryFn: () => weatherService.getHourlyForecastForLocation(locationId, userId),
    enabled: !!locationId && !!userId,
  });
};