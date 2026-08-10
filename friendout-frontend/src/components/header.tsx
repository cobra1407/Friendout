import { useMemo } from "react";
import { useNavigate } from "react-router";
import { Eye, PartyPopper, Compass, Heart, Sparkles, Search, ThumbsUp, Gift } from "lucide-react";
import { UserMenu } from "./UserMenu";
import { getTranslation } from "@/i18n";
import { getCurrentSeason } from "@/lib/utils/season.utils";
import summerLogo from "@/assets/images/friendout-summer.svg";
import winterLogo from "@/assets/images/friendout-winter.svg";
import autumnLogo from "@/assets/images/friendout-autumn.svg";
import springLogo from "@/assets/images/friendout-spring.svg";
import CreateActivityButton from "@/features/activity/components/CreateActivityButton";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";

interface HeaderProps {
    onCreateActivity?: () => void;
    onLogout?: () => void;
    isPublicPage?: boolean;
}

export const Header = ({ onCreateActivity, onLogout, isPublicPage = false }: HeaderProps) => {
    const { user } = useAuth();
    const navigate = useNavigate();

    const TAGLINE_ICONS = [Eye, PartyPopper, Compass, Heart, Sparkles, Search, ThumbsUp, Gift];
    const taglineIndex = useMemo(() => Math.floor(Math.random() * TAGLINE_ICONS.length), []);
    const tagline = getTranslation(`public_activity_page.header_tagline_${taglineIndex + 1}`);
    const TaglineIcon = TAGLINE_ICONS[taglineIndex];

    const logoFriendout: Record<string, string> = {
        spring: springLogo,
        summer: summerLogo,
        autumn: autumnLogo,
        winter: winterLogo,
    };
    const logoPath = logoFriendout[getCurrentSeason()];

    const handleLogoClick = () => {
        if (isPublicPage && !user) {
            navigate('/');
        } else {
            navigate('/activities');
        }
    };

    return (
        <header className="bg-background shadow-sm border-b h-[90px]">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full">
                <div className="flex items-center justify-between h-full gap-4">

                    {/* Logo & subtitle */}
                    <div className="flex items-center gap-3 flex-shrink-0">
                        <div className="w-[60px] sm:w-[100px] h-[80px] sm:h-[100px] flex items-center justify-center flex-shrink-0">
                            <img
                                src={logoPath}
                                alt={getTranslation('header.logo_alt')}
                                className="w-full h-full object-contain sm:p-1 cursor-pointer hover:opacity-80 transition-opacity"
                                onClick={handleLogoClick}
                                loading="eager"
                                width={100}
                                height={100}
                            />
                        </div>

                        <div className="hidden sm:block">
                            <p
                                key={isPublicPage ? tagline : undefined}
                                className="text-sm text-muted-foreground h-5 flex items-center gap-1.5 truncate max-w-[150px] sm:max-w-none animate-in fade-in slide-in-from-bottom-1 duration-500"
                            >
                                {isPublicPage ? (
                                    <>
                                        <TaglineIcon className="w-3.5 h-3.5 shrink-0 text-primary" />
                                        {tagline}
                                    </>
                                ) : (
                                    user ? getTranslation('header.welcome_user', { name: user.name ?? '' }) : getTranslation('header.welcome')
                                )}
                            </p>
                        </div>
                    </div>

                    {/* Actions */}
                    {!isPublicPage && (
                        <div className="flex items-center gap-2">
                            {onCreateActivity && <CreateActivityButton onCreateActivity={onCreateActivity} />}
                            <NotificationBell />
                            {onLogout && <UserMenu onLogout={onLogout} />}
                        </div>
                    )}
                </div>
            </div>
        </header>
    );
};
