import { useQuery } from '@tanstack/react-query';
import { getDashboardStats } from '../../api/orders';
import { Spinner, StatusBadge } from '../../components/ui';
import { formatDateTime, formatPrice } from '../../utils/format';

function StatCard({ label, value, icon }: { label: string; value: string; icon: string }) {
  return (
    <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
      <div className="mb-2 flex items-center justify-between">
        <span className="text-sm text-gray-500">{label}</span>
        <span className="text-2xl">{icon}</span>
      </div>
      <p className="text-2xl font-black text-gray-800">{value}</p>
    </div>
  );
}

export default function AdminDashboardPage() {
  const { data: stats, isLoading } = useQuery({
    queryKey: ['admin-stats'],
    queryFn: getDashboardStats,
  });

  if (isLoading) return <Spinner />;
  if (!stats) return null;

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-black text-gray-800">داشبورد</h1>

      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <StatCard label="کاربران" value={stats.usersCount.toLocaleString('fa-IR')} icon="👥" />
        <StatCard label="پکیج‌ها" value={stats.packagesCount.toLocaleString('fa-IR')} icon="🏋️" />
        <StatCard
          label="خریدهای موفق"
          value={`${stats.paidOrdersCount.toLocaleString('fa-IR')} از ${stats.ordersCount.toLocaleString('fa-IR')}`}
          icon="🛍"
        />
        <StatCard label="درآمد کل" value={`${formatPrice(stats.totalRevenue)} تومان`} icon="💰" />
      </div>

      <div className="rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
        <h2 className="mb-4 font-bold text-gray-800">آخرین سفارش‌ها</h2>

        {stats.recentOrders.length === 0 ? (
          <p className="py-6 text-center text-sm text-gray-400">سفارشی ثبت نشده است.</p>
        ) : (
          <div className="overflow-x-auto">
            <table className="w-full text-right text-sm">
              <thead>
                <tr className="border-b border-gray-100 text-xs text-gray-400">
                  <th className="pb-2 font-medium">خریدار</th>
                  <th className="pb-2 font-medium">تعداد اقلام</th>
                  <th className="pb-2 font-medium">مبلغ</th>
                  <th className="pb-2 font-medium">وضعیت</th>
                  <th className="pb-2 font-medium">تاریخ</th>
                </tr>
              </thead>
              <tbody>
                {stats.recentOrders.map((order) => (
                  <tr key={order.id} className="border-b border-gray-50 last:border-0">
                    <td className="py-3 font-medium text-gray-700">{order.userName}</td>
                    <td className="py-3 text-gray-500">
                      {order.items.reduce((sum, i) => sum + i.quantity, 0).toLocaleString('fa-IR')}
                    </td>
                    <td className="py-3 text-gray-700">{formatPrice(order.totalAmount)}</td>
                    <td className="py-3"><StatusBadge status={order.status} /></td>
                    <td className="py-3 text-xs text-gray-400">{formatDateTime(order.createdAtUtc)}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </div>
    </div>
  );
}
