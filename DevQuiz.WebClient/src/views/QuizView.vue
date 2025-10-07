<template>
  <div class="QuizView min-h-screen p-4">
    <QuizLoading v-if="quizStore.loading" />

    <div v-else-if="currentQuestion && currentQuestion.prompt" class="max-w-3xl mx-auto pt-8">
      <!-- Timer Display -->
      <div class="bg-secondary rounded-lg shadow-md mb-4 p-4">
        <div class="flex justify-between items-center">
          <div class="text-sm">
            Total Time: <span class="font-mono text-lg font-semibold text-blue-600">{{ formattedElapsedTime }}</span>
          </div>
          <div v-if="displayPenaltyTime" class="text-sm text-red-500 font-medium">
            {{ displayPenaltyTime }}
          </div>
        </div>
      </div>

      <div class="bg-secondary rounded-lg shadow-lg overflow-hidden">
        <!-- Header -->
        <QuizHeader
          :current-index="sessionStore.currentQuestionIndex"
          :total-questions="sessionStore.totalQuestions"
          :question-type="currentQuestion.type"
        />

        <!-- Question Content -->
        <div class="p-6">
          <h2 class="text-xl font-semibold mb-6">{{ currentQuestion.prompt }}</h2>

          <!-- Multiple Choice -->
          <MultipleChoiceQuestion
            v-if="currentQuestion.type === 'MultipleChoice'"
            :choices="currentQuestion.choices"
            :disabled="submitting"
            :selected-choice="selectedChoice"
            :show-result="showResult"
            :last-answer="lastAnswer"
            :was-correct="wasCorrect"
            @submit="submitMultipleChoice"
          />

          <!-- Code Fix -->
          <CodeFixQuestion
            v-else-if="currentQuestion.type === 'CodeFix'"
            v-model="codeAnswer"
            :placeholder="currentQuestion.initialCode"
            :test-code="currentQuestion.testCode"
            :test-result="testResult"
            :disabled="submitting"
            @update:test-result="testResult = $event"
            @submit="submitCodeFix"
          />
        </div>

        <!-- Penalty Message -->
        <PenaltyMessage :message="penaltyMessage" />
      </div>
    </div>

    <div v-else class="flex items-center justify-center min-h-screen">
      <div class="text-center">
        <p class="text-gray-600">No question available</p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/session'
import { useQuizStore } from '@/stores/quiz'
import QuizHeader from '@/components/quiz/QuizHeader.vue'
import QuizLoading from '@/components/quiz/QuizLoading.vue'
import MultipleChoiceQuestion from '@/components/quiz/MultipleChoiceQuestion.vue'
import CodeFixQuestion from '@/components/quiz/CodeFixQuestion.vue'
import PenaltyMessage from '@/components/quiz/PenaltyMessage.vue'

const router = useRouter()
const sessionStore = useSessionStore()
const quizStore = useQuizStore()

const submitting = ref(false)
const codeAnswer = ref('')
const penaltyMessage = ref('')
const selectedChoice = ref<string | null>(null)
const lastAnswer = ref<string | null>(null)
const wasCorrect = ref(false)
const showResult = ref(false)
const testResult = ref<boolean | null>(null)

const sessionStartTime = ref<Date | null>(null)
const elapsedMs = ref(0)
const totalPenaltyMs = ref(0)

const currentQuestion = computed(() => quizStore.currentQuestion)

