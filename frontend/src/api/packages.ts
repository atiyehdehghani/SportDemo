import { api } from './client';
import type { PagedResult, PackageDto } from '../types';

export interface PackageListParams {
  search?: string;
  includeInactive?: boolean;
  page?: number;
  pageSize?: number;
}

export async function getPackages(params: PackageListParams = {}): Promise<PagedResult<PackageDto>> {
  const { data } = await api.get<PagedResult<PackageDto>>('/packages', { params });
  return data;
}

export async function getPackageBySlug(slug: string): Promise<PackageDto> {
  const { data } = await api.get<PackageDto>(`/packages/${slug}`);
  return data;
}

export async function getPackageById(id: string): Promise<PackageDto> {
  const { data } = await api.get<PackageDto>(`/packages/id/${id}`);
  return data;
}

export async function createPackage(form: FormData): Promise<PackageDto> {
  const { data } = await api.post<PackageDto>('/packages', form);
  return data;
}

export async function updatePackage(id: string, form: FormData): Promise<PackageDto> {
  const { data } = await api.put<PackageDto>(`/packages/${id}`, form);
  return data;
}

export async function deletePackage(id: string): Promise<void> {
  await api.delete(`/packages/${id}`);
}
