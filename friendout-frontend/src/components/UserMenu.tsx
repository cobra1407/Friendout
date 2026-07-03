import { useNavigate } from "react-router-dom";
import { Settings, Package, Shield, LogOut, ChevronDown } from "lucide-react";
import { getTranslation } from "@/i18n";
import { useAuth } from "@/features/auth/hooks/useAuth";
import { Popover, PopoverContent, PopoverTrigger } from "@/components/ui/popover";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { cn } from "@/lib/utils";
import { useState } from "react";

interface UserMenuProps {
    onLogout: () => void;
}

export const UserMenu = ({ onLogout }: UserMenuProps) => {
    const { user } = useAuth();
    const navigate = useNavigate();
    const [isPopoverOpen, setIsPopoverOpen] = useState(false);

    if (!user) return null;

    const isAdmin = user.role === "Admin";
    const initials = user.name
        .split(" ")
        .map((n) => n[0])
        .join("")
        .toUpperCase()
        .slice(0, 2);

    return (
        <Popover open={isPopoverOpen} onOpenChange={setIsPopoverOpen}>
            <PopoverTrigger asChild onClick={() => setIsPopoverOpen(prev => !prev)}>
                <button className={cn(
                    "group relative flex items-center rounded-full  p-1 cursor-pointer border border-transparent hover:border-border transition-all duration-300 ease-in-out overflow-hidden",
                    isPopoverOpen ? "max-w-[250px] bg-muted" : "max-w-[50px] hover:max-w-[250px] hover:bg-muted"
                )}>
                    <Avatar className="h-10 w-10 flex-shrink-0 shadow-sm">
                        <AvatarImage src={user.avatarUrl} alt={user.name} />
                        <AvatarFallback className="text-sm font-bold bg-avatar text-white">
                            {initials}
                        </AvatarFallback>
                    </Avatar>

                    <div className={cn(
                        "flex items-center gap-2 ml-2 pr-3 whitespace-nowrap transition-all duration-300",
                        isPopoverOpen ? "opacity-100" : "opacity-0 group-hover:opacity-100 group-hover:delay-100"
                    )}>
                        <span className="text-sm font-semibold text-foreground">
                            {user.name}
                        </span>
                        <ChevronDown className={cn(
                            "w-3.5 h-3.5 text-muted-foreground transition-transform duration-300",
                            isPopoverOpen && "rotate-180"
                        )} />
                    </div>
                </button>
            </PopoverTrigger>

            <PopoverContent align="end" sideOffset={8} className="w-64 p-0 overflow-hidden rounded-xl shadow-xl border-border">
                <div className="px-4 py-4 bg-muted/30">
                    <div className="flex items-center gap-3">
                        <Avatar className="w-10 h-10 border border-background">
                            <AvatarImage src={user.avatarUrl} alt={user.name} />
                            <AvatarFallback className="text-sm font-semibold bg-avatar text-white">
                                {initials}
                            </AvatarFallback>
                        </Avatar>
                        <div className="flex-1 min-w-0">
                            <p className="text-sm font-bold truncate text-foreground">{user.name}</p>
                            {user.email && (
                                <p className="text-xs text-muted-foreground truncate font-medium">{user.email}</p>
                            )}
                        </div>
                        {isAdmin && (
                            <Badge variant="outline" className="text-xs px-1.5 py-0 font-bold bg-destructive/15 text-destructive border-none">
                                Admin
                            </Badge>
                        )}
                    </div>
                </div>

                <Separator />

                <div className="p-2">
                    <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest px-3 py-2">
                        {getTranslation("user_menu.my_account")}
                    </p>
                    <MenuButton
                        icon={<Settings className="w-4 h-4" />}
                        label={getTranslation("user_menu.preferences")}
                        onClick={() => { setIsPopoverOpen(false); navigate("/preferences"); }}
                    />
                    <MenuButton
                        icon={<Package className="w-4 h-4" />}
                        label={getTranslation("user_menu.my_equipment")}
                        onClick={() => { setIsPopoverOpen(false); navigate("/equipment"); }}
                    />
                </div>

                {isAdmin && (
                    <>
                        <Separator className="mx-2 opacity-50" />
                        <div className="p-2">
                            <p className="text-[10px] font-bold text-muted-foreground uppercase tracking-widest px-3 py-2">
                                {getTranslation("user_menu.administration")}
                            </p>
                            <MenuButton
                                icon={<Shield className="w-4 h-4" />}
                                label={getTranslation("user_menu.admin_panel")}
                                onClick={() => { setIsPopoverOpen(false); navigate("/admin"); }}
                            />
                        </div>
                    </>
                )}

                <Separator />

                <div className="p-2">
                    <button
                        onClick={() => { setIsPopoverOpen(false); onLogout(); }}
                        className="flex items-center gap-3 w-full rounded-lg px-3 py-2.5 text-sm font-semibold text-destructive hover:bg-destructive/10 transition-colors group/logout cursor-pointer"
                    >
                        <LogOut className="w-4 h-4 group-hover/logout:-translate-x-0.5 transition-transform" />
                        {getTranslation("header.logout")}
                    </button>
                </div>
            </PopoverContent>
        </Popover>
    );
};

interface MenuButtonProps {
    icon: React.ReactNode;
    label: string;
    badge?: string;
    onClick: () => void;
}

const MenuButton = ({ icon, label, badge, onClick }: MenuButtonProps) => (
    <button
        onClick={onClick}
        className="flex items-center gap-3 w-full rounded-lg px-3 py-2 text-sm font-medium hover:bg-muted transition-colors text-left group/btn cursor-pointer"
    >
        <span className="text-muted-foreground group-hover/btn:text-primary transition-colors">{icon}</span>
        <span className="flex-1">{label}</span>
        {badge && (
            <Badge variant="secondary" className="text-[10px] px-1.5 py-0 font-bold bg-muted-foreground/10 text-muted-foreground border-none">
                {badge}
            </Badge>
        )}
    </button>
);
