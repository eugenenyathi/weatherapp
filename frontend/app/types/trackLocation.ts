// Track location types and interfaces

export interface TrackLocation {
  id: string;
  isFavorite: boolean;
  displayName: string;
}

export interface CreateTrackLocationRequest {
  locationId: string;
  isFavorite?: boolean;
  displayName?: string;
}

export interface UpdateTrackLocationRequest {
  isFavorite?: boolean;
  displayName?: string;
}
