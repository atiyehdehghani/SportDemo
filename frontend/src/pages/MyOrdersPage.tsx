import { useQuery, useQueryClient } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { cancelOrder, getMyOrders } from '../api/orders';
import { getApiErrorMessage } from '../api/client';
import { Button, Spinner, StatusBadge } from '../components/ui';
import { formatDateTime, formatPrice } from '../utils/format';

export default function MyOrdersPage() {
  const queryClient = useQueryClient();

  const { data: orders, isLoading } = useQuery({ queryKey: ['my-orders'], queryFn: getMyOrders });

  const handleCancel = async (id: string) => {
    if (!window.confirm('این سفارش لغو شود؟')) return;
    try {
      await cancelOrder(id);
      queryClient.invalidateQueries({ queryKey: ['my-orders'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  if (isLoading) return <Spinner />;

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-black text-gray-800">سفارش‌های من</h1>

      {(orders?.length ?? 0) === 0 ? (
        <div className="py-20 text-center">
          <div className="mb-4 text-6xl">📦</div>
          <p className="mb-4 text-gray-500">هنوز سفارشی ثبت نکرده‌اید.</p>
          <Link to="/packages" className="font-medium text-primary-600 hover:underline">
            مشاهده پکیج‌ها
          </Link>
        </div>
      ) : (
        <div className="space-y-4">
          {orders!.map((order) => (
            <div key={order.id} className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
              <div className="mb-3 flex flex-wrap items-center justify-between gap-2">
                <div className="flex items-center gap-3">
                  <StatusBadge status={order.status} />
                  <span className="text-xs text-gray-400">{formatDateTime(order.createdAtUtc)}</span>
                </div>
                <span className="font-black text-primary-700">
                  {formatPrice(order.totalAmount)} <span className="text-xs font-normal">تومان</span>
                </span>
              </div>

              <ul className="divide-y divide-gray-50 rounded-xl bg-gray-50/60 px-4">
                {order.items.map((item) => (
                  <li key={item.packageId} className="flex items-center justify-between py-2.5 text-sm">
                    <span className="text-gray-700">
                      {item.packageTitle}
                      {item.quantity > 1 && (
                        <span className="mr-2 rounded bg-gray-200 px-1.5 py-0.5 text-xs text-gray-600">
                          ×{item.quantity.toLocaleString('fa-IR')}
                        </span>
                      )}
                    </span>
                    <span className="text-gray-500">{formatPrice(item.unitPrice * item.quantity)}</span>
                  </li>
                ))}
              </ul>

              {order.status === 'Pending' && (
                <div className="mt-3 flex justify-end">
                  <Button variant="danger" size="sm" onClick={() => handleCancel(order.id)}>
                    لغو سفارش
                  </Button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
