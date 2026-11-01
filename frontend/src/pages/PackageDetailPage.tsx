import { useQuery } from '@tanstack/react-query';
import { Link, useNavigate, useParams } from 'react-router-dom';
import { getPackageBySlug } from '../api/packages';
import { Button, Spinner } from '../components/ui';
import { useCart } from '../contexts/CartContext';
import { formatPrice } from '../utils/format';

export default function PackageDetailPage() {
  const { slug } = useParams<{ slug: string }>();
  const navigate = useNavigate();
  const { addItem } = useCart();

  const { data: pkg, isLoading, error } = useQuery({
    queryKey: ['package', slug],
    queryFn: () => getPackageBySlug(slug!),
    enabled: Boolean(slug),
  });

  if (isLoading) return <Spinner />;
  if (error || !pkg) {
    return (
      <div className="py-20 text-center">
        <p className="mb-4 text-gray-500">پکیج مورد نظر یافت نشد.</p>
        <Link to="/packages" className="text-primary-600 hover:underline">بازگشت به لیست پکیج‌ها</Link>
      </div>
    );
  }

  const hasDiscount = pkg.discountedPrice != null && pkg.discountedPrice < pkg.price;

  const handleAddToCart = () => {
    addItem({
      packageId: pkg.id,
      title: pkg.title,
      unitPrice: pkg.discountedPrice ?? pkg.price,
      imageUrl: pkg.imageUrl,
    });
    navigate('/cart');
  };

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <nav className="mb-6 text-sm text-gray-500">
        <Link to="/" className="hover:text-primary-600">خانه</Link>
        <span className="mx-2">/</span>
        <Link to="/packages" className="hover:text-primary-600">پکیج‌ها</Link>
        <span className="mx-2">/</span>
        <span className="text-gray-700">{pkg.title}</span>
      </nav>

      <div className="grid gap-8 lg:grid-cols-2">
        <div className="overflow-hidden rounded-2xl bg-gray-100">
          {pkg.imageUrl ? (
            <img src={pkg.imageUrl} alt={pkg.title} className="aspect-[4/3] w-full object-cover" />
          ) : (
            <div className="flex aspect-[4/3] items-center justify-center text-7xl">🏋️</div>
          )}
        </div>

        <div className="flex flex-col gap-4">
          <h1 className="text-2xl font-black text-gray-800">{pkg.title}</h1>

          <p className="whitespace-pre-line leading-7 text-gray-600">{pkg.description}</p>

          <div className="mt-auto space-y-4 rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
            <div className="flex items-baseline gap-3">
              <span className="text-3xl font-black text-primary-700">
                {formatPrice(pkg.discountedPrice ?? pkg.price)}
              </span>
              <span className="text-sm text-gray-400">تومان</span>
              {hasDiscount && (
                <span className="text-sm text-gray-400 line-through">{formatPrice(pkg.price)}</span>
              )}
            </div>

            <div className="flex gap-2">
              <Button size="lg" className="flex-1" onClick={handleAddToCart}>
                🛒 افزودن به سبد خرید
              </Button>
            </div>

            <p className="text-xs text-gray-400">
              پس از پرداخت، فایل پکیج برای شما قابل دانلود خواهد بود.
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
