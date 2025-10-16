<template>
  <div class="KioskView min-h-screen bg-primary text-white p-8">
    <div class="w-full h-full flex gap-8">
      <!-- Noob Quiz Leaderboard - Left Column -->
      <div class="flex-1 flex flex-col">
        <h1 class="text-4xl font-bold mb-6 text-center">Noob Quiz</h1>

        <div v-if="noobLeaderboard.loading" class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white"></div>
        </div>

        <div v-else-if="noobLeaderboard.entries.length === 0" class="text-center py-12 text-white/70">
          <p class="text-xl">No completions yet</p>
          <p class="mt-2">Be the first to complete the noob quiz!</p>
        </div>

        <div v-else-if="noobLeaderboard.entries.length < 3" class="flex-1">
          <LeaderboardPodium :entries="noobLeaderboard.entries" :format-time="formatTime" class="mb-8" />
        </div>

        <div v-else class="flex-1 flex flex-col">
          <LeaderboardPodium :entries="noobLeaderboard.entries" :format-time="formatTime" class="mb-8" />
          <div class="space-y-3 overflow-auto flex-1">
            <div
              v-for="(entry, index) in noobLeaderboard.entries.slice(3)"
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

      <!-- Nerd Quiz Leaderboard - Middle Column -->
      <div class="flex-1 flex flex-col">
        <h1 class="text-4xl font-bold mb-6 text-center">Nerd Quiz</h1>

        <div v-if="nerdLeaderboard.loading" class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white"></div>
        </div>

        <div v-else-if="nerdLeaderboard.entries.length === 0" class="text-center py-12 text-white/70">
          <p class="text-xl">No completions yet</p>
          <p class="mt-2">Be the first to complete the nerd quiz!</p>
        </div>

        <div v-else-if="nerdLeaderboard.entries.length < 3" class="flex-1">
          <LeaderboardPodium :entries="nerdLeaderboard.entries" :format-time="formatTime" class="mb-8" />
        </div>

        <div v-else class="flex-1 flex flex-col">
          <LeaderboardPodium :entries="nerdLeaderboard.entries" :format-time="formatTime" class="mb-8" />
          <div class="space-y-3 overflow-auto flex-1">
            <div
              v-for="(entry, index) in nerdLeaderboard.entries.slice(3)"
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

      <!-- QR Code - Right Column -->
      <div class="w-96 flex flex-col">
        <div class="bg-secondary rounded-2xl p-8 flex flex-col items-center justify-center flex-1">
          <h2 class="text-3xl font-bold mb-6">Join the Quiz!</h2>

          <div class="bg-white p-6 rounded-lg mb-6">
            <canvas ref="qrCanvas" class="max-w-full"></canvas>
          </div>

          <div class="text-center">
            <p class="text-xl font-mono mb-2">{{ quizUrl }}</p>
            <p class="text-white/70 text-lg">Scan or visit to start</p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useLeaderboardStore } from '@/stores/leaderboard'
import QRCode from 'qrcode'
import LeaderboardPodium from '@/components/quiz/LeaderboardPodium.vue'
import { useAvatarFallback } from '@/composables/useAvatarFallback'

const leaderboardStore = useLeaderboardStore()
const qrCanvas = ref<HTMLCanvasElement>()
const { fallbackUrl, handleImageError } = useAvatarFallback()

const quizUrl = window.location.origin

const noobLeaderboard = leaderboardStore.noobLeaderboard
const nerdLeaderboard = leaderboardStore.nerdLeaderboard

let pollInterval: ReturnType<typeof setInterval>

onMounted(() => {
  generateQRCode()
  loadLeaderboards()
  pollInterval = setInterval(loadLeaderboards, 10000)
})

onUnmounted(() => {
  clearInterval(pollInterval)
})

const formatTime = (ms: number) => {
  const seconds = ms / 1000
  return `${seconds.toFixed(1)}s`
}

const loadLeaderboards = async () => {
  try {
    await leaderboardStore.fetchBothLeaderboards(15)
  } catch {
    // Silently ignore - store handles error state
  }
}

const generateQRCode = async () => {
  if (qrCanvas.value) {
    try {
      await QRCode.toCanvas(qrCanvas.value, quizUrl, {
        width: 300,
        margin: 2,
        color: {
          dark: '#1e3a8a',
          light: '#ffffff',
        },
      })
    } catch {
      // Silently ignore QR generation failure
    }
  }
}
</script>

<style scoped lang="scss">
.KioskView {
  // Desktop-only optimizations
  min-width: 1400px; // Minimum width for desktop displays
  overflow: hidden;

  // Prevent text selection on kiosk
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
}

// Custom scrollbar for the leaderboard lists
.overflow-auto {
  scrollbar-width: thin;
  scrollbar-color: rgba(255, 255, 255, 0.3) transparent;

  &::-webkit-scrollbar {
    width: 8px;
  }

  &::-webkit-scrollbar-track {
    background: transparent;
  }

  &::-webkit-scrollbar-thumb {
    background-color: rgba(255, 255, 255, 0.3);
    border-radius: 4px;

    &:hover {
      background-color: rgba(255, 255, 255, 0.5);
    }
  }
}
</style>