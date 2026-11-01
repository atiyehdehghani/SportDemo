import { api } from './client';
import type { AuthResponse, UserDto } from '../types';

export async function login(email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/login', { email, password });
  return data;
}

export async function register(fullName: string, email: string, password: string): Promise<AuthResponse> {
  const { data } = await api.post<AuthResponse>('/auth/register', { fullName, email, password });
  return data;
}

export async function getCurrentUser(): Promise<UserDto> {
  const { data } = await api.get<UserDto>('/auth/me');
  return data;
}
