import type { ReactNode } from 'react';
import { Particles } from '@/components/ui/particles';
import { useTheme } from '@/contexts/ThemeContext';

interface ErrorLayoutProps {
  children: ReactNode;
}

/**
 * Layout for error pages.
 * Uses the app's theme background/foreground so it matches whatever
 * base theme and accent the user has selected in their preferences,
 * plus an animated particle background whose color follows the same theme.
 */
export const ErrorLayout = ({ children }: ErrorLayoutProps) => {
  const { baseTheme } = useTheme();
  // Light dots on dark backgrounds, dark dots on light backgrounds.
  const particleColor = baseTheme === 'dark' ? '#ffffff' : '#0f172a';

  return (
    <div className="relative min-h-screen w-full overflow-hidden bg-background text-foreground">
      <Particles
        className="absolute inset-0 bg-transparent"
        quantity={80}
        color={particleColor}
        ease={70}
      />
      <div className="relative z-10 flex flex-col items-center justify-center min-h-screen w-full">
        {children}
      </div>
    </div>
  );
};
