import type { ReactNode } from 'react';

interface ErrorLayoutProps {
  children: ReactNode;
}

/**
 * Layout for error pages with particles in the background
 * Uses a dark background that is independent of the theme
 * Applies the error-page class to the body element
 */
export const ErrorLayout = ({ children }: ErrorLayoutProps) => {
  return (
    <div className="relative min-h-screen w-full overflow-hidden">
      <div className="relative z-10 flex flex-col items-center justify-center min-h-screen w-full text-white">
        {children}
      </div>
    </div>
  );
};
