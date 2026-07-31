import moon from "@/assets/images/Moon-404-error.svg";
import { ErrorLayout } from "../layouts/ErrorLayout";
import { getTranslation } from "@/i18n";
import { Button } from "@/components/ui/button";
import { Home } from "lucide-react";

export const Error404Page = () => {
    const handleRedirect = () => {
        window.location.href = "/";
    };

    return (
        <ErrorLayout>
            <div className="flex flex-col items-center text-foreground px-6">
                <div className="flex items-center">
                    <span className="mx-2 sm:mx-4 md:mx-8 text-[16vw] font-bold text-[#15343e] dark:text-[#8fd3e0]">4</span>
                    <img
                        src={moon}
                        alt={getTranslation('error404.icon_alt')}
                        className="w-[40vw] max-w-[450px] animate-float"
                    />
                    <span className="mx-2 sm:mx-4 md:mx-8 text-[16vw] font-bold text-[#15343e] dark:text-[#8fd3e0]">4</span>
                </div>

                <p className="text-xl max-w-[500px] text-center text-foreground/80">
                    {getTranslation('error404.message')}
                </p>

                <Button className={" mt-8 flex items-center gap-2 h-9 px-3 sm:px-4 cursor-pointer"} onClick={handleRedirect}>
                    <Home className="w-4 h-4" />
                    <span className="text-sm">
                        {getTranslation('error404.back_home')}
                    </span>
                </Button>
            </div>
        </ErrorLayout>
    );
};
