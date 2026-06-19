import { getTranslation } from "@/i18n"

/**
 * Resolves a notification's title and message from its type and payload,
 * using the user's current locale via getTranslation.
 *
 * The payload is a dict of template variables (e.g. ActivityName, OrganizerName)
 * that map to {{placeholders}} in the i18n strings.
 */
export function formatNotification(
    type: string,
    payload: Record<string, string>
): { title: string; message: string } {
    const knownTypes = [
        "ActivityModified",
        "ActivityCanceled",
        "ActivityReminder",
        "InvitationReceived",
        "AccessRequestApproved",
        "AccessRequestDenied",
        "AccountDeleted",
    ]

    const key = knownTypes.includes(type) ? type : "default"

    const title   = getTranslation(`notifications.types.${key}.title`,   payload)
    const message = getTranslation(`notifications.types.${key}.message`, payload)

    return { title, message }
}
