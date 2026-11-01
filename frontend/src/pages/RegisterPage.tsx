import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { register } from '../api/auth';
import { getApiErrorMessage } from '../api/client';
import { Button, Field, Input } from '../components/ui';
import { useAuth } from '../contexts/AuthContext';

export default function RegisterPage() {
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirm, setConfirm] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const { setSession } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const from = (location.state as { from?: string } | null)?.from ?? '/';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    if (password !== confirm) {
      setError('تکرار رمز عبور با رمز عبور یکسان نیست.');
      return;
    }

    setSubmitting(true);
    try {
      const result = await register(fullName.trim(), email.trim(), password);
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
        <h1 className="mb-6 text-center text-2xl font-black text-gray-800">ایجاد حساب کاربری</h1>

        {error && (
          <div className="mb-4 rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>
        )}

        <form className="space-y-4" onSubmit={handleSubmit}>
          <Field label="نام و نام خانوادگی">
            <Input value={fullName} onChange={(e) => setFullName(e.target.value)} placeholder="مثلاً علی رضایی" required />
          </Field>

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
              placeholder="حداقل ۶ کاراکتر شامل عدد"
              required
              minLength={6}
            />
          </Field>

          <Field label="تکرار رمز عبور">
            <Input
              type="password"
              dir="ltr"
              value={confirm}
              onChange={(e) => setConfirm(e.target.value)}
              placeholder="••••••••"
              required
            />
          </Field>

          <Button type="submit" size="lg" className="w-full" disabled={submitting}>
            {submitting ? 'در حال ثبت‌نام...' : 'ثبت‌نام'}
          </Button>
        </form>

        <p className="mt-6 text-center text-sm text-gray-500">
          قبلاً ثبت‌نام کرده‌اید؟{' '}
          <Link to="/login" state={{ from }} className="font-medium text-primary-600 hover:underline">
            وارد شوید
          </Link>
        </p>
      </div>
    </div>
  );
}
