// Main service file that exports all individual services
export { authService } from './AuthService';
export { locationService } from './LocationService';
export { weatherService } from './WeatherService';
export { trackLocationService } from './TrackLocationService';
export { userPreferenceService } from './UserPreferenceService';

// Re-export all types from the types folder
export type {
  User,
  RegisterRequest,
  LoginRequest,
  Location,
  LocationWeatherSummary,
  DayWeather,
  LocationFiveDayForecast,
  HourWeather,
  LocationHourlyForecast,
  RefreshResult,
  TrackLocation,
  CreateTrackLocationRequest,
  UpdateTrackLocationRequest,
  UserPreference,
  UserPreferenceRequest,
} from '../types';
