import axios from "axios";

export interface UserPreference {
  id: string;
  preferredUnit: "Metric" | "Imperial";
  refreshInterval: number;
}

export interface UserPreferenceRequest {
  preferredUnit?: "Metric" | "Imperial";
  refreshInterval?: number;
}

class UserPreferenceService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5243/api";
  }

  async getUserPreference(userId: string): Promise<UserPreference | null> {
    try {
      const response = await axios.get<UserPreference | null>(
        `${this.baseUrl}/preferences/${userId}`,
      );
      return response.data;
    } catch (error: any) {
      if (error.response?.status === 404) {
        return null;
      }
      if (error.response) {
        throw new Error(
          error.response.data.message || "Failed to fetch user preferences",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async createUserPreference(
    userId: string,
    request: UserPreferenceRequest,
  ): Promise<UserPreference> {
    try {
      const response = await axios.post<UserPreference>(
        `${this.baseUrl}/preferences/${userId}`,
        request,
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
          error.response.data.message || "Failed to create user preferences",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async updateUserPreference(
    userId: string,
    preferenceId: string,
    request: UserPreferenceRequest,
  ): Promise<UserPreference> {
    try {
      const response = await axios.put<UserPreference>(
        `${this.baseUrl}/preferences/${userId}/${preferenceId}`,
        request,
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
          error.response.data.message || "Failed to update user preferences",
        );
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }
}

export const userPreferenceService = new UserPreferenceService();
