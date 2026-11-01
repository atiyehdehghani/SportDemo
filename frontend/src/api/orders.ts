import { api } from './client';
import type { DashboardStats, OrderDto, OrderStatus, PagedResult } from '../types';

export interface PlaceOrderInput {
  packageId: string;
  quantity: number;
}

export async function placeOrder(items: PlaceOrderInput[]): Promise<OrderDto> {
  const { data } = await api.post<OrderDto>('/orders', { items });
  return data;
}

export async function payOrder(id: string): Promise<OrderDto> {
  const { data } = await api.post<OrderDto>(`/orders/${id}/pay`);
  return data;
}

export async function cancelOrder(id: string): Promise<OrderDto> {
  const { data } = await api.post<OrderDto>(`/orders/${id}/cancel`);
  return data;
}

export async function getMyOrders(): Promise<OrderDto[]> {
  const { data } = await api.get<OrderDto[]>('/orders/my');
  return data;
}

export async function getOrderById(id: string): Promise<OrderDto> {
  const { data } = await api.get<OrderDto>(`/orders/${id}`);
  return data;
}

export async function getAllOrders(params: { status?: string; page?: number; pageSize?: number } = {}): Promise<PagedResult<OrderDto>> {
  const { data } = await api.get<PagedResult<OrderDto>>('/orders', { params });
  return data;
}

export async function updateOrderStatus(id: string, status: OrderStatus): Promise<OrderDto> {
  const { data } = await api.put<OrderDto>(`/orders/${id}/status`, { status });
  return data;
}

export async function getDashboardStats(): Promise<DashboardStats> {
  const { data } = await api.get<DashboardStats>('/orders/dashboard-stats');
  return data;
}
