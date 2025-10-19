<template>
  <div class="completion-animations">
    <!-- Toast notifications -->
    <div class="toast-container fixed top-4 right-4 z-50 space-y-2">
      <div
        v-for="toast in toasts"
        :key="toast.id"
        class="toast bg-gray-800 border-2 border-yellow-400 rounded-lg p-4 shadow-xl flex items-center gap-3 min-w-80 animate-slide-in-right"
      >
        <img :src="toast.avatarUrl" :alt="toast.name" class="w-12 h-12 rounded-full" />
        <div class="flex-1">
          <div class="font-bold text-white">{{ toast.name }}</div>
          <div class="text-sm text-gray-300">Completed in {{ formatTime(toast.totalMs) }}!</div>
          <div v-if="toast.ranking" class="text-xs text-yellow-400">
            Rank #{{ toast.ranking }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import confetti from 'canvas-confetti'
import type { ParticipantCompletion } from '@/lib/signalrService'

interface Toast {
  id: number
  name: string
  avatarUrl: string
  totalMs: number
  ranking: number
}

const toasts = ref<Toast[]>([])
let toastIdCounter = 0

function formatTime(ms: number): string {
  const seconds = ms / 1000
  return `${seconds.toFixed(1)}s`
}

function showToast(completion: ParticipantCompletion) {
  const toast: Toast = {
    id: toastIdCounter++,
    name: completion.name,
    avatarUrl: completion.avatarUrl,
    totalMs: completion.totalMs,
    ranking: completion.ranking
  }

  toasts.value.push(toast)

  // Auto-remove after 5 seconds
  setTimeout(() => {
    const index = toasts.value.findIndex(t => t.id === toast.id)
    if (index > -1) {
      toasts.value.splice(index, 1)
    }
  }, 5000)
}

function triggerConfetti(ranking: number) {
  const colors = getConfettiColors(ranking)

  // Center burst
  confetti({
    particleCount: 150,
    spread: 70,
    origin: { y: 0.6 },
    colors
  })

  // Side bursts
  setTimeout(() => {
    confetti({
      particleCount: 100,
      angle: 60,
      spread: 55,
      origin: { x: 0 },
      colors
    })
  }, 150)

  setTimeout(() => {
    confetti({
      particleCount: 100,
      angle: 120,
      spread: 55,
      origin: { x: 1 },
      colors
    })
  }, 300)
}

function getConfettiColors(ranking: number): string[] {
  if (ranking === 1) {
    return ['#FFD700', '#FFA500', '#FFFF00'] // Gold
  } else if (ranking === 2) {
    return ['#C0C0C0', '#D3D3D3', '#E8E8E8'] // Silver
  } else if (ranking === 3) {
    return ['#CD7F32', '#A0522D', '#8B4513'] // Bronze
  }
  return ['#FFD700', '#C0C0C0', '#CD7F32']
}

function handleCompletion(completion: ParticipantCompletion) {
  if (completion.isTopThree) {
    // Top 3: Confetti + Toast
    triggerConfetti(completion.ranking)
    showToast(completion)
  } else if (completion.isOnLeaderboard) {
    // 4-10: Just toast
    showToast(completion)
  } else {
    // Outside leaderboard: Just toast
    showToast(completion)
  }
}

defineExpose({
  handleCompletion
})
</script>

<style scoped lang="scss">
@keyframes slide-in-right {
  from {
    transform: translateX(100%);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

.animate-slide-in-right {
  animation: slide-in-right 0.3s ease-out;
}

.toast {
  transition: all 0.3s ease-out;
}
</style>
