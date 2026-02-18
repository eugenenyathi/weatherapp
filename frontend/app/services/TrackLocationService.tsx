import axios from "axios";
import type {
  TrackLocation,
  CreateTrackLocationRequest,
  UpdateTrackLocationRequest,
} from "../types";

class TrackLocationService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5243/api";
  }

  async getTrackedLocationByUserId(userId: string): Promise<TrackLocation> {
    try {
      const response = await axios.get<TrackLocation>(
        `${this.baseUrl}/track/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to fetch tracked location",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async createTrackLocation(
    userId: string,
    trackLocationData: CreateTrackLocationRequest,
  ): Promise<TrackLocation> {
    try {
      const response = await axios.post<TrackLocation>(
        `${this.baseUrl}/track/${userId}`,
        trackLocationData,
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
          error.response.data.message || "Failed to create tracked location",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async updateTrackLocation(
    userId: string,
    trackedLocationId: string,
    trackLocationData: UpdateTrackLocationRequest,
  ): Promise<TrackLocation> {
    try {
      const response = await axios.put<TrackLocation>(
        `${this.baseUrl}/track/${userId}/${trackedLocationId}`,
        trackLocationData,
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
          error.response.data.message || "Failed to update tracked location",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async deleteTrackLocation(
    userId: string,
    trackedLocationId: string,
  ): Promise<void> {
    try {
      await axios.delete(
        `${this.baseUrl}/track/${userId}/${trackedLocationId}`,
      );
    } catch (error: any) {
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to delete tracked location",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }
}

export const trackLocationService = new TrackLocationService();
