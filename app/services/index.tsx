// Main service file that exports all individual services and types
export * from './AuthService';
export * from './LocationService';
export * from './WeatherService';
export * from './TrackLocationService';
export * from './UserPreferenceService';

// Export types
export type { 
  User,
  RegisterRequest,
  LoginRequest,
  AuthResponse,
  ErrorResponse 
} from './AuthService';

export type { 
  Location,
  LocationRequest
} from './LocationService';

export type { 
  LocationWeatherSummary,
  DayWeather,
  LocationFiveDayForecast
} from './WeatherService';

export type { 
  TrackLocation,
  CreateTrackLocationRequest,
  UpdateTrackLocationRequest
} from './TrackLocationService';

export type { 
  UserPreference,
  UserPreferenceRequest
} from './UserPreferenceService';