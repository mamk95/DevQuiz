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
          <div class="flex flex-col gap-2 sm:flex-row">
            <div class="flex gap-2 w-full sm:w-auto">
              <select
                v-if="!isCustomCode"
                :value="countryCode"
                @change="handleCountryChange(($event.target as HTMLSelectElement).value)"
                class="w-full px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none bg-white sm:w-auto"
              >
                <option v-for="country in commonCountryCodes" :key="country.code" :value="country.code">
                  {{ country.code }} {{ country.country }}
                </option>
                <option value="custom">Other...</option>
              </select>
              <div v-else class="relative w-full sm:w-auto">
                <input
                  v-model="customCountryCode"
                  @input="handleCustomCodeInput"
                  type="text"
                  placeholder="+XXX"
                  maxlength="6"
                  class="w-full pr-8 pl-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none sm:w-32"
                />
                <button
                  type="button"
                  @click="handleCountryChange('+47')"
                  class="absolute right-1 top-1/2 -translate-y-1/2 p-1 text-gray-400 hover:text-gray-600 transition-colors cursor-pointer"
                  title="Back to country list"
                >
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12"></path>
                  </svg>
                </button>
              </div>
            </div>
            <input
              id="phone"
              v-model="phoneDigits"
              type="tel"
              required
              :pattern="phonePattern"
              :maxlength="phoneValidation.maxLength"
              class="w-full px-4 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent outline-none sm:flex-1"
              :placeholder="phonePlaceholder"
              @input="handlePhoneInput"
            />
          </div>
        </div>

        <div class="w-full flex flex-col mb-4">
          <label class="block text-sm font-medium text-gray-700 mb-2 text-left w-full">Difficulty</label>
          <div class="flex justify-center w-full">
            <DifficultySelector v-model="difficulty" />
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
import { ref, computed } from 'vue'
import DifficultySelector from '@/components/quiz/DifficultySelector.vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/stores/session'

const router = useRouter()
const sessionStore = useSessionStore()

// Common country codes with known validation rules
const commonCountryCodes = [
  { code: '+47', country: 'Norway', minLength: 8, maxLength: 8 },
  { code: '+46', country: 'Sweden', minLength: 7, maxLength: 13 },
  { code: '+45', country: 'Denmark', minLength: 8, maxLength: 8 },
  { code: '+358', country: 'Finland', minLength: 7, maxLength: 12 },
  { code: '+44', country: 'UK', minLength: 10, maxLength: 10 },
  { code: '+49', country: 'Germany', minLength: 10, maxLength: 12 },
  { code: '+33', country: 'France', minLength: 9, maxLength: 9 },
  { code: '+39', country: 'Italy', minLength: 9, maxLength: 10 },
  { code: '+34', country: 'Spain', minLength: 9, maxLength: 9 },
  { code: '+31', country: 'Netherlands', minLength: 9, maxLength: 9 },
  { code: '+48', country: 'Poland', minLength: 9, maxLength: 9 },
  { code: '+380', country: 'Ukraine', minLength: 9, maxLength: 9 },
]

const name = ref('')
const phoneDigits = ref('')
const countryCode = ref('+47')
const isCustomCode = ref(false)
const customCountryCode = ref('')
const loading = ref(false)
const error = ref('')
const difficulty = ref('noob')

// Find if current code is in common list
const knownCountry = computed(() =>
  commonCountryCodes.find(c => c.code === countryCode.value)
)

// For custom codes, use generic validation (4-15 digits is standard international range)
const phoneValidation = computed(() => {
  if (knownCountry.value) {
    return {
      minLength: knownCountry.value.minLength,
      maxLength: knownCountry.value.maxLength
    }
  }
  // Generic validation for unknown country codes
  return {
    minLength: 4,
    maxLength: 15
  }
})

const phonePattern = computed(() => {
  const validation = phoneValidation.value
  return `[0-9]{${validation.minLength},${validation.maxLength}}`
})

const phonePlaceholder = computed(() => {
  if (knownCountry.value) {
    return '0'.repeat(knownCountry.value.minLength)
  }
  return 'Phone number'
})

const handleCountryChange = (value: string) => {
  if (value === 'custom') {
    isCustomCode.value = true
    customCountryCode.value = '+'
    countryCode.value = '+'
  } else {
    isCustomCode.value = false
    countryCode.value = value
  }
}

const handleCustomCodeInput = (event: Event) => {
  const input = event.target as HTMLInputElement
  // Ensure it starts with + and only contains numbers after that
  let value = input.value
  if (!value.startsWith('+')) {
    value = '+' + value.replace(/[^0-9]/g, '')
  } else {
    value = '+' + value.slice(1).replace(/[^0-9]/g, '')
  }
  customCountryCode.value = value
  countryCode.value = value
}

const handlePhoneInput = (event: Event) => {
  const input = event.target as HTMLInputElement
  input.value = input.value.replace(/[^0-9]/g, '')
}

const handleJoin = async () => {
  error.value = ''

  // Validate country code - must be at least "+" and one digit
  if (!countryCode.value.match(/^\+\d+$/)) {
    error.value = 'Please enter a valid country code'
    return
  }

  const validation = phoneValidation.value
  const phoneLength = phoneDigits.value.length

  if (phoneLength < validation.minLength || phoneLength > validation.maxLength) {
    if (knownCountry.value && validation.minLength === validation.maxLength) {
      error.value = `Phone number must be exactly ${validation.minLength} digits`
    } else {
      error.value = `Phone number must be between ${validation.minLength} and ${validation.maxLength} digits`
    }
    return
  }

  loading.value = true

  try {
    await sessionStore.startSession(
      name.value,
      `${countryCode.value}${phoneDigits.value}`,
      difficulty.value
    )
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