export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PackageDto {
  id: string;
  title: string;
  slug: string;
  description: string;
  price: number;
  discountedPrice: number | null;
  imageUrl: string | null;
  fileUrl: string | null;
  isActive: boolean;
  createdAtUtc: string;
  effectivePrice: number;
}

export interface ArticleDto {
  id: string;
  title: string;
  slug: string;
  summary: string;
  content: string;
  coverImageUrl: string | null;
  isPublished: boolean;
  publishedAtUtc: string | null;
  createdAtUtc: string;
}

export interface SliderDto {
  id: string;
  title: string;
  subtitle: string;
  imageUrl: string;
  linkUrl: string | null;
  displayOrder: number;
  isActive: boolean;
}

export interface UserDto {
  id: string;
  email: string;
  fullName: string;
  isActive: boolean;
  roles: string[];
  createdAtUtc: string;
}

export interface AuthResponse {
  token: string;
  expiresAtUtc: string;
  user: UserDto;
}

export type OrderStatus = 'Pending' | 'Paid' | 'Canceled';

export interface OrderItemDto {
  packageId: string;
  packageTitle: string;
  unitPrice: number;
  quantity: number;
}

export interface OrderDto {
  id: string;
  userId: string;
  userName: string;
  totalAmount: number;
  status: OrderStatus;
  createdAtUtc: string;
  paidAtUtc: string | null;
  items: OrderItemDto[];
}

export interface DashboardStats {
  usersCount: number;
  packagesCount: number;
  ordersCount: number;
  paidOrdersCount: number;
  totalRevenue: number;
  recentOrders: OrderDto[];
}

export interface CartItem {
  packageId: string;
  title: string;
  unitPrice: number;
  imageUrl: string | null;
  quantity: number;
}
