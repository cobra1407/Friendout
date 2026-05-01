import { useEffect } from 'react'

interface OgMeta {
    title: string
    description?: string
    imageUrl?: string
    url?: string
}

/**
 * Dynamically updates Open Graph meta tags in <head> for the current page.
 *
 * Why this matters: WhatsApp, Telegram, and other apps generate link previews
 * by reading og:title, og:description, and og:image from the shared URL.
 * Since Friendout is a SPA with no SSR, these tags are static by default.
 * This hook updates them client-side so previews show the activity title and image.
 */
export function useOgMeta({ title, description, imageUrl, url }: OgMeta) {
    useEffect(() => {
        const setMeta = (property: string, content: string) => {
            let el = document.querySelector<HTMLMetaElement>(`meta[property="${property}"]`)
            if (!el) {
                el = document.createElement('meta')
                el.setAttribute('property', property)
                document.head.appendChild(el)
            }
            el.setAttribute('content', content)
        }

        const previousTitle = document.title

        document.title = `${title} — Friendout`
        setMeta('og:title', title)
        setMeta('og:site_name', 'Friendout')
        setMeta('og:type', 'website')
        setMeta('og:url', url ?? window.location.href)

        if (description) setMeta('og:description', description)
        if (imageUrl)    setMeta('og:image', imageUrl)

        return () => {
            document.title = previousTitle
        }
    }, [title, description, imageUrl, url])
}
