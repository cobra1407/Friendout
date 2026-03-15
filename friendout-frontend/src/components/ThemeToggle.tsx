import { useTheme, type Theme } from '@/contexts/ThemeContext';
import { cn } from '@/lib/utils';

const themes: { value: Theme; label: string; color: string }[] = [
  { value: 'light', label: 'Light', color: 'bg-yellow-400' },
  { value: 'dark', label: 'Dark', color: 'bg-gray-800' },
  { value: 'purple', label: 'Purple', color: 'bg-purple-600' },
  { value: 'blue', label: 'Blue', color: 'bg-blue-600' },
  { value: 'green', label: 'Green', color: 'bg-green-600' },
];

export const ThemeToggle = () => {
  const { theme, setTheme } = useTheme();

  return (
    <div className="flex items-center gap-2 p-2 bg-card rounded-lg border border-border">
      <span className="text-sm font-medium text-foreground">Theme:</span>
      <div className="flex gap-1">
        {themes.map((t) => (
          <button
            key={t.value}
            onClick={() => setTheme(t.value)}
            className={cn(
              'px-3 py-1.5 rounded-md text-sm font-medium transition-all',
              'hover:bg-accent hover:text-accent-foreground',
              theme === t.value
                ? 'bg-primary text-primary-foreground'
                : 'text-muted-foreground'
            )}
            title={t.label}
          >
            {t.label}
          </button>
        ))}
      </div>
    </div>
  );
};
