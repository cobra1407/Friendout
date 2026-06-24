import { Upload, X } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Label } from "@/components/ui/label"
import { getTranslation } from "@/i18n"

interface ActivityImageUploadProps {
    image: string | null
    onUpload: (event: React.ChangeEvent<HTMLInputElement>) => void
    onRemove: () => void
}

/**
 * A component to upload and display an activity image.
 *
 * @param {string | null} image - The current image URL.
 * @param {(event: React.ChangeEvent<HTMLInputElement>) => void} onUpload - A callback function to handle image upload.
 * @param {() => void} onRemove - A callback function to handle image removal.
 * @returns {React.ReactNode} A React component with the image upload and display functionality.
 */
export function ActivityImageUpload({ image, onUpload, onRemove }: ActivityImageUploadProps) {
    return (
        <div className="space-y-2">
            <Label>{getTranslation("activity_form.image_label")}</Label>
            {!image ? (
                <div className="border-2 border-dashed border-muted-foreground/30 rounded-lg p-6 text-center">
                    <Upload className="w-8 h-8 text-muted-foreground mx-auto mb-2" />
                    <p className="text-sm text-muted-foreground mb-2">
                        {getTranslation("activity_form.image_upload_hint")}
                    </p>
                    <input
                        type="file"
                        accept="image/*"
                        onChange={onUpload}
                        className="hidden"
                        id="image-upload"
                    />
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => document.getElementById("image-upload")?.click()}
                    >
                        {getTranslation("activity_form.image_choose_button")}
                    </Button>
                </div>
            ) : (
                <div className="relative">
                    <img
                        src={image}
                        alt={getTranslation("activity_form.image_preview_alt")}
                        className="w-full h-48 object-cover rounded-lg"
                    />
                    <Button
                        type="button"
                        variant="destructive"
                        size="sm"
                        onClick={onRemove}
                        className="absolute top-2 right-2"
                    >
                        <X className="w-4 h-4" />
                    </Button>
                </div>
            )}
        </div>
    )
}
