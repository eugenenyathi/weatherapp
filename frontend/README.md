# Weather App - Frontend

A modern, responsive weather application built with Next.js 16, React 19, and shadcn/ui components. Provides real-time weather data, 5-day forecasts, hourly forecasts, and location management with a clean, intuitive UI.

## 🚀 Quick Start

### Prerequisites

- **Node.js** 18.0 or higher
- **npm** or **yarn**
- Backend API running (see Backend Repository)

### Option 1: Local Development

#### Installation

```bash
# Clone the repository
cd weatherapp/frontend

# Install dependencies
npm install

# Set up environment variables
# Copy .env.example to .env.local and configure:
cp .env.example .env.local
```

#### Environment Variables

Create a `.env.local` file in the root directory:

```env
# Backend API URL
NEXT_PUBLIC_API_BASE_URL=http://localhost:5243/api

# OpenWeatherMap API Key
NEXT_PUBLIC_OPENWEATHER_API_KEY=your_api_key_here
```

#### Running the Development Server

```bash
npm run dev
```

Open [http://localhost:3000](http://localhost:3000) in your browser.

### Production Build

```bash
npm run build
npm start
```

## 📁 Project Structure

```
frontend/
├── app/
│   ├── modals/              # All modal components
│   │   ├── AddLocationModal.tsx
│   │   ├── EditModal.tsx
│   │   ├── HourlyWeatherModal.tsx
│   │   ├── LocationWarningModal.tsx
│   │   ├── TodayWeatherModal.tsx
│   │   ├── UserPreferenceModal.tsx
│   │   └── index.ts
│   ├── types/               # TypeScript type definitions
│   │   ├── auth.ts
│   │   ├── index.ts
│   │   ├── trackLocation.ts
│   │   ├── userPreference.ts
│   │   └── weather.ts
│   ├── services/            # API service layer
│   │   ├── AuthService.tsx
│   │   ├── LocationService.tsx
│   │   ├── TrackLocationService.tsx
│   │   ├── UserPreferenceService.ts
│   │   ├── WeatherService.tsx
│   │   └── index.tsx
│   ├── hooks/               # React Query hooks
│   │   ├── authHooks.tsx
│   │   ├── locationHooks.tsx
│   │   ├── trackLocationHooks.tsx
│   │   ├── userPreferenceHooks.tsx
│   │   └── weatherHooks.tsx
│   ├── login/               # Login page
│   ├── register/            # Registration page
│   ├── components/ui/       # shadcn/ui components
│   ├── page.tsx             # Main application page
│   ├── WeatherComponent.tsx # Weather list component
│   ├── WeatherRow.tsx       # Individual weather row
│   ├── ForecastComponent.tsx# 5-day forecast view
│   ├── Header.tsx           # Application header
│   ├── Tabs.tsx             # Tab navigation
│   ├── AuthContext.tsx      # Authentication context
│   └── layout.tsx           # Root layout with Toaster
├── components.json          # shadcn/ui configuration
├── package.json
├── tsconfig.json
└── README.md
```

## 🎨 Design Thinking & UI Philosophy

### Core Principles

1. **Clarity Over Clutter**
   - Clean, minimal interface with focused content
   - White space used intentionally to reduce cognitive load
   - Clear visual hierarchy with consistent typography

2. **Mobile-First Responsive Design**
   - Fully responsive from 320px to 4K displays
   - Touch-friendly targets (minimum 44px)
   - Adaptive layouts that reorganize based on screen size
   - Mobile: Icon-only buttons, compact layouts
   - Desktop: Full labels, expanded information display

3. **Progressive Disclosure**
   - Show essential information at a glance (location, rain %, high/low temps)
   - Detailed views available on demand (hourly, forecast, today's summary)
   - Prevents overwhelming users with too much data upfront

4. **Consistent Visual Language**
   - **Icons**: Lucide React icons for universal recognition
   - **Colors**: Neutral grays with purposeful accent colors
     - Red for favorites
     - Yellow for sun/weather
     - Blue for actions
     - Gray for secondary elements
   - **Typography**: Geist Sans for modern, readable text

5. **Feedback & Responsiveness**
   - Loading states for all async actions
   - Toast notifications for success/error feedback
   - Optimistic UI updates where appropriate
   - Clear error messages with actionable guidance

### Component Design Decisions

#### Header

- **Fixed positioning** - Always accessible, doesn't scroll with content
- **Centered max-width** - Maintains readability on ultra-wide screens
- **Responsive buttons**:
  - Desktop: Full labels with icons
  - Mobile: Icon-only to save space
- **User dropdown** - Contextual options based on auth state

#### Weather List

- **Table-like alignment** - Header row aligns perfectly with data columns
- **Icon + label headers** - Clear column identification
- **Last synced indicator** - Transparency about data freshness
- **Action buttons** - Heart (favorite) and menu (more options) always visible

#### Weather Row

- **Five-column layout**:
  1. Location name (expandable)
  2. Rain chance with icon
  3. High temperature with icon
  4. Low temperature with icon
  5. Action buttons (favorite, menu)
- **Consistent spacing** - Matches header exactly for visual alignment
- **Touch-friendly** - Adequate spacing between interactive elements

#### Dropdown Menu (3-dots)

- **Four contextual actions**:
  - 🕐 Hourly Weather - Next 24 hours forecast
  - 📅 Forecast - 5-day detailed forecast
  - ☀️ Today - Current day's summary
  - ✏️ Edit - Change display name
  - 🗑️ Remove - Delete location
- **Destructive styling** for remove action

#### Modals

- **shadcn Dialog components** - Consistent, accessible modals
- **Focused content** - Each modal has a single purpose
- **Loading states** - Clear feedback during async operations
- **Error handling** - Inline error messages

### Color System

| Purpose  | Color            | Usage                       |
| -------- | ---------------- | --------------------------- |
| Primary  | Blue (#3b82f6)   | Actions, links              |
| Success  | Green (#22c55e)  | Success toasts              |
| Error    | Red (#ef4444)    | Errors, destructive actions |
| Warning  | Yellow (#eab308) | Weather icons               |
| Favorite | Red (#ef4444)    | Heart icon when active      |
| Muted    | Gray (#6b7280)   | Secondary text, icons       |

### Typography Scale

| Element       | Mobile | Desktop |
| ------------- | ------ | ------- |
| App Title     | 18px   | 36px    |
| Location Name | 16px   | 18px    |
| Weather Data  | 16px   | 18px    |
| Labels        | 14px   | 16px    |
| Small Text    | 12px   | 14px    |

## 🔑 Features

### Authentication

- ✅ User registration with email/password
- ✅ Secure login with JWT tokens
- ✅ Session persistence via localStorage
- ✅ Guest mode (limited functionality)
- ✅ User preferences management

### Location Management

- ✅ Add locations via OpenWeatherMap geocoding
- ✅ Custom display names for locations
- ✅ Favorite locations for quick access
- ✅ Remove tracked locations
- ✅ Tab filtering (Favorites / All)

### Weather Data

- ✅ Current day summary (rain %, high/low temps)
- ✅ 5-day forecast with daily summaries
- ✅ 24-hour hourly forecast
- ✅ Today's weather condition summary
- ✅ Last synced timestamp
- ✅ Manual refresh with rate limiting

### User Preferences

- ✅ Temperature unit (Metric/Imperial)
- ✅ Refresh interval configuration
- ✅ Persistent across sessions

## 🛠️ Tech Stack

### Core

- **Next.js 16** - React framework with App Router
- **React 19** - UI library
- **TypeScript** - Type safety
- **Turbopack** - Fast build tool

### Styling & UI

- **Tailwind CSS v4** - Utility-first CSS
- **shadcn/ui** - Component library
- **Lucide React** - Icon library
- **class-variance-authority** - Component variants

### State & Data

- **TanStack Query (React Query)** - Server state management
- **Axios** - HTTP client
- **Context API** - Client state (auth)

### Notifications

- **Sonner** - Toast notifications

## 📱 Responsive Breakpoints

```css
/* Mobile First */
default:     /* 0px+   - Mobile */
sm:          /* 640px+ - Small tablets */
md:          /* 768px+ - Tablets */
lg:          /* 1024px - Desktops */
xl:          /* 1280px - Large screens */
```

## 🔌 API Integration

### Endpoints Used

| Endpoint                                                         | Method              | Purpose                  |
| ---------------------------------------------------------------- | ------------------- | ------------------------ |
| `/api/auth/register`                                             | POST                | User registration        |
| `/api/auth/login`                                                | POST                | User login               |
| `/api/location`                                                  | POST                | Create location          |
| `/api/track/{userId}`                                            | GET/POST/PUT/DELETE | Manage tracked locations |
| `/api/weather-forecasts/current-day-summaries/{userId}`          | GET                 | Get weather summaries    |
| `/api/weather-forecasts/five-day-forecast/{locationId}/{userId}` | GET                 | Get 5-day forecast       |
| `/api/weather-forecasts/hourly-forecast/{locationId}/{userId}`   | GET                 | Get hourly forecast      |
| `/api/weather-forecasts/refresh/{userId}`                        | POST                | Manual refresh           |
| `/api/preferences/{userId}`                                      | GET/POST/PUT        | User preferences         |

### Error Handling

```typescript
try {
  const data = await weatherService.getForecast(locationId, userId);
} catch (error: any) {
  if (error.response) {
    // Server responded with error
    toast.error(error.response.data.message);
  } else if (error.request) {
    // Network error
    toast.error("Network error: Unable to reach the server");
  } else {
    // Other errors
    toast.error(error.message);
  }
}
```

## 🧪 Available Scripts

```bash
# Development
npm run dev          # Start dev server

# Production
npm run build        # Build for production
npm start            # Start production server

# Code Quality
npm run lint         # Run ESLint
```
