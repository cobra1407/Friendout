import * as React from "react"
import { X } from "lucide-react"
import { cn } from "@/lib/utils"

interface ModalProps {
    open: boolean
    onClose: () => void
    children: React.ReactNode
    className?: string
}

const Modal = ({ open, onClose, children, className }: ModalProps) => {
    React.useEffect(() => {
        if (!open) return
        const handler = (e: KeyboardEvent) => {
            if (e.key === "Escape") onClose()
        }
        document.addEventListener("keydown", handler)
        return () => document.removeEventListener("keydown", handler)
    }, [open, onClose])

    if (!open) return null

    return (
        <div className="fixed inset-0 z-50 flex items-center justify-center">
            {/* Overlay */}
            <div className="absolute inset-0 bg-black/50 cursor-pointer" onClick={onClose} />
            {/* Content */}
            <div className={cn(
                "relative z-50 w-full max-w-[calc(100%-2rem)] sm:max-w-lg bg-background rounded-lg border shadow-lg p-6 grid gap-4",
                className
            )}>
                <button
                    onClick={onClose}
                    className="absolute top-4 right-4 opacity-70 hover:opacity-100 transition-opacity rounded-xs focus:outline-none focus:ring-2 focus:ring-ring cursor-pointer"
                >
                    <X className="h-4 w-4 cursor-pointer" />
                    <span className="sr-only">Close</span>
                </button>
                {children}
            </div>
        </div>
    )
}

const ModalHeader = ({ className, ...props }: React.ComponentProps<"div">) => {
    return (
        <div className={cn("flex flex-col gap-2 text-left", className)} {...props} />
    )
}

const ModalTitle = ({ className, ...props }: React.ComponentProps<"h2">) => {
    return (
        <h2 className={cn("text-lg font-semibold leading-none", className)} {...props} />
    )
}

const ModalDescription = ({ className, ...props }: React.ComponentProps<"p">) => {
    return (
        <p className={cn("text-sm text-muted-foreground", className)} {...props} />
    )
}

export { Modal, ModalHeader, ModalTitle, ModalDescription }
