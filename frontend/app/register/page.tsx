'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { useAuth } from '../AuthContext';
import { useRegister } from '../hooks/authHooks';

// Create a client for this page
const queryClient = new QueryClient();

function RegisterPageContent() {
  const [name, setName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState<string | null>(null);
  const router = useRouter();
  const { login } = useAuth();

  const registerMutation = useRegister();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      // Call the register API
      const response = await registerMutation.mutateAsync({
        name,
        email,
        password
      });

      // Login the user using the auth context
      login(response);

      // Redirect to home page
      router.push('/');
    } catch (err: any) {
      // Handle error
      setError(err.message || 'Registration failed');
    }
  };

  return (
    <div className="min-h-screen bg-gray-100 flex items-center justify-center">
      <div className="bg-white p-6 rounded-lg shadow-md w-full max-w-sm">
        <h1 className="text-xl font-bold text-center mb-4">Register</h1>
        {error && (
          <div className="mb-3 p-2 bg-red-100 text-red-700 rounded text-sm">
            {error}
          </div>
        )}
        <form onSubmit={handleSubmit}>
          <div className="mb-3">
            <label htmlFor="name" className="block text-gray-700 mb-1">Name</label>
            <input
              type="text"
              id="name"
              value={name}
              onChange={(e) => setName(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm text-black"
              required
              disabled={registerMutation.isPending}
            />
          </div>
          <div className="mb-3">
            <label htmlFor="email" className="block text-gray-700 mb-1">Email</label>
            <input
              type="email"
              id="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm text-black"
              required
              disabled={registerMutation.isPending}
            />
          </div>
          <div className="mb-4">
            <label htmlFor="password" className="block text-gray-700 mb-1">Password</label>
            <input
              type="password"
              id="password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full px-2 py-1.5 border border-gray-300 rounded focus:outline-none focus:ring-1 focus:ring-blue-500 text-sm text-black"
              required
              disabled={registerMutation.isPending}
            />
          </div>
          <button
            type="submit"
            className={`w-full py-1.5 px-3 rounded text-sm transition-colors ${
              registerMutation.isPending 
                ? 'bg-blue-400 cursor-not-allowed' 
                : 'bg-blue-500 hover:bg-blue-600'
            } text-white`}
            disabled={registerMutation.isPending}
          >
            {registerMutation.isPending ? 'Registering...' : 'Register'}
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

export default function Register() {
  return (
    <QueryClientProvider client={queryClient}>
      <RegisterPageContent />
    </QueryClientProvider>
  );
}