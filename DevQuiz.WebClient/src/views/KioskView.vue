<template>
  <div class="KioskView min-h-screen bg-gradient-to-br from-blue-900 to-blue-700 text-white p-8">
    <div class="max-w-7xl mx-auto h-full flex gap-8">
      <div class="flex-1 bg-white/10 backdrop-blur rounded-2xl p-8">
        <h1 class="text-4xl font-bold mb-8 text-center">Leaderboard</h1>

        <div v-if="loading" class="flex justify-center py-12">
          <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-white"></div>
        </div>

        <div v-else-if="leaderboard.length === 0" class="text-center py-12 text-white/70">
          <p class="text-xl">No completions yet</p>
          <p class="mt-2">Be the first to complete the quiz!</p>
        </div>

        <div v-else class="space-y-3">
          <div
            v-for="(entry, index) in leaderboard"
            :key="index"
            class="flex items-center gap-4 p-4 rounded-lg bg-white/10 backdrop-blur hover:bg-white/20 transition-colors"
          >
            <div
              class="w-12 h-12 rounded-full flex items-center justify-center font-bold text-lg"
              :class="getRankClass(index + 1)"
            >
              {{ index + 1 }}
            </div>
            <div class="flex-1">
              <p class="font-semibold text-lg">{{ entry.name }}</p>
            </div>
            <div class="text-right">
              <p class="text-2xl font-bold font-mono">{{ formatTime(entry.totalMs) }}</p>
            </div>
          </div>
        </div>
      </div>

      <div class="w-96 bg-white/10 backdrop-blur rounded-2xl p-8 flex flex-col items-center justify-center">
        <h2 class="text-2xl font-bold mb-6">Join the Quiz!</h2>

        <div class="bg-white p-4 rounded-lg mb-6">
          <canvas ref="qrCanvas" class="max-w-full"></canvas>
        </div>

        <div class="text-center">
          <p class="text-lg font-mono">{{ quizUrl }}</p>
          <p class="mt-2 text-white/70">Scan or visit to start</p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useLeaderboardStore } from '@/stores/leaderboard'
import type { LeaderboardEntry } from '@/lib/api'
import QRCode from 'qrcode'

const leaderboardStore = useLeaderboardStore()
const qrCanvas = ref<HTMLCanvasElement>()

const loading = ref(true)
const leaderboard = ref<LeaderboardEntry[]>([])
const quizUrl = window.location.origin
const isInitialLoad = ref(true)

let pollInterval: ReturnType<typeof setInterval>

onMounted(() => {
  generateQRCode()
  loadLeaderboard()
  pollInterval = setInterval(loadLeaderboard, 10000)
})

onUnmounted(() => {
  clearInterval(pollInterval)
})

const formatTime = (ms: number) => {
  const seconds = ms / 1000
  return `${seconds.toFixed(3)}s`
}

const getRankClass = (rank: number) => {
  switch (rank) {
    case 1:
      return 'bg-yellow-500 text-yellow-900'
    case 2:
      return 'bg-gray-300 text-gray-900'
    case 3:
      return 'bg-orange-600 text-orange-100'
    default:
      return 'bg-white/20'
  }
}

const loadLeaderboard = async () => {
  if (isInitialLoad.value) {
    loading.value = true
  }
  try {
    const newData = await leaderboardStore.fetchLeaderboard()
    leaderboard.value = newData
  } catch {
    // Failed to load leaderboard
  } finally {
    if (isInitialLoad.value) {
      loading.value = false
      isInitialLoad.value = false
    }
  }
}

const generateQRCode = async () => {
  if (qrCanvas.value) {
    try {
      await QRCode.toCanvas(qrCanvas.value, quizUrl, {
        width: 256,
        margin: 2,
        color: {
          dark: '#1e3a8a',
          light: '#ffffff'
        }
      })
    } catch {
      // Failed to generate QR code
    }
  }
}
</script>

<style scoped lang="scss"></style>