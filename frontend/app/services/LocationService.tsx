import axios from "axios";

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

class LocationService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5243/api";
  }

  async createLocation(locationData: LocationRequest): Promise<Location> {
    try {
      const response = await axios.post<Location>(
        `${this.baseUrl}/location`,
        locationData,
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to create location",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }
}

export const locationService = new LocationService();
