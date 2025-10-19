<template>
  <div class="ongoing-participants">
    <h3 class="text-xl font-bold text-white mb-4">Active Participants</h3>

    <div v-if="props.participants.length === 0" class="text-gray-400 text-center py-4">
      No active participants
    </div>

    <div v-else class="space-y-2 max-h-96 overflow-y-auto">
      <div
        v-for="participant in props.participants"
        :key="participant.sessionId"
        class="participant-card bg-gray-800 rounded-lg p-3 flex items-center gap-3 transition-all duration-300"
      >
        <img
          :src="participant.avatarUrl"
          :alt="participant.name"
          class="w-12 h-12 rounded-full"
        />

        <div class="flex-1 min-w-0">
          <div class="font-semibold text-white truncate">{{ participant.name }}</div>
          <div class="text-sm text-gray-400">
            Question {{ participant.currentQuestionIndex + 1 }} / {{ participant.totalQuestions }}
          </div>
        </div>

        <div class="flex flex-col items-end gap-1">
          <div class="text-lg font-mono font-bold text-yellow-400">
            {{ formatElapsedTime(participant) }}
          </div>
          <div
            class="text-xs px-2 py-1 rounded"
            :class="participant.difficulty === 'Noob' ? 'bg-green-900 text-green-300' : 'bg-purple-900 text-purple-300'"
          >
            {{ participant.difficulty }}
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, onUnmounted, ref } from 'vue'
import type { OngoingParticipant } from '@/lib/signalrService'

const props = defineProps<{
  participants: OngoingParticipant[]
}>()

const now = ref(Date.now())
let intervalId: number | null = null

onMounted(() => {
  // Update every 100ms for smooth timer updates
  intervalId = window.setInterval(() => {
    now.value = Date.now()
  }, 100)
})

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})

function formatElapsedTime(participant: OngoingParticipant): string {
  // Calculate elapsed time including penalties
  const elapsedWithoutPenalty = now.value - participant.startedAtMs
  const totalElapsed = elapsedWithoutPenalty + participant.totalPenaltyMs

  // Format as seconds with 1 decimal place (like leaderboard)
  const seconds = totalElapsed / 1000
  return `${seconds.toFixed(1)}s`
}
</script>

<style scoped lang="scss">
.participant-card {
  animation: fadeIn 0.3s ease-in-out;
}

@keyframes fadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}
</style>
