import { useQuery, useMutation } from '@tanstack/react-query';
import { authService } from '../services';

// Authentication hooks
export const useLogin = () => {
  return useMutation({
    mutationFn: (credentials: { email: string; password: string }) => 
      authService.login(credentials),
  });
};

export const useRegister = () => {
  return useMutation({
    mutationFn: (userData: { name: string; email: string; password: string }) => 
      authService.register(userData),
  });
};

export const useUpdateUser = () => {
  return useMutation({
    mutationFn: ({ userId, userData }: { userId: string; userData: { name?: string; email?: string; password?: string } }) => 
      authService.update(userId, userData),
  });
};