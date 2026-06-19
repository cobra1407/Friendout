import { useNavigate } from "react-router-dom";
import { UserMenu } from "./UserMenu";
import { getTranslation } from "@/i18n";
import { getCurrentSeason } from "@/lib/utils/season.utils";
import summerLogo from "@/assets/images/friendout-logo-summer.png";
import winterLogo from "@/assets/images/friendout-logo-winter.png";
import autumnLogo from "@/assets/images/friendout-logo-autumn.png";
import springLogo from "@/assets/images/friendout-logo-spring.png";
import CreateActivityButton from "@/features/activity/components/CreateActivityButton";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { NotificationBell } from "@/features/notifications/components/NotificationBell";

interface HeaderProps {
    onCreateActivity: () => void;
    onLogout: () => void;
}

export const Header = ({ onCreateActivity, onLogout }: HeaderProps) => {
    const { user } = useAuth();
    const navigate = useNavigate();

    const logoFriendout: Record<string, string> = {
        spring: springLogo,
        summer: summerLogo,
        autumn: autumnLogo,
        winter: winterLogo,
    };
    const logoPath = logoFriendout[getCurrentSeason()];

    return (
        <header className="bg-white shadow-sm border-b h-[90px]">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full">
                <div className="flex items-center justify-between h-full gap-4">

                    {/* Logo */}
                    <div className="flex items-center gap-3 flex-shrink-0">
                        <div className="w-[60px] sm:w-[100px] h-[60px] sm:h-[100px] flex items-center justify-center flex-shrink-0">
                            <img
                                src={logoPath}
                                alt={getTranslation('header.logo_alt')}
                                className="w-full h-full object-contain sm:p-4 cursor-pointer hover:opacity-80 transition-opacity"
                                onClick={() => navigate('/activities')}
                                loading="eager"
                                width={100}
                                height={100}
                            />
                        </div>
                        <div className="hidden sm:block">
                            <p className="text-sm text-muted-foreground h-5 flex items-center truncate max-w-[150px] sm:max-w-none">
                                {user ? getTranslation('header.welcome_user', { name: user.name ?? '' }) : getTranslation('header.welcome')}
                            </p>
                        </div>
                    </div>

                    {/* Actions */}
                    <div className="flex items-center gap-2">
                        <CreateActivityButton onCreateActivity={onCreateActivity} />
                        <NotificationBell />
                        <UserMenu onLogout={onLogout} />
                    </div>

                </div>
            </div>
        </header>
    );
};
