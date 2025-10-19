<template>
  <div class="flex-1 flex flex-col">
    <h1 class="text-4xl font-bold mb-6 text-center">{{ title }}</h1>

    <div v-if="leaderboard.loading" class="flex justify-center py-12">
      <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white"></div>
    </div>

    <div v-else-if="leaderboard.entries.length === 0" class="text-center py-12 text-white/70">
      <p class="text-xl">No completions yet</p>
      <p class="mt-2">Be the first to complete the {{ quizName }} quiz!</p>
    </div>

    <div v-else-if="leaderboard.entries.length < 3">
      <LeaderboardPodium :entries="leaderboard.entries" :format-time="formatTime" class="mb-8" />
    </div>

    <div v-else>
      <LeaderboardPodium :entries="leaderboard.entries" :format-time="formatTime" class="mb-8" />
      <div class="space-y-3">
        <div
          v-for="(entry, index) in leaderboard.entries.slice(3)"
          :key="index"
          class="flex items-center gap-4 p-4 rounded-xl bg-secondary transition-all duration-500"
          :class="{ 'spotlight-entry': isSpotlighted(entry) }"
        >
          <div class="w-12 h-12 rounded-full bg-primary/30 flex items-center justify-center font-bold text-lg">
            {{ index + 4 }}
          </div>
          <img
            :src="entry.avatarUrl || fallbackUrl"
            :alt="`Avatar of ${entry.name || 'Unknown'}`"
            class="w-12 h-12 rounded-full object-cover"
            @error="handleImageError"
            loading="lazy"
            decoding="async"
          />
          <div class="flex-1 min-w-0">
            <p class="font-semibold text-lg truncate">{{ entry.name }}</p>
          </div>
          <div class="text-right">
            <p class="text-2xl font-bold font-mono whitespace-nowrap">{{ formatTime(entry.totalMs) }}</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { LeaderboardEntry } from '@/lib/api'
import LeaderboardPodium from '@/components/quiz/LeaderboardPodium.vue'
import { useAvatarFallback } from '@/composables/useAvatarFallback'

interface LeaderboardData {
  entries: LeaderboardEntry[]
  loading: boolean
  error: string | null
}

interface Props {
  title: string
  quizName: string
  leaderboard: LeaderboardData
  formatTime: (ms: number) => string
}

const props = defineProps<Props>()

const { fallbackUrl, handleImageError } = useAvatarFallback()

// Track newly added entries for spotlight animation
const spotlightedNames = ref<Set<string>>(new Set())
const previousEntries = ref<string[]>([])
const isInitialized = ref(false)

// Watch for new entries and apply spotlight
watch(
  () => props.leaderboard.entries,
  (newEntries) => {
    const newNames = newEntries.map(e => e.name)

    // On first load with actual data, just initialize without animation
    if (!isInitialized.value && newNames.length > 0) {
      previousEntries.value = newNames
      isInitialized.value = true
      return
    }

    // Skip if not initialized yet (waiting for first real data)
    if (!isInitialized.value) {
      return
    }

    // Find entries that are new
    newNames.forEach(name => {
      if (!previousEntries.value.includes(name)) {
        spotlightedNames.value.add(name)

        // Remove spotlight after 2 seconds
        setTimeout(() => {
          spotlightedNames.value.delete(name)
        }, 2000)
      }
    })

    previousEntries.value = newNames
  },
  { deep: true, immediate: true }
)

function isSpotlighted(entry: LeaderboardEntry): boolean {
  return spotlightedNames.value.has(entry.name)
}
</script>

<style scoped lang="scss">
.spotlight-entry {
  animation: spotlight 2s ease-in-out;
}

@keyframes spotlight {
  0% {
    transform: scale(1);
    box-shadow: 0 0 0 rgba(255, 215, 0, 0);
  }
  50% {
    transform: scale(1.05);
    box-shadow: 0 0 30px rgba(255, 215, 0, 0.8), 0 0 60px rgba(255, 215, 0, 0.4);
  }
  100% {
    transform: scale(1);
    box-shadow: 0 0 0 rgba(255, 215, 0, 0);
  }
}
</style>