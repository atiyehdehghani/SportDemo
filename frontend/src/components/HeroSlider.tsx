import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import type { SliderDto } from '../types';

export default function HeroSlider({ slides }: { slides: SliderDto[] }) {
  const [index, setIndex] = useState(0);

  useEffect(() => {
    if (slides.length <= 1) return;
    const timer = setInterval(() => setIndex((i) => (i + 1) % slides.length), 5000);
    return () => clearInterval(timer);
  }, [slides.length]);

  if (slides.length === 0) return null;

  return (
    <div className="relative h-[320px] overflow-hidden rounded-2xl bg-gray-200 md:h-[420px]">
      {slides.map((slide, i) => (
        <div
          key={slide.id}
          className={`absolute inset-0 transition-opacity duration-700 ${i === index ? 'opacity-100' : 'pointer-events-none opacity-0'}`}
        >
          <img src={slide.imageUrl} alt={slide.title} className="h-full w-full object-cover" />
          <div className="absolute inset-0 bg-gradient-to-l from-black/50 to-transparent" />
          <div className="absolute inset-0 flex flex-col items-start justify-center gap-3 p-8 md:p-14">
            <h2 className="max-w-lg text-2xl font-black text-white md:text-4xl">{slide.title}</h2>
            <p className="max-w-md text-sm text-gray-100 md:text-base">{slide.subtitle}</p>
            {slide.linkUrl && (
              <Link
                to={slide.linkUrl}
                className="mt-2 rounded-lg bg-white px-5 py-2.5 text-sm font-bold text-gray-900 hover:bg-gray-100"
              >
                مشاهده
              </Link>
            )}
          </div>
        </div>
      ))}

      {slides.length > 1 && (
        <div className="absolute bottom-4 left-1/2 flex -translate-x-1/2 gap-2">
          {slides.map((s, i) => (
            <button
              key={s.id}
              onClick={() => setIndex(i)}
              aria-label={`اسلاید ${i + 1}`}
              className={`h-2.5 rounded-full transition-all ${i === index ? 'w-8 bg-white' : 'w-2.5 bg-white/50 hover:bg-white/80'}`}
            />
          ))}
        </div>
      )}
    </div>
  );
}
