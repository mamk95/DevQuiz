<template>
  <div class="relative w-full pt-16 bg-primary">
    <!-- Centered Logo -->
    <div class="flex items-center justify-center">
      <img v-if="!isDark" src="@/assets/logo/Logo lightmode.svg" alt="Logo" />
      <img v-else src="@/assets/logo/Logo darkmode.svg" alt="Logo" />
    </div>

    <!-- Dark Mode Toggle - Positioned to the right -->
    <div class="absolute top-0 right-4 flex items-center space-x-3 pt-16">
      <!-- Toggle Switch -->
      <button
        @click="toggle()"
        class="relative inline-flex h-6 w-11 items-center rounded-full transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2"
        :class="isDark ? 'bg-blue-600' : 'bg-gray-200'"
        :title="isDark ? 'Switch to light mode' : 'Switch to dark mode'"
      >
        <!-- Switch Circle -->
        <span
          class="inline-block h-4 w-4 transform rounded-full bg-white transition-transform duration-200"
          :class="isDark ? 'translate-x-6' : 'translate-x-1'"
        />
      </button>

      <!-- Sun/Moon Icon -->
      <div class="w-6 h-6 flex items-center justify-center">
        <!-- Sun icon for light mode -->
        <svg
          v-if="!isDark"
          class="w-6 h-6 text-yellow-500"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M12 3v1m0 16v1m9-9h-1M4 12H3m15.364 6.364l-.707-.707M6.343 6.343l-.707-.707m12.728 0l-.707.707M6.343 17.657l-.707.707M16 12a4 4 0 11-8 0 4 4 0 018 0z"
          />
        </svg>

        <!-- Moon icon for dark mode -->
        <svg v-else class="w-6 h-6 text-blue-400" fill="currentColor" viewBox="0 0 24 24">
          <path
            d="M20.354 15.354A9 9 0 018.646 3.646 9.003 9.003 0 0012 21a9.003 9.003 0 008.354-5.646z"
          />
        </svg>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useDark, useToggle } from '@vueuse/core'

const isDark = useDark({
  selector: 'html',
  attribute: 'class',
  valueDark: 'dark',
  valueLight: '',
  storageKey: 'theme-preference',
  storage: localStorage,
})

const toggle = useToggle(isDark)
</script>
