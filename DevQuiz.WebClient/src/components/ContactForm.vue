<template>
  <div class="ContactForm bg-blue-50 rounded-lg p-6 mt-6">
    <h3 class="text-lg font-semibold text-gray-800 mb-4">Stay Connected</h3>
    
    <div class="space-y-4">
      <div class="flex items-start space-x-3">
        <input
          id="consent-checkbox"
          v-model="consentGiven"
          type="checkbox"
          class="mt-1 h-4 w-4 text-blue-600 focus:ring-blue-500 border-gray-300 rounded"
        />
        <label for="consent-checkbox" class="text-sm text-gray-700 leading-5">
          I would like Capgemini to contact me about new job opportunities
        </label>
      </div>

      <div v-if="consentGiven" class="space-y-4 pt-2">
        <div>
          <label for="email" class="block text-sm font-medium text-gray-700 mb-2">
            Email Address
          </label>
          <input
            id="email"
            v-model="email"
            type="email"
            placeholder="Enter your email address"
            class="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm placeholder-gray-400 focus:outline-none focus:ring-blue-500 focus:border-blue-500"
            :class="{ 'border-red-300': emailError }"
          />
          <p v-if="emailError" class="mt-1 text-sm text-red-600">
            {{ emailError }}
          </p>
        </div>

        <button
          @click="submitContact"
          :disabled="isSubmitting || !isValidEmail || submitSuccess"
          class="w-full px-4 py-2 bg-blue-600 text-white font-medium rounded-md hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-blue-500 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
        >
          {{ submitSuccess ? 'Submitted!' : isSubmitting ? 'Submitting...' : 'Submit' }}
        </button>

        <p v-if="submitSuccess" class="text-sm text-green-600 text-center">
          Thank you! We'll contact you about new opportunities.
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { api } from '@/lib/api'
import { ref, computed } from 'vue'

const consentGiven = ref(false)
const email = ref('')
const emailError = ref('')
const isSubmitting = ref(false)
const submitSuccess = ref(false)

const isValidEmail = computed(() => {
  const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
  return email.value && emailRegex.test(email.value)
})

const validateEmail = () => {
  if (!email.value) {
    emailError.value = 'Email is required'
    return false
  }
  
  if (!isValidEmail.value) {
    emailError.value = 'Please enter a valid email address'
    return false
  }
  
  emailError.value = ''
  return true
}

const submitContact = async () => {
  if (!validateEmail()) return
  
  isSubmitting.value = true
  
  try {
    // Normalize email to lowercase
    const normalizedEmail = email.value.trim().toLowerCase()
    const response = await api.submitEmail(normalizedEmail)
    if (response.success) {
        submitSuccess.value = true
        // Prevent further submissions
        email.value = normalizedEmail
    } else {
        throw new Error(response.message || 'Submission failed')
    }
    
  } catch (error) {
    
    if(error instanceof Error && error.message) {
        emailError.value = error.message
    } else {
        emailError.value = 'Failed to submit. Please try again.'
    }
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped lang="scss"></style>

