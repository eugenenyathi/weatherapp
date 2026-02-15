import axios from 'axios';

export interface UserPreference {
  id: string;
  temperatureUnit: 'Metric' | 'Imperial';
  userId: string;
}

export interface UserPreferenceRequest {
  temperatureUnit: 'Metric' | 'Imperial';
}

class UserPreferenceService {
  private baseUrl: string;

  constructor() {
    // Use the environment variable for the API base URL, fallback to a default
    this.baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5243/api';
  }

  async getUserPreference(userId: string): Promise<UserPreference> {
    try {
      const response = await axios.get<UserPreference>(
        `${this.baseUrl}/preferences/${userId}`
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to fetch user preferences');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }

  async createUserPreference(userId: string, preferenceData: UserPreferenceRequest): Promise<UserPreference> {
    try {
      const response = await axios.post<UserPreference>(
        `${this.baseUrl}/preferences/${userId}`,
        preferenceData,
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to create user preferences');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }

  async updateUserPreference(userId: string, preferenceId: string, preferenceData: UserPreferenceRequest): Promise<UserPreference> {
    try {
      const response = await axios.put<UserPreference>(
        `${this.baseUrl}/preferences/${userId}/${preferenceId}`,
        preferenceData,
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to update user preferences');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }

  async deleteUserPreference(userId: string, preferenceId: string): Promise<void> {
    try {
      await axios.delete(
        `${this.baseUrl}/preferences/${userId}/${preferenceId}`
      );
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || 'Failed to delete user preferences');
      } else if (error.request) {
        throw new Error('Network error: Unable to reach the server');
      } else {
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }
}

export const userPreferenceService = new UserPreferenceService();