import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { api } from '@/lib/api'

export const useSessionStore = defineStore('session', () => {
  const participantName = ref('')
  const phone = ref('')
  const avatar = ref('')
  const hasSession = ref(false)
  const currentQuestionIndex = ref(0)
  const totalQuestions = ref(0) // Will be set from API response
  const totalTimeMs = ref<number | null>(null)

  const isAuthenticated = computed(() => hasSession.value)

  async function startSession(name: string, phoneNumber: string, difficulty: string, avatarUrl: string) {
    try {
      const result = await api.startSession(name, phoneNumber, difficulty, avatarUrl)

      if (result.success) {
        participantName.value = name
        phone.value = phoneNumber
        avatar.value = avatarUrl 
        hasSession.value = true
        currentQuestionIndex.value = 0
        totalTimeMs.value = null
        
        if (typeof result.totalQuestions === 'number') {
          totalQuestions.value = result.totalQuestions
        }
      } else {
        hasSession.value = false
        throw new Error(result.message || 'Failed to start session')
      }
    } catch (error) {
      hasSession.value = false
      throw error
    }
  }

  function setTotalTime(ms: number) {
    totalTimeMs.value = ms
  }

  function incrementQuestionIndex() {
    currentQuestionIndex.value++
  }

  function clearSession() {
    participantName.value = ''
    phone.value = ''
    avatar.value = ''
    hasSession.value = false
    currentQuestionIndex.value = 0
    totalTimeMs.value = null
  }

  return {
    participantName,
    phone,
    hasSession,
    isAuthenticated,
    currentQuestionIndex,
    totalQuestions,
    totalTimeMs,
    startSession,
    setTotalTime,
    incrementQuestionIndex,
    clearSession
  }
})