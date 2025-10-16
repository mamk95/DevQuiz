<template>
  <div class="FinishView flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-secondary rounded-lg shadow-lg p-8 text-center">
      <div class="text-6xl mb-6">🎉</div>

      <h1 class="text-3xl font-bold mb-4">Quiz Completed!</h1>

      <div class="rounded-lg p-6 mb-6 bg-gray-100 text-gray-900">
        <p class="text-sm mb-2">Your Total Time</p>
        <p class="text-4xl font-bold">{{ formattedTime }}</p>
      </div>

      <div class="bg-blue-50 rounded-lg p-6 mb-6">
        <p class="text-sm text-gray-600 mb-2">Your Total Time</p>
        <p class="text-4xl font-bold text-blue-600">{{ formattedTime }}</p>
      </div>

      <div class="space-y-2 mb-8">
        <p class="text-lg">Thank you for participating!</p>
        <p class="text-sm">Winners will be contacted by phone</p>
      </div>

      <ContactForm />

      <button
        @click="goHome"
        class="mt-6 px-6 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300 transition-colors"
      >
        Back to Home
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/session'

const router = useRouter()
import { api, type LeaderboardPersonalScore } from '@/lib/api'
import ContactForm from '@/components/ContactForm.vue'

const leaderBoard = ref<LeaderboardPersonalScore | null>(null)
onMounted(async () => {
  leaderBoard.value = await api.getMyScore()
})

const formattedTime = computed(() => {
  const totalMs = leaderBoard.value?.totalMs
  if (!totalMs) return '0.0s'
  const seconds = totalMs / 1000
  return `${seconds.toFixed(1)}s`
})

const goHome = () => {
  router.push('/')
}
</script>
