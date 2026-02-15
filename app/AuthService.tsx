import axios from 'axios';

// Interfaces for request and response payloads
export interface RegisterRequest {
  name: string;
  email: string;
  password: string;
}

export interface RegisterResponse {
  id: string;
  name: string;
  email: string;
}

export interface ErrorResponse {
  message: string;
}

class AuthService {
  private baseUrl: string;

  constructor() {
    // Use the environment variable for the API base URL, fallback to a default
    this.baseUrl = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:3001/api';
  }

  async register(userData: RegisterRequest): Promise<RegisterResponse> {
    try {
      const response = await axios.post<RegisterResponse>(
        `${this.baseUrl}/auth/register`,
        userData,
        {
          headers: {
            'Content-Type': 'application/json',
          },
        }
      );

      return response.data;
    } catch (error: any) {
      if (error.response) {
        // Server responded with error status
        const errorMessage: ErrorResponse = error.response.data;
        throw new Error(errorMessage.message || 'Registration failed');
      } else if (error.request) {
        // Request was made but no response received
        throw new Error('Network error: Unable to reach the server');
      } else {
        // Something else happened
        throw new Error(error.message || 'An unexpected error occurred');
      }
    }
  }
}

export const authService = new AuthService();