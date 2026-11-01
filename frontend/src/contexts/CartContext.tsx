import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useState,
  type ReactNode,
} from 'react';
import type { CartItem } from '../types';

interface CartContextValue {
  items: CartItem[];
  totalCount: number;
  totalAmount: number;
  addItem: (item: Omit<CartItem, 'quantity'>, quantity?: number) => void;
  removeItem: (packageId: string) => void;
  updateQuantity: (packageId: string, quantity: number) => void;
  clear: () => void;
}

const CartContext = createContext<CartContextValue | null>(null);

const CART_KEY = 'cart';

export function CartProvider({ children }: { children: ReactNode }) {
  const [items, setItems] = useState<CartItem[]>(() => {
    const cached = localStorage.getItem(CART_KEY);
    return cached ? (JSON.parse(cached) as CartItem[]) : [];
  });

  useEffect(() => {
    localStorage.setItem(CART_KEY, JSON.stringify(items));
  }, [items]);

  const addItem = useCallback((item: Omit<CartItem, 'quantity'>, quantity = 1) => {
    setItems((prev) => {
      const existing = prev.find((i) => i.packageId === item.packageId);
      if (existing) {
        return prev.map((i) =>
          i.packageId === item.packageId
            ? { ...i, quantity: Math.min(50, i.quantity + quantity) }
            : i,
        );
      }
      return [...prev, { ...item, quantity }];
    });
  }, []);

  const removeItem = useCallback((packageId: string) => {
    setItems((prev) => prev.filter((i) => i.packageId !== packageId));
  }, []);

  const updateQuantity = useCallback((packageId: string, quantity: number) => {
    setItems((prev) =>
      quantity <= 0
        ? prev.filter((i) => i.packageId !== packageId)
        : prev.map((i) => (i.packageId === packageId ? { ...i, quantity: Math.min(50, quantity) } : i)),
    );
  }, []);

  const clear = useCallback(() => setItems([]), []);

  const value = useMemo<CartContextValue>(() => {
    const totalCount = items.reduce((sum, i) => sum + i.quantity, 0);
    const totalAmount = items.reduce((sum, i) => sum + i.unitPrice * i.quantity, 0);
    return { items, totalCount, totalAmount, addItem, removeItem, updateQuantity, clear };
  }, [items, addItem, removeItem, updateQuantity, clear]);

  return <CartContext.Provider value={value}>{children}</CartContext.Provider>;
}

export function useCart(): CartContextValue {
  const ctx = useContext(CartContext);
  if (!ctx) throw new Error('useCart باید داخل CartProvider استفاده شود.');
  return ctx;
}
