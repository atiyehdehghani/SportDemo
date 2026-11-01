import { api } from './client';
import type { PagedResult, UserDto } from '../types';

export interface UserListParams {
  search?: string;
  page?: number;
  pageSize?: number;
}

export async function getUsers(params: UserListParams = {}): Promise<PagedResult<UserDto>> {
  const { data } = await api.get<PagedResult<UserDto>>('/users', { params });
  return data;
}

export async function createUser(input: {
  fullName: string;
  email: string;
  password: string;
  role: string;
}): Promise<UserDto> {
  const { data } = await api.post<UserDto>('/users', input);
  return data;
}

export async function updateUser(
  id: string,
  input: { fullName: string; isActive: boolean; role: string; newPassword?: string },
): Promise<void> {
  await api.put(`/users/${id}`, input);
}

export async function deleteUser(id: string): Promise<void> {
  await api.delete(`/users/${id}`);
}
