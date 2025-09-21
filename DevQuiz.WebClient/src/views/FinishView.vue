<template>
  <div class="FinishView min-h-screen bg-gray-50 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-8 text-center">
      <div class="text-6xl mb-6">🎉</div>

      <h1 class="text-3xl font-bold mb-4 text-gray-800">Quiz Completed!</h1>

      <div class="bg-blue-50 rounded-lg p-6 mb-6">
        <p class="text-sm text-gray-600 mb-2">Your Total Time</p>
        <p class="text-4xl font-bold text-blue-600">{{ formattedTime }}</p>
      </div>

      <div class="text-gray-700 space-y-2 mb-8">
        <p class="text-lg">Thank you for participating!</p>
        <p class="text-sm text-gray-600">Winners will be contacted by phone</p>
      </div>

      <button
        @click="goHome"
        class="px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
      >
        Back to Home
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/session'

const router = useRouter()
const sessionStore = useSessionStore()

const formattedTime = computed(() => {
  const totalMs = sessionStore.totalTimeMs
  if (!totalMs) return '0.000s'
  const seconds = totalMs / 1000
  return `${seconds.toFixed(3)}s`
})

const goHome = () => {
  sessionStore.clearSession()
  router.push('/')
}
</script>

<style scoped lang="scss"></style>