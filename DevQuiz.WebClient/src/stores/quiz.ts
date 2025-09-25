import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api, type Question, type AnswerResponse } from '@/lib/api'
import { useSessionStore } from './session'

export const useQuizStore = defineStore('quiz', () => {
  const currentQuestion = ref<Question | null>(null)
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function getCurrentQuestion(): Promise<Question | null> {
    loading.value = true
    error.value = null

    try {
      const question = await api.getCurrentQuestion()
      currentQuestion.value = question
      return question
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load question'
      currentQuestion.value = null
      throw err
    } finally {
      loading.value = false
    }
  }

  async function submitAnswer(answerText: string): Promise<AnswerResponse> {
    error.value = null

    try {
      const response = await api.submitAnswer(answerText)

      if (response.correct) {
        const sessionStore = useSessionStore()
        sessionStore.incrementQuestionIndex()

        if (response.quizCompleted && response.totalMs) {
          sessionStore.setTotalTime(response.totalMs)
        }
      }

      return response
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to submit answer'
      throw err
    }
  }

  function clearQuiz() {
    currentQuestion.value = null
    loading.value = false
    error.value = null
  }

  return {
    currentQuestion,
    loading,
    error,
    getCurrentQuestion,
    submitAnswer,
    clearQuiz
  }
})