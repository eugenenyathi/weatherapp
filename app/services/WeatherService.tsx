import axios from 'axios';

export interface LocationWeatherSummary {
  locationId: string;
  locationName: string;
  date: string; // DateOnly in .NET maps to string in JS
  minTemp: number;
  maxTemp: number;
  rain: number;
  unit: 'Metric' | 'Imperial';
}

export interface DayWeather {
  date: string; // DateOnly in .NET maps to string in JS
  minTemp: number;
  maxTemp: number;
  humidity: number;
  rain: number;
  summary: string;
}

export interface LocationFiveDayForecast {
  locationId: string;
  locationName: string;
  unit: 'Metric' | 'Imperial';
  fiveDayForecasts: DayWeather[];
}

class WeatherService {
  private baseUrl: string;

  constructor() {
    // Use the environment variable for the API base URL, fallback to a default
    this.baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5243/api';
  }

  async getCurrentDaySummariesForAllTrackedLocations(userId: string): Promise<LocationWeatherSummary[]> {
    try {
      const response = await axios.get<LocationWeatherSummary[]>(
        `${this.baseUrl}/weather-forecasts/current-day-summaries/${userId}`
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to fetch weather summaries');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }

  async getFiveDayForecastForLocation(locationId: string, userId: string): Promise<LocationFiveDayForecast> {
    try {
      const response = await axios.get<LocationFiveDayForecast>(
        `${this.baseUrl}/weather-forecasts/five-day-forecast/${locationId}/${userId}`
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to fetch forecast');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }
}

export const weatherService = new WeatherService();