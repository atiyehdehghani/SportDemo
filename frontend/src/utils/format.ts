export function formatPrice(value: number): string {
  return value.toLocaleString('fa-IR');
}

export function formatDate(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleDateString('fa-IR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

export function formatDateTime(iso: string | null): string {
  if (!iso) return '—';
  return new Date(iso).toLocaleString('fa-IR', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  });
}

export const orderStatusLabels: Record<string, string> = {
  Pending: 'در انتظار پرداخت',
  Paid: 'پرداخت‌شده',
  Canceled: 'لغو‌شده',
};
