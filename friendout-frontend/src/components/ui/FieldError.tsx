/** Affiche un message d'erreur inline sous un champ de formulaire. */
export function FieldError({ message }: { message?: string }) {
    if (!message) return null
    return (
        <p className="text-sm text-destructive mt-1" role="alert">
            {message}
        </p>
    )
}
