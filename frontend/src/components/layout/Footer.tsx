import { Link } from 'react-router-dom';

export default function Footer() {
  return (
    <footer className="mt-16 bg-gray-900 text-gray-300">
      <div className="mx-auto grid max-w-6xl gap-8 px-4 py-12 md:grid-cols-3">
        <div>
          <div className="mb-3 flex items-center gap-2 text-xl font-black text-white">
            <span className="text-2xl">🏋️</span>
            اسپورت مارکت
          </div>
          <p className="text-sm leading-6 text-gray-400">
            فروشگاه تخصصی پکیج‌های تمرینی، تغذیه و آماده‌سازی بدنسازی؛ طراحی‌شده توسط مربیان حرفه‌ای برای رسیدن شما به هدف.
          </p>
        </div>

        <div>
          <h4 className="mb-3 font-bold text-white">دسترسی سریع</h4>
          <ul className="space-y-2 text-sm">
            <li><Link to="/packages" className="hover:text-white">پکیج‌های ورزشی</Link></li>
            <li><Link to="/articles" className="hover:text-white">مقالات آموزشی</Link></li>
            <li><Link to="/cart" className="hover:text-white">سبد خرید</Link></li>
            <li><Link to="/orders" className="hover:text-white">سفارش‌های من</Link></li>
          </ul>
        </div>

        <div>
          <h4 className="mb-3 font-bold text-white">تماس با ما</h4>
          <ul className="space-y-2 text-sm text-gray-400">
            <li>📧 info@sportmarket.ir</li>
            <li>📞 ۰۲۱-۱۲۳۴۵۶۷۸</li>
            <li>📍 تهران، خیابان ورزش، پلاک ۱۰</li>
          </ul>
        </div>
      </div>

      <div className="border-t border-gray-800 py-4 text-center text-xs text-gray-500">
        © {new Date().getFullYear()} اسپورت مارکت — تمامی حقوق محفوظ است.
      </div>
    </footer>
  );
}
