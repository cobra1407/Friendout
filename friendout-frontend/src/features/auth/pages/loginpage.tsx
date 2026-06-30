import { getTranslation } from "@/i18n";
import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { DiscordLoginButton } from "../components/DiscordLoginButton";
import { GoogleLoginButton } from "../components/GoogleLoginButton";
import { RequestAccessModal } from "../components/RequestAccessModal";
import { Carousel, CarouselContent, CarouselItem } from "@/components/ui/carousel";
import defaultImage1 from "@/assets/images/default-1.webp";
import defaultImage2 from "@/assets/images/default-2.webp";
import defaultImage3 from "@/assets/images/default-3.webp";
import { FontAwesomeIcon } from "@fortawesome/react-fontawesome";
import { faCalendarDay, faUsers, faGlassCheers } from "@fortawesome/free-solid-svg-icons";
import { authApi } from "@/features/auth/api/auth.api";

const ACCESS_DENIED_CODES = ["discord_access_denied", "google_access_denied"];

export const LoginPage = () => {
    const [requestModalOpen, setRequestModalOpen] = useState(false);
    const [deniedEmail, setDeniedEmail] = useState("");

    // Defaults to both available while loading, so the buttons aren't both hidden during
    // the brief fetch — a provider disappears only once we know for sure it's disabled.
    const { data: loginMethods } = useQuery({
        queryKey: ["auth", "login-methods"],
        queryFn: authApi.loginMethods,
    });
    const discordAvailable = loginMethods?.discordAvailable ?? true;
    const googleAvailable = loginMethods?.googleAvailable ?? true;

    useEffect(() => {
        const params = new URLSearchParams(window.location.search);
        const errorCode = params.get("error_code");
        if (errorCode) {
            const message = getTranslation(`errors.${errorCode}`);
            toast.error(message !== `errors.${errorCode}` ? message : getTranslation("errors.unknown_error"));

            // Auto-open the request modal when the user was denied access.
            if (ACCESS_DENIED_CODES.includes(errorCode)) {
                // Pre-fill the access request form with the email the user just tried to
                // sign in with (sent back by the backend after a Google whitelist rejection),
                const email = params.get("email");
                if (email) setDeniedEmail(email);
                setRequestModalOpen(true);
            }
        }
    }, []);

    const handleDiscordLogin = async () => {
        await authApi.discordLogin();
    };

    const handleGoogleLogin = async () => {
        await authApi.googleLogin();
    };

    return (
        <>
            <div className="min-h-screen flex items-center justify-center px-4 sm:px-6">
                <div className="flex flex-col md:flex-row w-full max-w-4xl overflow-hidden rounded-2xl shadow-2xl bg-card">
                    {/* LEFT LOGIN */}
                    <div className="flex w-full md:w-1/2 flex-col items-center justify-center px-6 sm:px-10 py-8 md:py-12 gap-6">
                        <h1 className="text-2xl sm:text-3xl font-bold text-center">
                            {getTranslation("login_page.title")}
                        </h1>
                        <div className="flex gap-4">
                            <button className="h-10 w-10 rounded-full bg-muted hover:bg-muted/80 flex items-center justify-center">
                                <FontAwesomeIcon icon={faCalendarDay} color="#3B82F6" />
                            </button>
                            <button className="h-10 w-10 rounded-full bg-muted hover:bg-muted/80 flex items-center justify-center">
                                <FontAwesomeIcon icon={faGlassCheers} color="#FBBF24" />
                            </button>
                            <button className="h-10 w-10 rounded-full bg-muted hover:bg-muted/80 flex items-center justify-center">
                                <FontAwesomeIcon icon={faUsers} color="#10B981" />
                            </button>
                        </div>
                        <p className="text-center text-base sm:text-lg md:text-xl text-muted-foreground mb-4 sm:mb-6">
                            {getTranslation("login_page.welcome_sentence")}
                        </p>
                        {discordAvailable && (
                            <DiscordLoginButton onClick={handleDiscordLogin}>
                                {getTranslation("login_page.login_button")}
                            </DiscordLoginButton>
                        )}
                        {googleAvailable && (
                            <GoogleLoginButton onClick={handleGoogleLogin}>
                                {getTranslation("login_page.login_button_google")}
                            </GoogleLoginButton>
                        )}

                        {/* Request access link */}
                        <p className="text-sm text-muted-foreground">
                            {getTranslation("login_page.request_access_link")}{" "}
                            <button
                                onClick={() => setRequestModalOpen(true)}
                                className="underline underline-offset-2 hover:text-foreground transition-colors text-blue-700 dark:text-blue-400 cursor-pointer font-semibold"
                            >
                                {getTranslation("login_page.request_access_button")}
                            </button>
                        </p>
                    </div>

                    {/* RIGHT CAROUSEL */}
                    <div className="relative w-full md:w-1/2 mt-6 md:mt-0 bg-muted/30 overflow-hidden rounded-b-2xl md:rounded-r-2xl">
                        <Carousel autoplayIntervalMs={6000} opts={{ loop: true }} className="h-64 sm:h-80 md:h-full w-full flex">
                            <CarouselContent className="h-full">
                                {[defaultImage1, defaultImage2, defaultImage3].map((img, index) => (
                                    <CarouselItem key={index} className="h-full w-full border-none">
                                        <div className="h-full w-full flex items-center justify-center overflow-hidden">
                                            <img src={img} alt={`Slide ${index + 1}`} className="h-full w-full object-cover object-center" />
                                        </div>
                                    </CarouselItem>
                                ))}
                            </CarouselContent>
                        </Carousel>
                        <div className="pointer-events-none absolute inset-0 bg-gradient-to-l from-background/40 to-transparent" />
                    </div>
                </div>
            </div>

            <RequestAccessModal
                open={requestModalOpen}
                onClose={() => setRequestModalOpen(false)}
                defaultEmail={deniedEmail}
            />
        </>
    );
};
