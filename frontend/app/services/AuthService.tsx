import axios from "axios";
import type { User, RegisterRequest, LoginRequest } from "../types";

class AuthService {
  private baseUrl: string;

  constructor() {
    this.baseUrl =
      process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5243/api";
  }

  async register(userData: RegisterRequest): Promise<User> {
    try {
      const response = await axios.post<User>(
        `${this.baseUrl}/auth/register`,
        userData,
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || "Registration failed");
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async login(credentials: LoginRequest): Promise<User> {
    try {
      const response = await axios.post<User>(
        `${this.baseUrl}/auth/login`,
        credentials,
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || "Login failed");
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }

  async update(
    userId: string,
    userData: { name?: string; email?: string; password?: string },
  ): Promise<User> {
    try {
      const response = await axios.put<User>(
        `${this.baseUrl}/auth/update/${userId}`,
        userData,
        {
          headers: {
            "Content-Type": "application/json",
          },
        },
      );
      return response.data;
    } catch (error: any) {
      if (error.response) {
        throw new Error(error.response.data.message || "Update failed");
      } else if (error.request) {
        throw new Error("Network error: Unable to reach the server");
      } else {
        throw new Error(error.message || "An unexpected error occurred");
      }
    }
  }
}

export const authService = new AuthService();
