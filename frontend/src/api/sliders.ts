import { api } from './client';
import type { SliderDto } from '../types';

export async function getSliders(): Promise<SliderDto[]> {
  const { data } = await api.get<SliderDto[]>('/sliders');
  return data;
}

export async function getAllSliders(): Promise<SliderDto[]> {
  const { data } = await api.get<SliderDto[]>('/sliders/all');
  return data;
}

export async function createSlider(form: FormData): Promise<SliderDto> {
  const { data } = await api.post<SliderDto>('/sliders', form);
  return data;
}

export async function updateSlider(id: string, form: FormData): Promise<SliderDto> {
  const { data } = await api.put<SliderDto>(`/sliders/${id}`, form);
  return data;
}

export async function deleteSlider(id: string): Promise<void> {
  await api.delete(`/sliders/${id}`);
}
