import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { login } from '../api/auth';
import { getApiErrorMessage } from '../api/client';
import { Button, Field, Input } from '../components/ui';
import { useAuth } from '../contexts/AuthContext';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const { setSession } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from ?? '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const result = await login(email.trim(), password);
      setSession(result.token, result.user);
      navigate(from, { replace: true });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="mx-auto max-w-md px-4 py-16">
      <div className="rounded-2xl border border-gray-100 bg-white p-8 shadow-sm">
        <h1 className="mb-6 text-center text-2xl font-black text-gray-800">ورود به حساب کاربری</h1>

        {error && (
          <div className="mb-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>
        )}

        <form className="space-y-4" onSubmit={handleSubmit}>
          <Field label="ایمیل">
            <Input
              type="email"
              dir="ltr"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="you@example.com"
              required
            />
          </Field>

          <Field label="رمز عبور">
            <Input
              type="password"
              dir="ltr"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </Field>

          <Button type="submit" size="lg" className="w-full" disabled={submitting}>
            {submitting ? 'در حال ورود...' : 'ورود'}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          حساب کاربری ندارید؟{' '}
          <Link to="/register" state={{ from }} className="font-medium text-primary-600 hover:underline">
            ثبت‌نام کنید
          </Link>
        </p>

        <div className="mt-6 rounded-lg bg-gray-50 p-3 text-center text-xs leading-5 text-gray-500">
          حساب مدیر: <span dir="ltr">admin@sportmarket.ir / Admin@1234</span>
        </div>
      </div>
    </div>
  );
}
