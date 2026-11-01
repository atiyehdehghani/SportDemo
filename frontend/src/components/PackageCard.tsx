import { Link } from 'react-router-dom';
import type { PackageDto } from '../types';
import { formatPrice } from '../utils/format';

export default function PackageCard({ pkg }: { pkg: PackageDto }) {
  const hasDiscount = pkg.discountedPrice != null && pkg.discountedPrice < pkg.price;

  return (
    <Link
      to={`/packages/${pkg.slug}`}
      className="group flex flex-col overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition-all hover:-translate-y-1 hover:shadow-lg"
    >
      <div className="relative aspect-[4/3] overflow-hidden bg-gray-100">
        {pkg.imageUrl ? (
          <img
            src={pkg.imageUrl}
            alt={pkg.title}
            className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
          />
        ) : (
          <div className="flex h-full items-center justify-center text-5xl">🏋️</div>
        )}
        {hasDiscount && (
          <span className="absolute top-3 right-3 rounded-full bg-red-500 px-2.5 py-1 text-xs font-bold text-white">
            {Math.round((1 - (pkg.discountedPrice ?? pkg.price) / pkg.price) * 100)}٪ تخفیف
          </span>
        )}
      </div>

      <div className="flex flex-1 flex-col gap-2 p-4">
        <h3 className="font-bold text-gray-800 group-hover:text-primary-600">{pkg.title}</h3>
        <p className="line-clamp-2 flex-1 text-sm leading-6 text-gray-500">{pkg.description}</p>
        <div className="mt-2 flex items-center justify-between">
          <div className="flex items-baseline gap-2">
            <span className="text-lg font-black text-primary-700">
              {formatPrice(pkg.discountedPrice ?? pkg.price)}
            </span>
            <span className="text-xs text-gray-400">تومان</span>
            {hasDiscount && (
              <span className="text-xs text-gray-400 line-through">{formatPrice(pkg.price)}</span>
            )}
          </div>
          <span className="rounded-lg bg-primary-50 px-3 py-1.5 text-xs font-bold text-primary-700 transition-colors group-hover:bg-primary-600 group-hover:text-white">
            مشاهده
          </span>
        </div>
      </div>
    </Link>
  );
}
