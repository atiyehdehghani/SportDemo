import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { getAllOrders, updateOrderStatus } from '../../api/orders';
import { getApiErrorMessage } from '../../api/client';
import { Button, Pagination, Select, Spinner, StatusBadge } from '../../components/ui';
import { formatDateTime, formatPrice } from '../../utils/format';
import type { OrderStatus } from '../../types';

export default function AdminOrdersPage() {
  const queryClient = useQueryClient();
  const [status, setStatus] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-orders', { status, page }],
    queryFn: () => getAllOrders({ status: status || undefined, page, pageSize: 15 }),
  });

  const handleStatusChange = async (id: string, newStatus: string) => {
    try {
      await updateOrderStatus(id, newStatus as OrderStatus);
      queryClient.invalidateQueries({ queryKey: ['admin-orders'] });
      queryClient.invalidateQueries({ queryKey: ['admin-stats'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  return (
    <div className="space-y-4">
      <h1 className="text-2xl font-black text-gray-800">خریدها و سفارش‌ها</h1>

      <div className="w-56">
        <Select
          value={status}
          onChange={(e) => {
            setStatus(e.target.value);
            setPage(1);
          }}
        >
          <option value="">همه وضعیت‌ها</option>
          <option value="Pending">در انتظار پرداخت</option>
          <option value="Paid">پرداخت‌شده</option>
          <option value="Canceled">لغو‌شده</option>
        </Select>
      </div>

      <div className="overflow-x-auto rounded-2xl border border-gray-100 bg-white shadow-sm">
        {isLoading ? (
          <Spinner />
        ) : (
          <table className="w-full text-right text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60 text-xs text-gray-400">
                <th className="p-3 font-medium">خریدار</th>
                <th className="p-3 font-medium">اقلام</th>
                <th className="p-3 font-medium">مبلغ</th>
                <th className="p-3 font-medium">وضعیت</th>
                <th className="p-3 font-medium">تاریخ</th>
                <th className="p-3 font-medium">تغییر وضعیت</th>
              </tr>
            </thead>
            <tbody>
              {(data?.items ?? []).map((order) => (
                <tr key={order.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50/40">
                  <td className="p-3 font-medium text-gray-700">{order.userName}</td>
                  <td className="p-3">
                    <span className="text-gray-600">
                      {order.items.map((i) => i.packageTitle).join('، ')}
                    </span>
                  </td>
                  <td className="p-3 font-bold text-gray-700">{formatPrice(order.totalAmount)}</td>
                  <td className="p-3"><StatusBadge status={order.status} /></td>
                  <td className="p-3 text-xs text-gray-400">{formatDateTime(order.createdAtUtc)}</td>
                  <td className="p-3">
                    {order.status === 'Paid' ? (
                      <span className="text-xs text-gray-300">—</span>
                    ) : (
                      <div className="flex gap-1">
                        <Button size="sm" onClick={() => handleStatusChange(order.id, 'Paid')}>
                          تأیید پرداخت
                        </Button>
                        {order.status === 'Pending' && (
                          <Button size="sm" variant="danger" onClick={() => handleStatusChange(order.id, 'Canceled')}>
                            لغو
                          </Button>
                        )}
                      </div>
                    )}
                  </td>
                </tr>
              ))}
              {(data?.items.length ?? 0) === 0 && (
                <tr>
                  <td colSpan={6} className="p-8 text-center text-gray-400">سفارشی یافت نشد.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <Pagination page={page} totalPages={data?.totalPages ?? 1} onChange={setPage} />
    </div>
  );
}
