import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { useState } from 'react';
import { getArticles } from '../api/articles';
import { Input, Pagination, Spinner } from '../components/ui';
import { formatDate } from '../utils/format';

export default function ArticlesPage() {
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['articles', { search: query, page }],
    queryFn: () => getArticles({ search: query || undefined, page, pageSize: 12 }),
  });

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-black text-gray-800">مقالات آموزشی</h1>

      <form
        className="mb-8 flex gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          setQuery(search.trim());
          setPage(1);
        }}
      >
        <Input placeholder="جستجوی مقاله..." value={search} onChange={(e) => setSearch(e.target.value)} />
        <button className="shrink-0 rounded-lg bg-primary-600 px-5 text-sm font-medium text-white hover:bg-primary-700">
          جستجو
        </button>
      </form>

      {isLoading ? (
        <Spinner />
      ) : (data?.items.length ?? 0) === 0 ? (
        <p className="py-16 text-center text-gray-500">مقاله‌ای یافت نشد.</p>
      ) : (
        <>
          <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
            {data!.items.map((article) => (
              <Link
                key={article.id}
                to={`/articles/${article.slug}`}
                className="group overflow-hidden rounded-2xl border border-gray-100 bg-white shadow-sm transition-all hover:-translate-y-1 hover:shadow-lg"
              >
                <div className="aspect-[16/9] overflow-hidden bg-gray-100">
                  {article.coverImageUrl ? (
                    <img
                      src={article.coverImageUrl}
                      alt={article.title}
                      className="h-full w-full object-cover transition-transform duration-300 group-hover:scale-105"
                    />
                  ) : (
                    <div className="flex h-full items-center justify-center text-4xl">📝</div>
                  )}
                </div>
                <div className="space-y-2 p-4">
                  <h3 className="font-bold text-gray-800 group-hover:text-primary-600">{article.title}</h3>
                  <p className="line-clamp-2 text-sm leading-6 text-gray-500">{article.summary}</p>
                  <span className="block text-xs text-gray-400">{formatDate(article.publishedAtUtc)}</span>
                </div>
              </Link>
            ))}
          </div>
          <Pagination page={page} totalPages={data!.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}
