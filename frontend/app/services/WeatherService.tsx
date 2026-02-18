import axios from "axios";
import type {
  LocationWeatherSummary,
  DayWeather,
  LocationFiveDayForecast,
  HourWeather,
  LocationHourlyForecast,
  RefreshResult,
} from "../types";

class WeatherService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5243/api";
  }

  async getCurrentDaySummariesForAllTrackedLocations(
    userId: string,
  ): Promise<LocationWeatherSummary[]> {
    try {
      const response = await axios.get<LocationWeatherSummary[]>(
        `${this.baseUrl}/weather-forecasts/current-day-summaries/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to fetch weather summaries",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async getFiveDayForecastForLocation(
    locationId: string,
    userId: string,
  ): Promise<LocationFiveDayForecast> {
    try {
      const response = await axios.get<LocationFiveDayForecast>(
        `${this.baseUrl}/weather-forecasts/five-day-forecast/${locationId}/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to fetch forecast",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async getHourlyForecastForLocation(
    locationId: string,
    userId: string,
  ): Promise<LocationHourlyForecast> {
    try {
      const response = await axios.get<LocationHourlyForecast>(
        `${this.baseUrl}/weather-forecasts/hourly-forecast/${locationId}/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to fetch hourly forecast",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async refreshWeatherData(userId: string): Promise<RefreshResult> {
    try {
      const response = await axios.post<RefreshResult>(
        `${this.baseUrl}/weather-forecasts/refresh/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to refresh weather data",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }
}

export const weatherService = new WeatherService();
