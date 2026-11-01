import { useQuery } from '@tanstack/react-query';
import { useState } from 'react';
import { getPackages } from '../api/packages';
import PackageCard from '../components/PackageCard';
import { Input, Pagination, Spinner } from '../components/ui';

export default function PackagesPage() {
  const [search, setSearch] = useState('');
  const [query, setQuery] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading } = useQuery({
    queryKey: ['packages', { search: query, page }],
    queryFn: () => getPackages({ search: query || undefined, page, pageSize: 12 }),
  });

  return (
    <div className="mx-auto max-w-6xl px-4 py-8">
      <h1 className="mb-6 text-2xl font-black text-gray-800">پکیج‌های ورزشی</h1>

      <form
        className="mb-8 flex gap-2"
        onSubmit={(e) => {
          e.preventDefault();
          setQuery(search.trim());
          setPage(1);
        }}
      >
        <Input
          placeholder="جستجوی پکیج..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
        />
        <button className="shrink-0 rounded-lg bg-primary-600 px-5 text-sm font-medium text-white hover:bg-primary-700">
          جستجو
        </button>
      </form>

      {isLoading ? (
        <Spinner />
      ) : (data?.items.length ?? 0) === 0 ? (
        <p className="py-16 text-center text-gray-500">پکیجی یافت نشد.</p>
      ) : (
        <>
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {data!.items.map((pkg) => (
              <PackageCard key={pkg.id} pkg={pkg} />
            ))}
          </div>
          <Pagination page={page} totalPages={data!.totalPages} onChange={setPage} />
        </>
      )}
    </div>
  );
}
