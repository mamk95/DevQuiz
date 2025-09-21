<template>
  <div class="JoinView min-h-screen bg-gray-50 flex items-center justify-center p-4">
    <div class="max-w-md w-full bg-white rounded-lg shadow-lg p-8">
      <h1 class="text-3xl font-bold text-center mb-8 text-gray-800">DevQuiz</h1>

      <form @submit.prevent="handleJoin" class="space-y-6">
        <div>
          <label for="name" class="block text-sm font-medium text-gray-700 mb-2">
            Name
          </label>
          <input
            id="name"
            v-model="name"
            type="text"
            required
            maxlength="64"
            class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
            placeholder="Enter your name"
          />
        </div>

        <div>
          <label for="phone" class="block text-sm font-medium text-gray-700 mb-2">
            Phone Number
          </label>
          <div class="flex">
            <span class="inline-flex items-center px-4 py-2 rounded-l-lg border border-r-0 border-gray-300 bg-gray-100 text-gray-700">
              +47
            </span>
            <input
              id="phone"
              v-model="phoneDigits"
              type="tel"
              required
              pattern="[0-9]{8}"
              maxlength="8"
              class="flex-1 px-4 py-2 border border-gray-300 rounded-r-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none"
              placeholder="00000000"
              @input="handlePhoneInput"
            />
          </div>
        </div>

        <div class="text-sm text-gray-600 p-3 bg-gray-50 rounded-lg">
          <p class="mb-2">📝 One attempt per phone number</p>
          <p class="mb-2">📱 Winners will be contacted by phone</p>
          <p>🔒 Data will be deleted after the event</p>
        </div>

        <button
          type="submit"
          :disabled="loading"
          class="w-full py-3 px-4 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 focus:ring-4 focus:ring-blue-500 focus:ring-opacity-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {{ loading ? 'Starting...' : 'Start Quiz' }}
        </button>

        <div v-if="error" class="p-3 bg-red-50 border border-red-200 rounded-lg text-red-700 text-sm">
          {{ error }}
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/session'

const router = useRouter()
const sessionStore = useSessionStore()

const name = ref('')
const phoneDigits = ref('')
const loading = ref(false)
const error = ref('')

const handlePhoneInput = (event: Event) => {
  const input = event.target as HTMLInputElement
  input.value = input.value.replace(/[^0-9]/g, '')
}

const handleJoin = async () => {
  error.value = ''

  if (phoneDigits.value.length !== 8) {
    error.value = 'Phone number must be exactly 8 digits'
    return
  }

  loading.value = true

  try {
    await sessionStore.startSession(name.value, `+47${phoneDigits.value}`)
    if (sessionStore.hasSession) {
      router.push('/quiz')
    }
  } catch (err) {
    error.value = err instanceof Error ? err.message : 'Failed to start quiz'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped lang="scss"></style>