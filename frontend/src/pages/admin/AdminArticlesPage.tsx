import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useRef, useState } from 'react';
import { createArticle, deleteArticle, getArticles, updateArticle } from '../../api/articles';
import { getApiErrorMessage } from '../../api/client';
import { Button, Field, Input, Modal, Spinner, Textarea } from '../../components/ui';
import { formatDate } from '../../utils/format';
import type { ArticleDto } from '../../types';

interface ArticleFormState {
  id: string | null;
  title: string;
  summary: string;
  content: string;
  isPublished: boolean;
  coverImageUrl: string | null;
}

const emptyForm: ArticleFormState = {
  id: null,
  title: '',
  summary: '',
  content: '',
  isPublished: true,
  coverImageUrl: null,
};

export default function AdminArticlesPage() {
  const queryClient = useQueryClient();
  const [page, setPage] = useState(1);
  const [modalOpen, setModalOpen] = useState(false);
  const [form, setForm] = useState<ArticleFormState>(emptyForm);
  const [coverFile, setCoverFile] = useState<File | null>(null);
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const coverInputRef = useRef<HTMLInputElement>(null);

  const { data, isLoading } = useQuery({
    queryKey: ['admin-articles', page],
    queryFn: () => getArticles({ includeUnpublished: true, page, pageSize: 10 }),
  });

  const openCreate = () => {
    setForm(emptyForm);
    setCoverFile(null);
    setError('');
    setModalOpen(true);
  };

  const openEdit = (a: ArticleDto) => {
    setForm({
      id: a.id,
      title: a.title,
      summary: a.summary,
      content: a.content,
      isPublished: a.isPublished,
      coverImageUrl: a.coverImageUrl,
    });
    setCoverFile(null);
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
      fd.append('summary', form.summary.trim());
      fd.append('content', form.content.trim());
      fd.append('isPublished', String(form.isPublished));
      if (form.coverImageUrl) fd.append('coverImageUrl', form.coverImageUrl);
      if (coverFile) fd.append('coverImageFile', coverFile);

      if (form.id) {
        await updateArticle(form.id, fd);
      } else {
        await createArticle(fd);
      }

      setModalOpen(false);
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
      queryClient.invalidateQueries({ queryKey: ['articles'] });
    } catch (err) {
      setError(getApiErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const handleDelete = async (a: ArticleDto) => {
    if (!window.confirm(`حذف مقاله «${a.title}»؟`)) return;
    try {
      await deleteArticle(a.id);
      queryClient.invalidateQueries({ queryKey: ['admin-articles'] });
      queryClient.invalidateQueries({ queryKey: ['articles'] });
    } catch (err) {
      window.alert(getApiErrorMessage(err));
    }
  };

  return (
    <div className="space-y-4">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <h1 className="text-2xl font-black text-gray-800">مدیریت مقالات</h1>
        <Button onClick={openCreate}>+ مقاله جدید</Button>
      </div>

      <div className="overflow-x-auto rounded-2xl border border-gray-100 bg-white shadow-sm">
        {isLoading ? (
          <Spinner />
        ) : (
          <table className="w-full text-right text-sm">
            <thead>
              <tr className="border-b border-gray-100 bg-gray-50/60 text-xs text-gray-400">
                <th className="p-3 font-medium">عنوان</th>
                <th className="p-3 font-medium">وضعیت</th>
                <th className="p-3 font-medium">تاریخ انتشار</th>
                <th className="p-3 font-medium">عملیات</th>
              </tr>
            </thead>
            <tbody>
              {(data?.items ?? []).map((a) => (
                <tr key={a.id} className="border-b border-gray-50 last:border-0 hover:bg-gray-50/40">
                  <td className="p-3">
                    <div className="flex items-center gap-3">
                      <div className="h-10 w-16 shrink-0 overflow-hidden rounded-lg bg-gray-100">
                        {a.coverImageUrl ? (
                          <img src={a.coverImageUrl} alt="" className="h-full w-full object-cover" />
                        ) : (
                          <div className="flex h-full items-center justify-center">📝</div>
                        )}
                      </div>
                      <span className="font-medium text-gray-700">{a.title}</span>
                    </div>
                  </td>
                  <td className="p-3">
                    <span className={`text-xs font-medium ${a.isPublished ? 'text-green-600' : 'text-amber-600'}`}>
                      {a.isPublished ? 'منتشرشده' : 'پیش‌نویس'}
                    </span>
                  </td>
                  <td className="p-3 text-xs text-gray-400">{formatDate(a.publishedAtUtc)}</td>
                  <td className="p-3">
                    <div className="flex gap-1">
                      <Button size="sm" variant="secondary" onClick={() => openEdit(a)}>ویرایش</Button>
                      <Button size="sm" variant="danger" onClick={() => handleDelete(a)}>حذف</Button>
                    </div>
                  </td>
                </tr>
              ))}
              {(data?.items.length ?? 0) === 0 && (
                <tr>
                  <td colSpan={4} className="p-8 text-center text-gray-400">مقاله‌ای ثبت نشده است.</td>
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

      <Modal open={modalOpen} title={form.id ? 'ویرایش مقاله' : 'مقاله جدید'} onClose={() => setModalOpen(false)} wide>
        <form className="space-y-4" onSubmit={handleSubmit}>
          {error && <div className="rounded-lg bg-red-50 px-4 py-3 text-sm text-red-700">{error}</div>}

          <Field label="عنوان مقاله">
            <Input value={form.title} onChange={(e) => setForm((f) => ({ ...f, title: e.target.value }))} required />
          </Field>

          <Field label="خلاصه">
            <Textarea
              rows={2}
              maxLength={500}
              value={form.summary}
              onChange={(e) => setForm((f) => ({ ...f, summary: e.target.value }))}
              required
            />
          </Field>

          <Field label="متن مقاله">
            <Textarea
              rows={8}
              value={form.content}
              onChange={(e) => setForm((f) => ({ ...f, content: e.target.value }))}
              required
            />
          </Field>

          <Field label="تصویر کاور">
            <div className="flex items-center gap-2">
              <Button type="button" variant="secondary" onClick={() => coverInputRef.current?.click()}>
                انتخاب تصویر
              </Button>
              <span className="truncate text-xs text-gray-500">
                {coverFile ? coverFile.name : form.coverImageUrl ? 'تصویر موجود حفظ می‌شود' : 'اختیاری'}
              </span>
            </div>
            <input
              ref={coverInputRef}
              type="file"
              accept="image/*"
              className="hidden"
              onChange={(e) => setCoverFile(e.target.files?.[0] ?? null)}
            />
          </Field>

          <label className="flex items-center gap-2 text-sm text-gray-700">
            <input
              type="checkbox"
              className="h-4 w-4 accent-blue-600"
              checked={form.isPublished}
              onChange={(e) => setForm((f) => ({ ...f, isPublished: e.target.checked }))}
            />
            انتشار
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
