export function useAvatarFallback() {
  const fallbackUrl = new URL('@/assets/avatars/Avatar-rubber-duck.svg', import.meta.url).href

  const handleImageError = (event: Event) => {
    const target = event.target as HTMLImageElement
    if (target.src !== fallbackUrl) {
      target.src = fallbackUrl
    }
  }

  return {
    fallbackUrl,
    handleImageError
  }
}
