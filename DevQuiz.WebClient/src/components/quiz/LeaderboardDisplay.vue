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
          class="flex items-center gap-4 p-4 rounded-xl bg-secondary"
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

defineProps<Props>()

const { fallbackUrl, handleImageError } = useAvatarFallback()
</script>