const formattedElapsedTime = computed(() => {
  const totalSeconds = Math.floor(elapsedMs.value / 1000)
  const minutes = Math.floor(totalSeconds / 60)
  const seconds = totalSeconds % 60
  return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`
})

const displayPenaltyTime = computed(() => {
  if (totalPenaltyMs.value > 0) {
    const penaltySeconds = Math.floor(totalPenaltyMs.value / 1000)
    return `+${penaltySeconds}s penalty`
  }
  return ''
})

let penaltyTimeout: ReturnType<typeof setTimeout>
let resultTimeout: ReturnType<typeof setTimeout>
let timerInterval: ReturnType<typeof setInterval> | null = null

onMounted(() => {
  if (!sessionStore.hasSession) {
    router.push('/')
    return
  }
  
  timerInterval = setInterval(() => {
    if (sessionStartTime.value) {
      elapsedMs.value = Date.now() - sessionStartTime.value.getTime()
    }
  }, 100)
  
  loadCurrentQuestion()
})

onUnmounted(() => {
  clearTimeout(penaltyTimeout)
  clearTimeout(resultTimeout)
  if (timerInterval) {
    clearInterval(timerInterval)
  }
})

// Initialize code editor with initial code when question loads
watch(currentQuestion, (newQuestion) => {
  if (newQuestion?.type === 'CodeFix' && newQuestion.initialCode) {
    codeAnswer.value = newQuestion.initialCode
  } else {
    codeAnswer.value = ''
  }
  selectedChoice.value = null
  lastAnswer.value = null
  showResult.value = false
  testResult.value = null
})

const submitMultipleChoice = async (answer: string) => {
  if (submitting.value) return

  selectedChoice.value = answer
  submitting.value = true
  penaltyMessage.value = ''
  showResult.value = false

  try {
    const result = await quizStore.submitAnswer(answer)
    wasCorrect.value = result.correct
    lastAnswer.value = answer

    if (result.correct) {
      if (result.totalPenaltyMs !== undefined) {
        totalPenaltyMs.value = result.totalPenaltyMs
      }
      if (result.quizCompleted) {
        if (timerInterval) {
          clearInterval(timerInterval)
          timerInterval = null
        }
        sessionStore.setTotalTime(result.totalMs!)
        await router.push('/finish')
        return
      }
      await loadCurrentQuestion()
    } else {
      if (result.penaltyMsAdded) {
        showPenalty(result.penaltyMsAdded)
      }
      if (result.totalPenaltyMs !== undefined) {
        totalPenaltyMs.value = result.totalPenaltyMs
      }
      showResult.value = true
      clearTimeout(resultTimeout)
      resultTimeout = setTimeout(() => {
        showResult.value = false
        selectedChoice.value = null
      }, 2000)
    }
  } catch {
    // Failed to submit answer
  } finally {
    submitting.value = false
  }
}

const submitCodeFix = async () => {
  if (submitting.value || !codeAnswer.value.trim()) return
  submitting.value = true
  penaltyMessage.value = ''

  try {
    const result = await quizStore.submitAnswer(codeAnswer.value)
    testResult.value = result.correct

    if (result.correct) {
      if (result.totalPenaltyMs !== undefined) {
        totalPenaltyMs.value = result.totalPenaltyMs
      }
      if (result.quizCompleted) {
          if (timerInterval) {
            clearInterval(timerInterval)
            timerInterval = null
          }
        sessionStore.setTotalTime(result.totalMs!)
        await router.push('/finish')
        return
      }
      codeAnswer.value = ''
      testResult.value = null
      await loadCurrentQuestion()
    } else {
      if (result.penaltyMsAdded) {
        showPenalty(result.penaltyMsAdded)
      }
      if (result.totalPenaltyMs !== undefined) {
        totalPenaltyMs.value = result.totalPenaltyMs
      }
      setTimeout(() => {
        testResult.value = null
      }, 3000)
    }
  } catch {
    // Failed to submit answer
  } finally {
    submitting.value = false
  }
}

const showPenalty = (penaltyMs: number) => {
  const penaltySeconds = Math.floor(penaltyMs / 1000)
  penaltyMessage.value = `Incorrect! +${penaltySeconds} second${penaltySeconds === 1 ? '' : 's'} penalty`
  clearTimeout(penaltyTimeout)
  penaltyTimeout = setTimeout(() => {
    penaltyMessage.value = ''
  }, 3000)
}

const loadCurrentQuestion = async () => {
  try {
    const question = await quizStore.getCurrentQuestion()
    
    if (question?.sessionStartedAtUtc && !sessionStartTime.value) {
      sessionStartTime.value = question.sessionStartedAtUtc
      elapsedMs.value = Date.now() - sessionStartTime.value.getTime()
    }
    
    if (question?.done) {
      sessionStore.setTotalTime(question.totalMs!)
      router.push('/finish')
    }
  } catch {
    // Failed to load question, redirect to home
    router.push('/')
  }
}
</script>
