import { Palette } from "lucide-react"
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card"
import { ThemeToggle } from "@/components/ThemeToggle"
import { getTranslation } from "@/i18n"
import ThemeLivePreview from "./ThemeLivePreview"
import { Switch } from "@/components/ui/switch"
import { Label } from "@/components/ui/label"
import { useState, useRef } from "react"

export const PreferencesThemeSection = () => {
    const [showPreview, setShowPreview] = useState(false);
    const previewRef = useRef<HTMLDivElement>(null);

    const handleTogglePreview = (value: boolean) => {
        setShowPreview(value);
        if (value) {
            setTimeout(() => {
                previewRef.current?.scrollIntoView({ behavior: "smooth", block: "start" });
            }, 320);
        }
    };

    return (
        <Card className="border shadow-sm h-full">
            <CardHeader className="pb-3 flex flex-row items-center justify-between">
                <div className="flex items-center gap-2">
                    <div className="p-1.5 rounded-lg bg-violet-50 dark:bg-violet-950/40">
                        <Palette className="w-4 h-4 text-violet-600 dark:text-violet-400" />
                    </div>
                    <div className="flex flex-col">
                        <CardTitle className="text-base">{getTranslation("preferences.theme.title")}</CardTitle>
                        <CardDescription className="text-xs">{getTranslation("preferences.theme.description")}</CardDescription>
                    </div>
                </div>
                <div className="flex items-center space-x-2">
                    <Switch
                        id="preview-mode"
                        checked={showPreview}
                        onCheckedChange={handleTogglePreview}
                    />
                    <Label htmlFor="preview-mode" className="text-sm font-medium text-muted-foreground cursor-pointer">
                        {getTranslation("preferences.theme.show_preview_button")}
                    </Label>
                </div>
            </CardHeader>
            <CardContent className="pt-0">
                <div className="flex flex-col gap-6">
                    {/* Theme settings */}
                    <ThemeToggle />

                    {/* Live preview */}
                    <div
                        className={`grid transition-all duration-300 ease-in-out ${showPreview ? "grid-rows-[1fr] opacity-100" : "grid-rows-[0fr] opacity-0"
                            }`}
                    >
                        <div ref={previewRef} className="overflow-hidden">
                            <ThemeLivePreview className="flex flex-col items-center justify-center lg:mt-0 pt-4 border-t" />
                        </div>
                    </div>
                </div>
            </CardContent>
        </Card>
    )
}
