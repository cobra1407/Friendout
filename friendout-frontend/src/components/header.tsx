import { useAuth } from "@/features/auth/hooks/useAuth";
import { Button } from "./ui/button";
import { LogOut, Plus } from "lucide-react";
import { getTranslation } from "@/i18n";
import { getCurrentSeason } from "@/lib/utils/season.utils";
import summerLogo from "@/assets/images/friendout-logo-summer.png";
import winterLogo from "@/assets/images/friendout-logo-winter.png";
import autumnLogo from "@/assets/images/friendout-logo-autumn.png";
import springLogo from "@/assets/images/friendout-logo-spring.png";
import { useNavigate } from "react-router-dom";

interface HeaderProps {
    onCreateActivity: () => void;
    onLogout: () => void;
}

export const Header = ({ onCreateActivity, onLogout }: HeaderProps) => {
    const { user } = useAuth();
    const logoFriendout: Record<string, string> = {
        spring: springLogo,
        fall: autumnLogo,
        summer: summerLogo,
        autumn: autumnLogo,
        winter: winterLogo,
    };
    const logoPath = logoFriendout[getCurrentSeason()];
    const navigate = useNavigate();

    return (
        <header className="bg-white shadow-sm border-b h-[90px]">
            <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-full">
                <div className="flex justify-between items-center h-full flex-wrap">

                    {/* Logo + Welcome */}
                    <div className="flex items-center gap-2 flex-shrink-0">
                        <div className="w-[60px] sm:w-[100px] h-[60px] sm:h-[100px] flex items-center justify-center flex-shrink-0">
                            <img
                                src={logoPath}
                                alt={getTranslation('header.logo_alt')}
                                className="w-full h-full object-contain p-2 sm:p-4 cursor-pointer hover:opacity-80"
                                onClick={() => navigate('/activities')}
                                loading="eager"
                                width={100}
                                height={100}
                            />
                        </div>

                        <p className="text-sm text-muted-foreground h-5 flex items-center truncate max-w-[150px] sm:max-w-none">
                            {user ? getTranslation('header.welcome_user', { name: user.name ?? '' }) : getTranslation('header.welcome')}
                        </p>
                    </div>

                    {/* Buttons */}
                    <div className="flex items-center gap-2 flex-wrap mt-2 sm:mt-0">
                        <Button
                            className="flex items-center gap-2 h-10 px-3 sm:px-4"
                            onClick={onCreateActivity}
                        >
                            <Plus className="w-4 h-4" />
                            <span className="hidden sm:inline">{getTranslation('header.create_activity')}</span>
                        </Button>

                        <Button
                            variant="outline"
                            className="flex items-center gap-2 h-10 px-3 sm:px-4"
                            onClick={onLogout}
                        >
                            <LogOut className="w-4 h-4" />
                            <span className="hidden sm:inline">{getTranslation('header.logout')}</span>
                        </Button>
                    </div>

                </div>
            </div>
        </header>
    );
};
