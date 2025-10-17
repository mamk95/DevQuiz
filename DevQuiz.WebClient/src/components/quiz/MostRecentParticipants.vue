<template>
  <div class="space-y-3">
    <div
      v-for="(participant, index) in participants"
      :key="index"
      class="flex items-center gap-4 p-4 rounded-xl bg-secondary"
    >
      <img
        :src="participant.avatarUrl || fallbackUrl"
        :alt="`Avatar of ${participant.name || 'Unknown'}`"
        class="w-12 h-12 rounded-full object-cover"
        @error="handleImageError"
        loading="lazy"
        decoding="async"
      />
      <div class="flex-1 min-w-0">
        <p class="font-semibold text-lg truncate">{{ participant.name }}</p>
      </div>
      <div class="text-right">
        <p class="text-2xl font-bold font-mono whitespace-nowrap">
          {{ formatElapsedTime(index) }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, watch } from 'vue'
import type { MostRecentParticipant } from '@/lib/api'
import type { QuizDifficulty } from '@/types/quiz'
import { useAvatarFallback } from '@/composables/useAvatarFallback'

interface Props {
  participants: MostRecentParticipant[]
  difficulty: QuizDifficulty
}

const props = defineProps<Props>()

const emit = defineEmits<{
  celebrate: [name: string, position: number, difficulty: QuizDifficulty, index: number]
}>()

const { fallbackUrl, handleImageError } = useAvatarFallback()

const elapsedTimes = ref<number[]>([])
const completedParticipantIds = ref<Set<string>>(new Set())

let intervalId: ReturnType<typeof setInterval>

const initializeTimes = () => {
  elapsedTimes.value = props.participants.map(p => p.totalMs)
}

onMounted(() => {
  initializeTimes()
  
  intervalId = setInterval(() => {
    elapsedTimes.value = elapsedTimes.value.map(time => time + 100)
  }, 100)
})

onUnmounted(() => {
  if (intervalId) {
    clearInterval(intervalId)
  }
})

watch(() => props.participants, (newParticipants, oldParticipants) => {
  initializeTimes()
  
  // Check for newly completed participants
  newParticipants.forEach((participant, index) => {
    if (participant.completedAt && participant.position !== null && participant.position > 0) {
      // Create a unique ID based on name and completion time
      const participantId = `${participant.name}-${participant.completedAt}`
      
      // If this participant just completed (wasn't completed before)
      if (!completedParticipantIds.value.has(participantId)) {
        completedParticipantIds.value.add(participantId)

        emit('celebrate', participant.name, participant.position, props.difficulty, index)
      }

    }
  })
}, { deep: true })

const formatElapsedTime = (index: number) => {
  const ms = elapsedTimes.value[index] || 0
  const seconds = Math.floor(ms / 1000)
  const deciseconds = Math.floor((ms % 1000) / 100)
  return `${seconds}.${deciseconds}s`
}
</script>
