import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { payOrder, placeOrder } from '../api/orders';
import { getApiErrorMessage } from '../api/client';
import { Button, Modal, Spinner } from '../components/ui';
import { useAuth } from '../contexts/AuthContext';
import { useCart } from '../contexts/CartContext';
import { formatPrice } from '../utils/format';

export default function CartPage() {
  const { items, totalAmount, updateQuantity, removeItem, clear } = useCart();
  const { user, isLoading } = useAuth();
  const navigate = useNavigate();

  const [showPayModal, setShowPayModal] = useState(false);
  const [processing, setProcessing] = useState(false);
  const [payStep, setPayStep] = useState<'form' | 'success' | 'failed'>('form');
  const [error, setError] = useState('');
  const [cardNumber, setCardNumber] = useState('');

  const startCheckout = () => {
    if (!user) {
      navigate('/login', { state: { from: '/cart' } });
      return;
    }
    setPayStep('form');
    setCardNumber('');
    setError('');
    setShowPayModal(true);
  };

  // مرحله ۱: ثبت سفارش — مرحله ۲: پرداخت شبیه‌سازی‌شده درگاه
  const handlePay = async () => {
    setError('');
    setProcessing(true);
    try {
      const order = await placeOrder(items.map((i) => ({ packageId: i.packageId, quantity: i.quantity })));
      await payOrder(order.id);
      setPayStep('success');
      clear();
    } catch (err) {
      setError(getApiErrorMessage(err));
      setPayStep('failed');
    } finally {
      setProcessing(false);
    }
  };

  if (isLoading) return <Spinner />;

  if (items.length === 0 && payStep !== 'success') {
    return (
      <div className="py-24 text-center">
        <div className="mb-4 text-6xl">🛒</div>
        <p className="mb-4 text-gray-500">سبد خرید شما خالی است.</p>
        <Link to="/packages" className="font-medium text-primary-600 hover:underline">
          مشاهده پکیج‌ها
        </Link>
      </div>
    );
  }

  return (
    <div className="mx-auto max-w-4xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-black text-gray-800">سبد خرید</h1>

      <div className="space-y-4">
        {items.map((item) => (
          <div
            key={item.packageId}
            className="flex items-center gap-4 rounded-2xl border border-gray-100 bg-white p-4 shadow-sm"
          >
            <div className="h-20 w-20 shrink-0 overflow-hidden rounded-xl bg-gray-100">
              {item.imageUrl ? (
                <img src={item.imageUrl} alt={item.title} className="h-full w-full object-cover" />
              ) : (
                <div className="flex h-full items-center justify-center text-2xl">🏋️</div>
              )}
            </div>

            <div className="min-w-0 flex-1">
              <h3 className="truncate font-bold text-gray-800">{item.title}</h3>
              <p className="mt-1 text-sm text-gray-500">{formatPrice(item.unitPrice)} تومان</p>
            </div>

            <div className="flex items-center gap-1">
              <button
                className="h-8 w-8 rounded-lg bg-gray-100 font-bold text-gray-600 hover:bg-gray-200"
                onClick={() => updateQuantity(item.packageId, item.quantity + 1)}
                aria-label="افزایش"
              >
                +
              </button>
              <span className="w-10 text-center font-bold">{item.quantity.toLocaleString('fa-IR')}</span>
              <button
                className="h-8 w-8 rounded-lg bg-gray-100 font-bold text-gray-600 hover:bg-gray-200"
                onClick={() => updateQuantity(item.packageId, item.quantity - 1)}
                aria-label="کاهش"
              >
                −
              </button>
            </div>

            <div className="w-28 text-left font-black text-primary-700">
              {formatPrice(item.unitPrice * item.quantity)}
              <span className="mr-1 text-xs font-normal text-gray-400">تومان</span>
            </div>

            <button
              className="rounded-lg p-2 text-gray-400 hover:bg-red-50 hover:text-red-600"
              onClick={() => removeItem(item.packageId)}
              aria-label="حذف"
            >
              🗑
            </button>
          </div>
        ))}
      </div>

      <div className="mt-6 flex flex-col items-end gap-3 rounded-2xl border border-gray-100 bg-white p-5 shadow-sm">
        <div className="flex w-full items-center justify-between text-sm text-gray-500">
          <span>مبلغ کل:</span>
          <span className="text-xl font-black text-gray-800">
            {formatPrice(totalAmount)} <span className="text-sm font-normal">تومان</span>
          </span>
        </div>

        <div className="flex w-full gap-2 sm:w-auto">
          <Button variant="secondary" onClick={clear}>
            خالی کردن سبد
          </Button>
          <Button size="lg" onClick={startCheckout} disabled={items.length === 0}>
            ادامه و پرداخت
          </Button>
        </div>

        {!user && (
          <p className="text-xs text-amber-600">برای تکمیل خرید ابتدا وارد حساب کاربری شوید.</p>
        )}
      </div>

      {/* درگاه پرداخت شبیه‌سازی‌شده */}
      <Modal
        open={showPayModal}
        title={payStep === 'success' ? 'پرداخت موفق' : payStep === 'failed' ? 'پرداخت ناموفق' : 'درگاه پرداخت (آزمایشی)'}
        onClose={() => {
          if (!processing) setShowPayModal(false);
        }}
      >
        {payStep === 'form' && (
          <div className="space-y-4">
            <div className="rounded-xl bg-blue-50 p-4 text-sm text-blue-800">
              مبلغ قابل پرداخت:{' '}
              <b className="text-base">
                {formatPrice(totalAmount)} تومان
              </b>
            </div>

            <label className="block">
              <span className="mb-1 block text-sm font-medium text-gray-700">شماره کارت</span>
              <input
                dir="ltr"
                className="w-full rounded-lg border border-gray-300 px-3 py-2 text-center tracking-widest outline-none focus:border-primary-500"
                placeholder="6037 9999 9999 9999"
                value={cardNumber}
                onChange={(e) => setCardNumber(e.target.value)}
                maxLength={19}
              />
            </label>

            <p className="text-xs text-gray-400">
              این درگاه پرداخت آزمایشی است؛ شماره کارت دلخواه وارد کنید (مثلاً 6037 9999 9999 9999).
            </p>

            <Button size="lg" className="w-full" onClick={handlePay} disabled={processing || cardNumber.replace(/\D/g, '').length < 16}>
              {processing ? 'در حال پردازش...' : 'پرداخت'}
            </Button>
          </div>
        )}

        {payStep === 'success' && (
          <div className="space-y-4 text-center">
            <div className="text-5xl">✅</div>
            <p className="font-bold text-gray-800">پرداخت با موفقیت انجام شد!</p>
            <p className="text-sm text-gray-500">سفارش شما ثبت و پرداخت شد. می‌توانید از بخش سفارش‌های من آن را ببینید.</p>
            <div className="flex justify-center gap-2">
              <Button onClick={() => { setShowPayModal(false); navigate('/orders'); }}>
                سفارش‌های من
              </Button>
              <Button variant="secondary" onClick={() => { setShowPayModal(false); navigate('/packages'); }}>
                ادامه خرید
              </Button>
            </div>
          </div>
        )}

        {payStep === 'failed' && (
          <div className="space-y-4 text-center">
            <div className="text-5xl">❌</div>
            <p className="font-bold text-red-700">پرداخت انجام نشد.</p>
            <p className="text-sm text-gray-500">{error || 'خطایی رخ داد. دوباره تلاش کنید.'}</p>
            <Button
              variant="secondary"
              onClick={() => {
                setPayStep('form');
                setError('');
              }}
              disabled={processing}
            >
              تلاش مجدد
            </Button>
          </div>
        )}
      </Modal>
    </div>
  );
}
