<template>
  <div class="p-4">
    <div class="flex justify-between items-center">
      <div class="text-sm">
        <span class="font-mono text-lg font-semibold text-white">{{ formattedElapsedTime }}</span>
      </div>

      <div v-if="displayPenaltyTime" class="text-sm text-red-500 font-medium">
        {{ displayPenaltyTime }}
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'

const props = defineProps<{
  sessionStartTime: Date | null
  totalPenaltyMs: number
}>()

const elapsedMs = ref(0)

const formattedElapsedTime = computed(() => {
  const totalSeconds = Math.floor(elapsedMs.value / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes.toString()}m ${seconds.toString().padStart(2, '0')}s`
})

const displayPenaltyTime = computed(() => {
  if (props.totalPenaltyMs > 0) {
    const penaltySeconds = Math.floor(props.totalPenaltyMs / 1000)
    return `+${penaltySeconds}s penalty`
  }
  return ''
})

let timerInterval: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  timerInterval = setInterval(() => {
    if (props.sessionStartTime) {
      elapsedMs.value = Date.now() - props.sessionStartTime.getTime()
    }
  }, 100)
})

onUnmounted(() => {
  if (timerInterval) {
    clearInterval(timerInterval)
  }
})
</script>
