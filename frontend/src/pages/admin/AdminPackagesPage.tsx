import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRef, useState } from 'react';
import { createPackage, deletePackage, getPackages, updatePackage } from '../../api/packages';
import { getApiErrorMessage } from '../../api/client';
import { Button, Field, Input, Modal, Spinner, Textarea } from '../../components/ui';
import { formatPrice } from '../../utils/format';
import type { PackageDto } from '../../types';

interface PackageFormState {
  id: string | null;
  title: string;
  description: string;
  price: string;
  discountedPrice: string;
  isActive: boolean;
  imageUrl: string | null;
  fileUrl: string | null;
}

const emptyForm: PackageFormState = {
  id: null,
  title: '',
  description: '',
  price: '',
  discountedPrice: '',
  isActive: true,
  imageUrl: null,
  fileUrl: null,
};

export default function AdminPackagesPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<PackageFormState>(emptyForm);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const imageInputRef = useRef<HTMLInputElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [packageFile, setPackageFile] = useState<File | null>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-packages', page],
    queryFn: () => getPackages({ includeInactive: true, page, pageSize: 10 }),
  });

  const openCreate = () => {
    setForm(emptyForm);
    setImageFile(null);
    setPackageFile(null);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (p: PackageDto) => {
    setForm({
      id: p.id,
      title: p.title,
      description: p.description,
      price: String(p.price),
      discountedPrice: p.discountedPrice != null ? String(p.discountedPrice) : '',
      isActive: p.isActive,
      imageUrl: p.imageUrl,
      fileUrl: p.fileUrl,
    });
    setImageFile(null);
    setPackageFile(null);
    setError('');
    setModalOpen(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const fd = new FormData();
      fd.append('title', form.title.trim());
      fd.append('description', form.description.trim());
      fd.append('price', form.price);
      if (form.discountedPrice) fd.append('discountedPrice', form.discountedPrice);
      fd.append('isActive', String(form.isActive));
      if (form.imageUrl) fd.append('imageUrl', form.imageUrl);
      if (form.fileUrl) fd.append('fileUrl', form.fileUrl);
      if (imageFile) fd.append('imageFile', imageFile);
      if (packageFile) fd.append('packageFile', packageFile);

      if (form.id) {
        await updatePackage(form.id, fd);
      } else {
        await createPackage(fd);
      }

      setModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['admin-packages'] });
      queryClient.invalidateQueries({ queryKey: ['packages'] });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (p: PackageDto) => {
    if (!window.confirm(`حذف پکیج «${p.title}»؟`)) return;
    try {
      await deletePackage(p.id);
      queryClient.invalidateQueries({ queryKey: ['admin-packages'] });
      queryClient.invalidateQueries({ queryKey: ['packages'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-black text-gray-800">مدیریت پکیج‌ها</h1>
        <Button onClick={openCreate}>+ پکیج جدید</Button>
      </div>

      <div className="overflow-x-auto rounded-2xl border border-gray-100 bg-white shadow-sm">
        {isLoading ? (
          <Spinner />
        ) : (
          <table className="w-full text-right text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60 text-xs text-gray-400">
                <th className="p-3 font-medium">پکیج</th>
                <th className="p-3 font-medium">قیمت</th>
                <th className="p-3 font-medium">قیمت با تخفیف</th>
                <th className="p-3 font-medium">وضعیت</th>
                <th className="p-3 font-medium">عملیات</th>
              </tr>
            </thead>
            <tbody>
              {(data?.items ?? []).map((p) => (
                <tr key={p.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50/40">
                  <td className="p-3">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-10 shrink-0 overflow-hidden rounded-lg bg-gray-100">
                        {p.imageUrl ? (
                          <img src={p.imageUrl} alt="" className="h-full w-full object-cover" />
                        ) : (
                          <div className="flex h-full items-center justify-center">🏋️</div>
                        )}
                      </div>
                      <span className="font-medium text-gray-700">{p.title}</span>
                    </div>
                  </td>
                  <td className="p-3 text-gray-500">{formatPrice(p.price)}</td>
                  <td className="p-3">
                    {p.discountedPrice != null ? (
                      <span className="font-medium text-green-600">{formatPrice(p.discountedPrice)}</span>
                    ) : (
                      <span className="text-gray-300">—</span>
                    )}
                  </td>
                  <td className="p-3">
                    <span className={`text-xs font-medium ${p.isActive ? 'text-green-600' : 'text-red-500'}`}>
                      {p.isActive ? 'فعال' : 'غیرفعال'}
                    </span>
                  </td>
                  <td className="p-3">
                    <div className="flex gap-1">
                      <Button size="sm" variant="secondary" onClick={() => openEdit(p)}>ویرایش</Button>
                      <Button size="sm" variant="danger" onClick={() => handleDelete(p)}>حذف</Button>
                    </div>
                  </td>
                </tr>
              ))}
              {(data?.items.length ?? 0) === 0 && (
                <tr>
                  <td colSpan={5} className="p-8 text-center text-gray-400">پکیجی ثبت نشده است.</td>
                </tr>
              )}
            </tbody>
          </table>
        )}
      </div>

      {data && data.totalPages > 1 && (
        <div className="flex justify-center gap-1">
          {Array.from({ length: data.totalPages }, (_, i) => i + 1).map((p) => (
            <button
              key={p}
              className={`h-9 w-9 rounded-lg text-sm ${p === page ? 'bg-primary-600 text-white' : 'bg-white border border-gray-200 text-gray-600 hover:bg-gray-100'}`}
              onClick={() => setPage(p)}
            >
              {p.toLocaleString('fa-IR')}
            </button>
          ))}
        </div>
      )}

      <Modal open={modalOpen} title={form.id ? 'ویرایش پکیج' : 'پکیج جدید'} onClose={() => setModalOpen(false)} wide>
        <form className="space-y-4" onSubmit={handleSubmit}>
          {error && <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

          <Field label="عنوان پکیج">
            <Input
              value={form.title}
              onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))}
              required
            />
          </Field>

          <Field label="توضیحات">
            <Textarea
              rows={4}
              value={form.description}
              onChange={(e) => setForm((f) => ({ ...f, description: e.target.value }))}
              required
            />
          </Field>

          <div className="grid grid-cols-2 gap-4">
            <Field label="قیمت (تومان)">
              <Input
                type="number"
                dir="ltr"
                min={0}
                value={form.price}
                onChange={(e) => setForm((f) => ({ ...f, price: e.target.value }))}
                required
              />
            </Field>
            <Field label="قیمت با تخفیف (اختیاری)">
              <Input
                type="number"
                dir="ltr"
                min={0}
                value={form.discountedPrice}
                onChange={(e) => setForm((f) => ({ ...f, discountedPrice: e.target.value }))}
              />
            </Field>
          </div>

          <div className="grid grid-cols-2 gap-4">
            <Field label="تصویر پکیج">
              <div className="flex items-center gap-2">
                <Button type="button" variant="secondary" onClick={() => imageInputRef.current?.click()}>
                  انتخاب تصویر
                </Button>
                <span className="truncate text-xs text-gray-500">
                  {imageFile ? imageFile.name : form.imageUrl ? 'تصویر موجود حفظ می‌شود' : 'تصویری انتخاب نشده'}
                </span>
              </div>
              <input
                ref={imageInputRef}
                type="file"
                accept="image/*"
                className="hidden"
                onChange={(e) => setImageFile(e.target.files?.[0] ?? null)}
              />
            </Field>

            <Field label="فایل پکیج (اختیاری)">
              <div className="flex items-center gap-2">
                <Button type="button" variant="secondary" onClick={() => fileInputRef.current?.click()}>
                  انتخاب فایل
                </Button>
                <span className="truncate text-xs text-gray-500">
                  {packageFile ? packageFile.name : form.fileUrl ? 'فایل موجود حفظ می‌شود' : 'فایلی انتخاب نشده'}
                </span>
              </div>
              <input
                ref={fileInputRef}
                type="file"
                accept=".pdf,.zip,.rar,.mp4,.mp3,.doc,.docx,.ppt,.pptx,.xls,.xlsx"
                className="hidden"
                onChange={(e) => setPackageFile(e.target.files?.[0] ?? null)}
              />
            </Field>
          </div>

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              className="h-4 w-4 accent-blue-600"
              checked={form.isActive}
              onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
            />
            فعال (برای کاربران نمایش داده شود)
          </label>

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
