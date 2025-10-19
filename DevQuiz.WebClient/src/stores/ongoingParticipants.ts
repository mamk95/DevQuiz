import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import type { OngoingParticipant } from '@/lib/signalrService'

const INACTIVE_THRESHOLD_MS = 100000 // 100 seconds

export const useOngoingParticipantsStore = defineStore('ongoingParticipants', () => {
  const participants = ref<Map<string, OngoingParticipant>>(new Map())

  const activeParticipants = computed(() => {
    const now = Date.now()
    return Array.from(participants.value.values())
      .filter(p => {
        const timeSinceLastActivity = now - p.lastActivityMs
        return timeSinceLastActivity < INACTIVE_THRESHOLD_MS
      })
      .sort((a, b) => b.lastActivityMs - a.lastActivityMs)
  })

  const noobParticipants = computed(() =>
    activeParticipants.value.filter(p => p.difficulty === 'Noob')
  )

  const nerdParticipants = computed(() =>
    activeParticipants.value.filter(p => p.difficulty === 'Nerd')
  )

  function addParticipant(participant: OngoingParticipant) {
    participants.value.set(participant.sessionId, participant)
  }

  function updateParticipant(participant: OngoingParticipant) {
    participants.value.set(participant.sessionId, participant)
  }

  function removeParticipant(sessionId: string) {
    participants.value.delete(sessionId)
  }

  function clearAll() {
    participants.value.clear()
  }

  // Clean up inactive participants periodically
  function cleanupInactive() {
    const now = Date.now()
    for (const [sessionId, participant] of participants.value.entries()) {
      const timeSinceLastActivity = now - participant.lastActivityMs
      if (timeSinceLastActivity >= INACTIVE_THRESHOLD_MS) {
        participants.value.delete(sessionId)
      }
    }
  }

  return {
    participants,
    activeParticipants,
    noobParticipants,
    nerdParticipants,
    addParticipant,
    updateParticipant,
    removeParticipant,
    clearAll,
    cleanupInactive
  }
})
