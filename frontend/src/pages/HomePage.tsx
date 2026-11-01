import { useQuery } from '@tanstack/react-query';
import { Link } from 'react-router-dom';
import { getPackages } from '../api/packages';
import { getArticles } from '../api/articles';
import { getSliders } from '../api/sliders';
import HeroSlider from '../components/HeroSlider';
import PackageCard from '../components/PackageCard';
import { Spinner } from '../components/ui';
import { formatDate } from '../utils/format';

function SectionTitle({ title, linkTo, linkLabel }: { title: string; linkTo: string; linkLabel: string }) {
  return (
    <div className="mb-6 flex items-center justify-between">
      <h2 className="text-xl font-black text-gray-800 md:text-2xl">{title}</h2>
      <Link to={linkTo} className="text-sm font-medium text-primary-600 hover:text-primary-700">
        {linkLabel} ←
      </Link>
    </div>
  );
}

export default function HomePage() {
  const slidersQuery = useQuery({ queryKey: ['sliders'], queryFn: getSliders });
  const packagesQuery = useQuery({ queryKey: ['packages', 'home'], queryFn: () => getPackages({ pageSize: 6 }) });
  const articlesQuery = useQuery({ queryKey: ['articles', 'home'], queryFn: () => getArticles({ pageSize: 3 }) });

  const sliders = slidersQuery.data ?? [];
  const packages = packagesQuery.data?.items ?? [];
  const articles = articlesQuery.data?.items ?? [];

  return (
    <div className="mx-auto max-w-6xl space-y-14 px-4 py-6">
      {/* اسلایدر */}
      {slidersQuery.isLoading ? <Spinner /> : sliders.length > 0 ? <HeroSlider slides={sliders} /> : null}

      {/* پکیج‌ها */}
      <section>
        <SectionTitle title="پکیج‌های ورزشی" linkTo="/packages" linkLabel="همه پکیج‌ها" />
        {packagesQuery.isLoading ? (
          <Spinner />
        ) : packages.length === 0 ? (
          <p className="py-8 text-center text-gray-500">هنوز پکیجی ثبت نشده است.</p>
        ) : (
          <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
            {packages.map((pkg) => (
              <PackageCard key={pkg.id} pkg={pkg} />
            ))}
          </div>
        )}
      </section>

      {/* مقالات */}
      <section>
        <SectionTitle title="مقالات آموزشی" linkTo="/articles" linkLabel="همه مقالات" />
        {articlesQuery.isLoading ? (
          <Spinner />
        ) : articles.length === 0 ? (
          <p className="py-8 text-center text-gray-500">هنوز مقاله‌ای منتشر نشده است.</p>
        ) : (
          <div className="grid gap-6 md:grid-cols-3">
            {articles.map((article) => (
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
        )}
      </section>
    </div>
  );
}
