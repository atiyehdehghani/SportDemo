import { NavLink, Outlet } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';

const menu = [
  { to: '/admin', label: 'داشبورد', icon: '📊', end: true },
  { to: '/admin/users', label: 'کاربران', icon: '👥' },
  { to: '/admin/packages', label: 'پکیج‌ها', icon: '🏋️' },
  { to: '/admin/orders', label: 'خریدها', icon: '🛍' },
  { to: '/admin/sliders', label: 'اسلایدر', icon: '🖼' },
  { to: '/admin/articles', label: 'مقالات', icon: '📝' },
];

export default function AdminLayout() {
  const { user, isAdmin } = useAuth();

  if (!isAdmin) {
    return (
      <div className="py-24 text-center text-gray-500">
        <div className="mb-3 text-5xl">🔒</div>
        <p>دسترسی به این بخش فقط برای مدیران مجاز است.</p>
      </div>
    );
  }

  return (
    <div className="mx-auto flex max-w-7xl gap-6 px-4 py-8">
      <aside className="h-fit w-56 shrink-0 rounded-2xl border border-gray-100 bg-white p-3 shadow-sm">
        <div className="mb-3 border-b border-gray-100 px-2 pb-3">
          <p className="text-sm font-bold text-gray-800">{user?.fullName}</p>
          <p className="text-xs text-gray-400">مدیر سیستم</p>
        </div>
        <nav className="space-y-1">
          {menu.map((item) => (
            <NavLink
              key={item.to}
              to={item.to}
              end={item.end}
              className={({ isActive }) =>
                `flex items-center gap-2 rounded-lg px-3 py-2 text-sm font-medium transition-colors ${
                  isActive ? 'bg-primary-600 text-white' : 'text-gray-600 hover:bg-gray-100'
                }`
              }
            >
              <span>{item.icon}</span>
              {item.label}
            </NavLink>
          ))}
        </nav>
      </aside>

      <main className="min-w-0 flex-1">
        <Outlet />
      </main>
    </div>
  );
}
