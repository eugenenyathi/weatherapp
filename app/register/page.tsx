'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';

export default function Register() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const router = useRouter();

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    // In a real app, you would register the user here
    // For now, we'll just simulate a successful registration
    localStorage.setItem('isLoggedIn', 'true');
    router.push('/'); // Redirect to home page
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center">
      <div className="bg-white p-6 rounded-lg shadow-md w-full max-w-sm">
        <h1 className="text-xl font-bold text-center mb-4">Register</h1>
        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label htmlFor="email" className="block text-gray-700 mb-1">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm"
              required
            />
          </div>
          <div className="mb-4">
            <label htmlFor="password" className="block text-gray-700 mb-1">Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm"
              required
            />
          </div>
          <button
            type="submit"
            className="w-full bg-blue-500 text-white py-1.5 px-3 rounded text-sm hover:bg-blue-600 transition-colors"
          >
            Register
          </button>
        </form>
        <div className="mt-3 text-center">
          <p className="text-gray-600 text-sm">
            Already have an account?{' '}
            <Link href="/login" className="text-blue-500 hover:underline">
              Login
            </Link>
          </p>
          <p className="text-gray-600 mt-2 text-sm">
            <Link href="/" className="text-blue-500 hover:underline">
              ← Back to Home
            </Link>
          </p>
        </div>
      </div>
    </div>
  );
}