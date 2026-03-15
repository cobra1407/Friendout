import { FontAwesomeIcon } from '@fortawesome/react-fontawesome';
import { faDiscord } from '@fortawesome/free-brands-svg-icons';
import {type ReactNode, useState } from 'react';
import { Spinner } from '@/components/ui/spinner';

interface DiscordLoginButtonProps {
  onClick: () => void;
   children: ReactNode;
}

export const DiscordLoginButton = ({ onClick, children }: DiscordLoginButtonProps) => {
  const [isLoading, setIsLoading] = useState(false);

  const handleOnclick = () => {
    setIsLoading(true);
    onClick();
  }

  return (
    <button
      onClick={handleOnclick}
      className="
        flex items-center justify-center gap-2
        px-5 py-2.5
        rounded-lg
        bg-[#5865F2]/80
        text-white font-semibold
        transition-all duration-300
        hover:backdrop-blur-md
        hover:shadow-[0_0_15px_#5865F2]
      "
      disabled={isLoading}
    >
      {isLoading ? (
        <Spinner className="w-5 h-5" />
      ) : (
        <FontAwesomeIcon icon={faDiscord} className="w-5 h-5" />
      )}
      <span>{children}</span>
    </button>
  );
};
