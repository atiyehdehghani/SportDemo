import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../../contexts/AuthContext';
import { useCart } from '../../contexts/CartContext';

export default function Header() {
  const { user, logout, isAdmin } = useAuth();
  const { totalCount } = useCart();
  const navigate = useNavigate();

  return (
    <header className="sticky top-0 z-40 border-b border-gray-100 bg-white/95 backdrop-blur">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between gap-4 px-4">
        <Link to="/" className="flex items-center gap-2 text-xl font-black text-primary-700">
          <span className="text-2xl">🏋️</span>
          اسپورت مارکت
        </Link>

        <nav className="hidden items-center gap-6 text-sm font-medium text-gray-600 md:flex">
          <NavLink to="/" className={({ isActive }) => (isActive ? 'text-primary-600' : 'hover:text-primary-600')}>
            خانه
          </NavLink>
          <NavLink to="/packages" className={({ isActive }) => (isActive ? 'text-primary-600' : 'hover:text-primary-600')}>
            پکیج‌ها
          </NavLink>
          <NavLink to="/articles" className={({ isActive }) => (isActive ? 'text-primary-600' : 'hover:text-primary-600')}>
            مقالات
          </NavLink>
        </nav>

        <div className="flex items-center gap-2">
          {user ? (
            <>
              {isAdmin && (
                <Link
                  to="/admin"
                  className="rounded-lg bg-gray-800 px-3 py-1.5 text-xs font-medium text-white hover:bg-gray-700"
                >
                  پنل مدیریت
                </Link>
              )}
              <Link to="/orders" className="text-sm font-medium text-gray-700 hover:text-primary-600">
                {user.fullName}
              </Link>
              <button
                className="text-sm text-gray-500 hover:text-red-600"
                onClick={() => {
                  logout();
                  navigate('/');
                }}
              >
                خروج
              </button>
            </>
          ) : (
            <>
              <Link to="/login" className="rounded-lg px-3 py-1.5 text-sm font-medium text-gray-700 hover:text-primary-600">
                ورود
              </Link>
              <Link to="/register" className="rounded-lg bg-primary-600 px-3 py-1.5 text-sm font-medium text-white hover:bg-primary-700">
                ثبت‌نام
              </Link>
            </>
          )}

          <Link
            to="/cart"
            className="relative rounded-lg bg-gray-100 p-2 text-lg hover:bg-gray-200"
            aria-label="سبد خرید"
          >
            🛒
            {totalCount > 0 && (
              <span className="absolute -top-1 -left-1 flex h-5 min-w-5 items-center justify-center rounded-full bg-red-500 px-1 text-[10px] font-bold text-white">
                {totalCount.toLocaleString('fa-IR')}
              </span>
            )}
          </Link>
        </div>
      </div>
    </header>
  );
}
