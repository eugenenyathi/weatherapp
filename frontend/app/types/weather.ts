// Weather-related types and interfaces

export interface Location {
  id: string;
  name: string;
  latitude: number;
  longitude: number;
  country: string;
}

export interface LocationRequest {
  name: string;
  latitude: number;
  longitude: number;
  country: string;
}

export interface LocationWeatherSummary {
  id: string; // ID of the tracked location entry
  locationId: string;
  locationName: string;
  displayName?: string;
  date: string;
  minTemp: number;
  maxTemp: number;
  rain: number;
  summary?: string;
  unit: "Metric" | "Imperial";
  isFavorite: boolean;
  lastSyncedAt?: string;
}

export interface DayWeather {
  date: string;
  minTemp: number;
  maxTemp: number;
  humidity: number;
  rain: number;
  summary: string;
}

export interface LocationFiveDayForecast {
  locationId: string;
  locationName: string;
  unit: "Metric" | "Imperial";
  fiveDayForecasts: DayWeather[];
}

export interface HourWeather {
  dateTime: string;
  temp: number;
  humidity: number;
}

export interface LocationHourlyForecast {
  locationId: string;
  locationName: string;
  unit: "Metric" | "Imperial";
  hourlyForecasts: HourWeather[];
}

export interface RefreshResult {
  success: boolean;
  message: string;
  lastSyncedAt?: string;
  nextRefreshAllowedAt?: string;
}
