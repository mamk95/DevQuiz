import { fileURLToPath, URL } from 'node:url'

import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// https://vite.dev/config/
export default defineConfig({
  server: {
    port: 3000,
  },
  plugins: [
    vue(),
  ],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    },
  },
  build: {
    assetsInlineLimit: (filePath) => {
      // Never inline SVG files that contain "avatar" or "Avatar" - always treat them as file paths
      if (filePath.endsWith('.svg') && (filePath.includes('avatar') || filePath.includes('Avatar'))) {
        return false
      }
      // Use default behavior for other files (4KB limit)
      return undefined
    },
  },
})
