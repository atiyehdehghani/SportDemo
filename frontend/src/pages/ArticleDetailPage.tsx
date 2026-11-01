import { useQuery } from '@tanstack/react-query';
import { Link, useParams } from 'react-router-dom';
import { getArticleBySlug } from '../api/articles';
import { Spinner } from '../components/ui';
import { formatDate } from '../utils/format';

export default function ArticleDetailPage() {
  const { slug } = useParams<{ slug: string }>();

  const { data: article, isLoading, error } = useQuery({
    queryKey: ['article', slug],
    queryFn: () => getArticleBySlug(slug!),
    enabled: Boolean(slug),
  });

  if (isLoading) return <Spinner />;
  if (error || !article) {
    return (
      <div className="py-20 text-center">
        <p className="mb-4 text-gray-500">مقاله مورد نظر یافت نشد.</p>
        <Link to="/articles" className="text-primary-600 hover:underline">بازگشت به لیست مقالات</Link>
      </div>
    );
  }

  return (
    <article className="mx-auto max-w-3xl px-4 py-8">
      <nav className="mb-6 text-sm text-gray-500">
        <Link to="/" className="hover:text-primary-600">خانه</Link>
        <span className="mx-2">/</span>
        <Link to="/articles" className="hover:text-primary-600">مقالات</Link>
      </nav>

      <h1 className="mb-3 text-2xl font-black leading-relaxed text-gray-800 md:text-3xl">{article.title}</h1>
      <span className="mb-6 block text-sm text-gray-400">{formatDate(article.publishedAtUtc)}</span>

      {article.coverImageUrl && (
        <img
          src={article.coverImageUrl}
          alt={article.title}
          className="mb-6 aspect-[16/9] w-full rounded-2xl object-cover"
        />
      )}

      <div className="whitespace-pre-line text-base leading-8 text-gray-700">{article.content}</div>
    </article>
  );
}
