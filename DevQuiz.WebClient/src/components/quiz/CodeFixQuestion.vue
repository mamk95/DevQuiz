<template>
  <div class="space-y-4">
    <!-- Code Editor -->
    <div class="border-2 border-gray-200 rounded-lg overflow-hidden">
      <div class="bg-gray-800 text-gray-200 px-4 py-2 text-sm font-mono flex items-center">
        <svg class="w-4 h-4 mr-2 text-green-400" fill="currentColor" viewBox="0 0 20 20">
          <path
            fill-rule="evenodd"
            d="M12.316 3.051a1 1 0 01.633 1.265l-4 12a1 1 0 11-1.898-.632l4-12a1 1 0 011.265-.633zM5.707 6.293a1 1 0 010 1.414L3.414 10l2.293 2.293a1 1 0 11-1.414 1.414l-3-3a1 1 0 010-1.414l3-3a1 1 0 011.414 0zm8.586 0a1 1 0 011.414 0l3 3a1 1 0 010 1.414l-3 3a1 1 0 01-1.414-1.414L16.586 10l-2.293-2.293a1 1 0 010-1.414z"
            clip-rule="evenodd"
          />
        </svg>
        Code Editor
      </div>
      <div class="relative">
        <textarea
          :value="modelValue"
          @input="handleInput"
          required
          rows="6"
          class="w-full px-4 py-3 font-mono text-sm focus:ring-2 focus:ring-blue-500 focus:outline-none resize-none"
          :placeholder="placeholder || 'Enter your code here...'"
          :disabled="disabled"
          spellcheck="false"
        ></textarea>
      </div>
    </div>

    <!-- Test Display -->
    <div
      v-if="testCode"
      class="border-2 rounded-lg overflow-hidden"
      :class="
        testResult === null ? 'border-gray-200' : testResult ? 'border-green-500' : 'border-red-500'
      "
    >
      <div
        class="px-4 py-2 text-sm font-mono flex items-center justify-between"
        :class="
          testResult === null
            ? 'bg-gray-100 text-gray-700'
            : testResult
              ? 'bg-green-50 text-green-700'
              : 'bg-red-50 text-red-700'
        "
      >
        <div class="flex items-center">
          <svg
            v-if="testResult === null"
            class="w-4 h-4 mr-2 text-gray-500"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z"
              clip-rule="evenodd"
            />
          </svg>
          <svg
            v-else-if="testResult"
            class="w-4 h-4 mr-2 text-green-600"
            fill="currentColor"
            viewBox="0 0 20 20"
          >
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
              clip-rule="evenodd"
            />
          </svg>
          <svg v-else class="w-4 h-4 mr-2 text-red-600" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
              clip-rule="evenodd"
            />
          </svg>
          <span>Test</span>
        </div>
        <span class="text-xs">
          {{ testResult === null ? 'Not tested' : testResult ? 'Passing' : 'Failing' }}
        </span>
      </div>
      <div class="px-4 py-3 bg-gray-900 text-gray-300 font-mono text-sm">
        <code>{{ testCode }}</code>
      </div>
    </div>

    <!-- Submit Button -->
    <button
      @click="$emit('submit')"
      :disabled="disabled || !modelValue.trim()"
      class="w-full py-3 px-4 bg-blue-600 text-white rounded-lg font-medium hover:bg-blue-700 focus:ring-4 focus:ring-blue-500 focus:ring-opacity-50 disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
    >
      {{ disabled ? 'Submitting...' : 'Submit Answer' }}
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  modelValue: string
  placeholder?: string
  testCode?: string
  testResult: boolean | null
  disabled: boolean
}>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
  'update:testResult': [value: null]
  submit: []
}>()

const handleInput = (event: Event) => {
  const target = event.target as HTMLTextAreaElement
  emit('update:modelValue', target.value)
  emit('update:testResult', null)
}
</script>
