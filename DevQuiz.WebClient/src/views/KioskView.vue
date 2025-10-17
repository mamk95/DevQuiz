<template>
  <div class="KioskView bg-primary text-white p-8">
    <div class="flex gap-8">
      <LeaderboardDisplay
        title="Noob Quiz"
        quiz-name="noob"
        :leaderboard="noobLeaderboard"
        :format-time="formatTime"
      />

      <LeaderboardDisplay
        title="Nerd Quiz"
        quiz-name="nerd"
        :leaderboard="nerdLeaderboard"
        :format-time="formatTime"
      />

      <!-- QR Code -->
      <div class="w-[600px]">
        <div class="bg-secondary rounded-2xl p-8 flex flex-col items-center justify-center">
          <h2 class="text-3xl font-bold mb-6">Join the Quiz!</h2>

          <div class="bg-white p-6 rounded-lg mb-6">
            <canvas ref="qrCanvas" class="w-[500px] h-[500px]"></canvas>
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
import LeaderboardDisplay from '@/components/quiz/LeaderboardDisplay.vue'

const leaderboardStore = useLeaderboardStore()
const qrCanvas = ref<HTMLCanvasElement>()

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
        width: 500,
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

  // Prevent text selection on kiosk
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
}
</style>