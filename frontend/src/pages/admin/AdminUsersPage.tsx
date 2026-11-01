import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useState } from 'react';
import { createUser, deleteUser, getUsers, updateUser } from '../../api/users';
import { getApiErrorMessage } from '../../api/client';
import { Button, Field, Input, Modal, Pagination, Select, Spinner } from '../../components/ui';
import { useAuth } from '../../contexts/AuthContext';
import { formatDate } from '../../utils/format';
import type { UserDto } from '../../types';

interface UserFormState {
  id: string | null;
  fullName: string;
  email: string;
  password: string;
  role: string;
  isActive: boolean;
}

const emptyForm: UserFormState = {
  id: null,
  fullName: '',
  email: '',
  password: '',
  role: 'User',
  isActive: true,
};

export default function AdminUsersPage() {
  const queryClient = useQueryClient();
  const { user: currentUser } = useAuth();

  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<UserFormState>(emptyForm);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-users', { search: query, page }],
    queryFn: () => getUsers({ search: query || undefined, page, pageSize: 10 }),
  });

  const openCreate = () => {
    setForm(emptyForm);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (u: UserDto) => {
    setForm({
      id: u.id,
      fullName: u.fullName,
      email: u.email,
      password: '',
      role: u.roles[0] ?? 'User',
      isActive: u.isActive,
    });
    setError('');
    setModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      if (form.id) {
        await updateUser(form.id, {
          fullName: form.fullName.trim(),
          isActive: form.isActive,
          role: form.role,
          newPassword: form.password || undefined,
        });
      } else {
        await createUser({
          fullName: form.fullName.trim(),
          email: form.email.trim(),
          password: form.password,
          role: form.role,
        });
      }
      setModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (u: UserDto) => {
    if (!window.confirm(`حذف کاربر «${u.fullName}»؟ این عمل قابل بازگشت نیست.`)) return;
    try {
      await deleteUser(u.id);
      queryClient.invalidateQueries({ queryKey: ['admin-users'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-black text-gray-800">مدیریت کاربران</h1>
        <Button onClick={openCreate}>+ کاربر جدید</Button>
      </div>

      <form
        className="flex gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          setQuery(search.trim());
          setPage(1);
        }}
      >
        <Input placeholder="جستجوی نام یا ایمیل..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <Button variant="secondary" type="submit">جستجو</Button>
      </form>

      <div className="overflow-x-auto rounded-2xl border border-gray-100 bg-white shadow-sm">
        {isLoading ? (
          <Spinner />
        ) : (
          <table className="w-full text-right text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60 text-xs text-gray-400">
                <th className="p-3 font-medium">نام</th>
                <th className="p-3 font-medium">ایمیل</th>
                <th className="p-3 font-medium">نقش</th>
                <th className="p-3 font-medium">وضعیت</th>
                <th className="p-3 font-medium">تاریخ عضویت</th>
                <th className="p-3 font-medium">عملیات</th>
              </tr>
            </thead>
            <tbody>
              {(data?.items ?? []).map((u) => (
                <tr key={u.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50/40">
                  <td className="p-3 font-medium text-gray-700">{u.fullName}</td>
                  <td className="p-3 text-gray-500" dir="ltr">{u.email}</td>
                  <td className="p-3">
                    <span
                      className={`rounded-full px-2 py-0.5 text-xs font-medium ${
                        u.roles.includes('Admin') ? 'bg-purple-100 text-purple-700' : 'bg-gray-100 text-gray-600'
                      }`}
                    >
                      {u.roles.includes('Admin') ? 'مدیر' : 'کاربر'}
                    </span>
                  </td>
                  <td className="p-3">
                    <span className={`text-xs font-medium ${u.isActive ? 'text-green-600' : 'text-red-500'}`}>
                      {u.isActive ? 'فعال' : 'غیرفعال'}
                    </span>
                  </td>
                  <td className="p-3 text-xs text-gray-400">{formatDate(u.createdAtUtc)}</td>
                  <td className="p-3">
                    <div className="flex gap-1">
                      <Button size="sm" variant="secondary" onClick={() => openEdit(u)}>ویرایش</Button>
                      {u.id !== currentUser?.id && (
                        <Button size="sm" variant="danger" onClick={() => handleDelete(u)}>حذف</Button>
                      )}
                    </div>
                  </td>
                </tr>
              ))}
              {(data?.items.length ?? 0) === 0 && (
                <tr>
                  <td colSpan={6} className="p-8 text-center text-gray-400">کاربری یافت نشد.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      <Pagination page={page} totalPages={data?.totalPages ?? 1} onChange={setPage} />

      <Modal open={modalOpen} title={form.id ? 'ویرایش کاربر' : 'کاربر جدید'} onClose={() => setModalOpen(false)}>
        <form className="space-y-4" onSubmit={handleSubmit}>
          {error && <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

          <Field label="نام و نام خانوادگی">
            <Input
              value={form.fullName}
              onChange={(e) => setForm((f) => ({ ...f, fullName: e.target.value }))}
              required
            />
          </Field>

          {!form.id && (
            <Field label="ایمیل">
              <Input
                type="email"
                dir="ltr"
                value={form.email}
                onChange={(e) => setForm((f) => ({ ...f, email: e.target.value }))}
                required
              />
            </Field>
          )}

          <Field label={form.id ? 'رمز عبور جدید (خالی = بدون تغییر)' : 'رمز عبور'}>
            <Input
              type="password"
              dir="ltr"
              value={form.password}
              onChange={(e) => setForm((f) => ({ ...f, password: e.target.value }))}
              required={!form.id}
              minLength={form.id && !form.password ? undefined : 6}
            />
          </Field>

          <div className="grid grid-cols-2 gap-4">
            <Field label="نقش">
              <Select
                value={form.role}
                onChange={(e) => setForm((f) => ({ ...f, role: e.target.value }))}
              >
                <option value="User">کاربر</option>
                <option value="Admin">مدیر</option>
              </Select>
            </Field>

            <Field label="وضعیت">
              <Select
                value={form.isActive ? '1' : '0'}
                onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.value === '1' }))}
              >
                <option value="1">فعال</option>
                <option value="0">غیرفعال</option>
              </Select>
            </Field>
          </div>

          <div className="flex justify-end gap-2 pt-2">
            <Button variant="secondary" type="button" onClick={() => setModalOpen(false)}>انصراف</Button>
            <Button type="submit" disabled={submitting}>
              {submitting ? 'در حال ذخیره...' : 'ذخیره'}
            </Button>
          </div>
        </form>
      </Modal>
    </div>
  );
}
