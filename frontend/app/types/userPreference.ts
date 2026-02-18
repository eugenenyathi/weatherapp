// User preference types and interfaces

export interface UserPreference {
  id: string;
  preferredUnit: "Metric" | "Imperial";
  refreshInterval: number;
}

export interface UserPreferenceRequest {
  preferredUnit?: "Metric" | "Imperial";
  refreshInterval?: number;
}
