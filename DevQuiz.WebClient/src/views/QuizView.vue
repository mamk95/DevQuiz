<template>
  <div class="QuizView min-h-screen p-4">
    <QuizLoading v-if="quizStore.loading" />

    <div v-else-if="currentQuestion && currentQuestion.prompt" class="max-w-3xl mx-auto pt-8">
      <div class="bg-primary rounded-lg shadow-lg overflow-hidden bg-secondary">
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

const currentQuestion = computed(() => quizStore.currentQuestion)

let penaltyTimeout: ReturnType<typeof setTimeout>
let resultTimeout: ReturnType<typeof setTimeout>

onMounted(() => {
  if (!sessionStore.hasSession) {
    router.push('/')
    return
  }
  loadCurrentQuestion()
})

onUnmounted(() => {
  clearTimeout(penaltyTimeout)
  clearTimeout(resultTimeout)
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
      if (result.quizCompleted) {
        sessionStore.setTotalTime(result.totalMs!)
        await router.push('/finish')
        return
      }
      await loadCurrentQuestion()
    } else {
      showPenalty()
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
      if (result.quizCompleted) {
        sessionStore.setTotalTime(result.totalMs!)
        await router.push('/finish')
        return
      }
      codeAnswer.value = ''
      testResult.value = null
      await loadCurrentQuestion()
    } else {
      showPenalty()
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

const showPenalty = () => {
  penaltyMessage.value = 'Incorrect! +1 second penalty'
  clearTimeout(penaltyTimeout)
  penaltyTimeout = setTimeout(() => {
    penaltyMessage.value = ''
  }, 3000)
}

const loadCurrentQuestion = async () => {
  try {
    const question = await quizStore.getCurrentQuestion()
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
