interface FieldErrorProps {
    message?: string
}

/**
 * Displays an error message for a form field if the message is not null or undefined.
 *
 * @param {FieldErrorProps} props - The props object containing the error message.
 * @returns {React.ReactElement | null} - The rendered error message element or null if no message is provided.
 */
export function FieldError({ message }: FieldErrorProps) {
    if (!message) return null
    return (
        <p className="text-sm text-red-500 mt-1" role="alert">
            {message}
        </p>
    )
}
