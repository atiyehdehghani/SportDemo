import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import { getCurrentUser } from '../api/auth';
import { api } from '../api/client';
import type { UserDto } from '../types';

interface AuthContextValue {
  user: UserDto | null;
  isLoading: boolean;
  setSession: (token: string, user: UserDto) => void;
  logout: () => void;
  isAdmin: boolean;
}

const AuthContext = createContext<AuthContextValue | null>(null);

const TOKEN_KEY = 'token';
const USER_KEY = 'user';

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserDto | null>(() => {
    const cached = localStorage.getItem(USER_KEY);
    return cached ? (JSON.parse(cached) as UserDto) : null;
  });
  const [isLoading, setIsLoading] = useState(() => Boolean(localStorage.getItem(TOKEN_KEY)));

  useEffect(() => {
    let cancelled = false;

    async function refresh() {
      try {
        const fresh = await getCurrentUser();
        if (!cancelled) {
          setUser(fresh);
          localStorage.setItem(USER_KEY, JSON.stringify(fresh));
        }
      } catch {
        if (!cancelled) {
          localStorage.removeItem(TOKEN_KEY);
          localStorage.removeItem(USER_KEY);
          setUser(null);
        }
      } finally {
        if (!cancelled) setIsLoading(false);
      }
    }

    if (localStorage.getItem(TOKEN_KEY)) {
      refresh();
    }

    return () => {
      cancelled = true;
    };
  }, []);

  const setSession = useCallback((token: string, newUser: UserDto) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(USER_KEY, JSON.stringify(newUser));
    api.defaults.headers.common.Authorization = `Bearer ${token}`;
    setUser(newUser);
    setIsLoading(false);
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    delete api.defaults.headers.common.Authorization;
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      user,
      isLoading,
      setSession,
      logout,
      isAdmin: user?.roles.includes('Admin') ?? false,
    }),
    [user, isLoading, setSession, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error('useAuth باید داخل AuthProvider استفاده شود.');
  return ctx;
}
