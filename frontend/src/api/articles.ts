import { api } from './client';
import type { ArticleDto, PagedResult } from '../types';

export interface ArticleListParams {
  search?: string;
  includeUnpublished?: boolean;
  page?: number;
  pageSize?: number;
}

export async function getArticles(params: ArticleListParams = {}): Promise<PagedResult<ArticleDto>> {
  const { data } = await api.get<PagedResult<ArticleDto>>('/articles', { params });
  return data;
}

export async function getArticleBySlug(slug: string): Promise<ArticleDto> {
  const { data } = await api.get<ArticleDto>(`/articles/${slug}`);
  return data;
}

export async function getArticleById(id: string): Promise<ArticleDto> {
  const { data } = await api.get<ArticleDto>(`/articles/id/${id}`);
  return data;
}

export async function createArticle(form: FormData): Promise<ArticleDto> {
  const { data } = await api.post<ArticleDto>('/articles', form);
  return data;
}

export async function updateArticle(id: string, form: FormData): Promise<ArticleDto> {
  const { data } = await api.put<ArticleDto>(`/articles/${id}`, form);
  return data;
}

export async function deleteArticle(id: string): Promise<void> {
  await api.delete(`/articles/${id}`);
}
