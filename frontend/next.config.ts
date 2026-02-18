import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Enable standalone output for Docker deployment
  output: 'standalone',
  
  // Optimize compilation
  poweredByHeader: false,
  compress: true,
  
  // Image optimization
  images: {
    unoptimized: true, // For static export compatibility
  },
};

export default nextConfig;
