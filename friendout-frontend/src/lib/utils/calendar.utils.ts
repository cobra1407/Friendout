interface CalendarEvent {
    id: string
    title: string
    description: string
    startAt: string
    endAt?: string | null
    location?: string | null
}

/**
 * Formats a date string to the iCalendar format (YYYYMMDDTHHmmssZ).
 */
function toIcsDate(dateStr: string): string {
    return new Date(dateStr)
        .toISOString()
        .replace(/[-:]/g, "")
        .replace(/\.\d{3}/, "")
}

/**
 * Generates and downloads an .ics file for the given event.
 * Compatible with Google Calendar, Outlook, and Apple Calendar.
 */
export function downloadIcs(event: CalendarEvent): void {
    const start = toIcsDate(event.startAt)
    const end = event.endAt
        ? toIcsDate(event.endAt)
        : toIcsDate(new Date(new Date(event.startAt).getTime() + 60 * 60 * 1000).toISOString())

    const location = event.location ?? ""
    const now = toIcsDate(new Date().toISOString())

    const ics = [
        "BEGIN:VCALENDAR",
        "VERSION:2.0",
        "PRODID:-//Friendout//EN",
        "CALSCALE:GREGORIAN",
        "METHOD:PUBLISH",
        "BEGIN:VEVENT",
        `UID:${event.id}@friendout`,
        `DTSTAMP:${now}`,
        `DTSTART:${start}`,
        `DTEND:${end}`,
        `SUMMARY:${event.title}`,
        `DESCRIPTION:${event.description.replace(/\n/g, "\\n")}`,
        `LOCATION:${location}`,
        "END:VEVENT",
        "END:VCALENDAR",
    ].join("\r\n")

    const blob = new Blob([ics], { type: "text/calendar;charset=utf-8" })
    const url = URL.createObjectURL(blob)
    const link = document.createElement("a")
    link.href = url
    link.download = `${event.title.replace(/\s+/g, "-")}.ics`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)
}

/**
 * Returns a Google Calendar "add event" URL for the given event.
 */
export function getGoogleCalendarUrl(event: CalendarEvent): string {
    const start = toIcsDate(event.startAt)
    const end = event.endAt
        ? toIcsDate(event.endAt)
        : toIcsDate(new Date(new Date(event.startAt).getTime() + 60 * 60 * 1000).toISOString())

    const params = new URLSearchParams({
        action: "TEMPLATE",
        text: event.title,
        dates: `${start}/${end}`,
        details: event.description,
        location: event.location ?? "",
    })

    return `https://calendar.google.com/calendar/render?${params.toString()}`
}

/**
 * Returns an Outlook Web "add event" URL for the given event.
 */
export function getOutlookCalendarUrl(event: CalendarEvent): string {
    const params = new URLSearchParams({
        path: "/calendar/action/compose",
        rru: "addevent",
        subject: event.title,
        startdt: new Date(event.startAt).toISOString(),
        enddt: event.endAt
            ? new Date(event.endAt).toISOString()
            : new Date(new Date(event.startAt).getTime() + 60 * 60 * 1000).toISOString(),
        body: event.description,
        location: event.location ?? "",
    })

    return `https://outlook.live.com/calendar/0/action/compose?${params.toString()}`
}
