import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRef, useState } from 'react';
import { createSlider, deleteSlider, getAllSliders, updateSlider } from '../../api/sliders';
import { getApiErrorMessage } from '../../api/client';
import { Button, Field, Input, Modal, Spinner } from '../../components/ui';
import type { SliderDto } from '../../types';

interface SliderFormState {
  id: string | null;
  title: string;
  subtitle: string;
  linkUrl: string;
  displayOrder: string;
  isActive: boolean;
  imageUrl: string | null;
}

const emptyForm: SliderFormState = {
  id: null,
  title: '',
  subtitle: '',
  linkUrl: '',
  displayOrder: '1',
  isActive: true,
  imageUrl: null,
};

export default function AdminSlidersPage() {
  const queryClient = useQueryClient();
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<SliderFormState>(emptyForm);
  const [imageFile, setImageFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const imageInputRef = useRef<HTMLInputElement>(null);

  const { data: sliders, isLoading } = useQuery({ queryKey: ['admin-sliders'], queryFn: getAllSliders });

  const openCreate = () => {
    setForm(emptyForm);
    setImageFile(null);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (s: SliderDto) => {
    setForm({
      id: s.id,
      title: s.title,
      subtitle: s.subtitle,
      linkUrl: s.linkUrl ?? '',
      displayOrder: String(s.displayOrder),
      isActive: s.isActive,
      imageUrl: s.imageUrl,
    });
    setImageFile(null);
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
      fd.append('subtitle', form.subtitle.trim());
      fd.append('displayOrder', form.displayOrder || '0');
      fd.append('isActive', String(form.isActive));
      if (form.linkUrl.trim()) fd.append('linkUrl', form.linkUrl.trim());
      if (form.imageUrl) fd.append('imageUrl', form.imageUrl);
      if (imageFile) fd.append('imageFile', imageFile);

      if (form.id) {
        await updateSlider(form.id, fd);
      } else {
        await createSlider(fd);
      }

      setModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['admin-sliders'] });
      queryClient.invalidateQueries({ queryKey: ['sliders'] });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (id: string, title: string) => {
    if (!window.confirm(`حذف اسلاید «${title}»؟`)) return;
    try {
      await deleteSlider(id);
      queryClient.invalidateQueries({ queryKey: ['admin-sliders'] });
      queryClient.invalidateQueries({ queryKey: ['sliders'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-black text-gray-800">مدیریت اسلایدر</h1>
        <Button onClick={openCreate}>+ اسلاید جدید</Button>
      </div>

      {isLoading ? (
        <Spinner />
      ) : (
        <div className="space-y-3">
          {(sliders ?? []).map((s) => (
            <div
              key={s.id}
              className="flex items-center gap-4 rounded-2xl border border-gray-100 bg-white p-3 shadow-sm"
            >
              <div className="h-16 w-28 shrink-0 overflow-hidden rounded-lg bg-gray-100">
                <img src={s.imageUrl} alt={s.title} className="h-full w-full object-cover" />
              </div>
              <div className="min-w-0 flex-1">
                <p className="font-bold text-gray-800">{s.title}</p>
                <p className="truncate text-sm text-gray-500">{s.subtitle}</p>
              </div>
              <span className="text-xs text-gray-400">ترتیب: {s.displayOrder.toLocaleString('fa-IR')}</span>
              <span className={`text-xs font-medium ${s.isActive ? 'text-green-600' : 'text-red-500'}`}>
                {s.isActive ? 'فعال' : 'غیرفعال'}
              </span>
              <div className="flex gap-1">
                <Button size="sm" variant="secondary" onClick={() => openEdit(s)}>
                  ویرایش
                </Button>
                <Button size="sm" variant="danger" onClick={() => handleDelete(s.id, s.title)}>حذف</Button>
              </div>
            </div>
          ))}
          {(sliders?.length ?? 0) === 0 && (
            <p className="rounded-2xl bg-white p-8 text-center text-gray-400">اسلایدی ثبت نشده است.</p>
          )}
        </div>
      )}

      <Modal open={modalOpen} title={form.id ? 'ویرایش اسلاید' : 'اسلاید جدید'} onClose={() => setModalOpen(false)}>
        <form className="space-y-4" onSubmit={handleSubmit}>
          {error && <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

          <Field label="عنوان">
            <Input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))} required />
          </Field>

          <Field label="زیرعنوان">
            <Input value={form.subtitle} onChange={(e) => setForm((f) => ({ ...f, subtitle: e.target.value }))} />
          </Field>

          <Field label="لینک (اختیاری، مثل /packages)">
            <Input dir="ltr" value={form.linkUrl} onChange={(e) => setForm((f) => ({ ...f, linkUrl: e.target.value }))} />
          </Field>

          <div className="grid grid-cols-2 gap-4">
            <Field label="ترتیب نمایش">
              <Input
                type="number"
                dir="ltr"
                value={form.displayOrder}
                onChange={(e) => setForm((f) => ({ ...f, displayOrder: e.target.value }))}
              />
            </Field>

            <Field label="تصویر اسلاید">
              <div className="flex items-center gap-2">
                <Button type="button" variant="secondary" onClick={() => imageInputRef.current?.click()}>
                  انتخاب تصویر
                </Button>
                <span className="truncate text-xs text-gray-500">
                  {imageFile ? imageFile.name : form.imageUrl ? 'تصویر موجود حفظ می‌شود' : 'الزامی'}
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
          </div>

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              className="h-4 w-4 accent-blue-600"
              checked={form.isActive}
              onChange={(e) => setForm((f) => ({ ...f, isActive: e.target.checked }))}
            />
            فعال
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
