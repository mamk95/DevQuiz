<template>
  <div class="space-y-3">
    <button
      v-for="(choice, index) in choices"
      :key="index"
      @click="$emit('submit', choice)"
      :disabled="disabled"
      :class="[
        'w-full p-4 text-left rounded-lg border-2 transition-all duration-200',
        selectedChoice === choice && !showResult
          ? 'border-blue-500 bg-blue-50'
          : 'border-gray-200 hover:border-blue-400 hover:bg-blue-50',
        disabled && 'opacity-50 cursor-not-allowed',
        showResult &&
          lastAnswer === choice &&
          !wasCorrect &&
          'border-red-500 bg-red-50 shake-animation',
      ]"
    >
      <div class="flex items-center">
        <span class="flex-1">{{ choice }}</span>
        <span v-if="showResult && lastAnswer === choice && !wasCorrect" class="text-red-600 ml-2">
          <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
            <path
              fill-rule="evenodd"
              d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z"
              clip-rule="evenodd"
            />
          </svg>
        </span>
      </div>
    </button>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  choices?: string[]
  disabled: boolean
  selectedChoice: string | null
  showResult: boolean
  lastAnswer: string | null
  wasCorrect: boolean
}>()

defineEmits<{
  submit: [answer: string]
}>()
</script>

<style scoped lang="scss">
@keyframes shake {
  0%,
  100% {
    transform: translateX(0);
  }
  10%,
  30%,
  50%,
  70%,
  90% {
    transform: translateX(-2px);
  }
  20%,
  40%,
  60%,
  80% {
    transform: translateX(2px);
  }
}

.shake-animation {
  animation: shake 0.5s ease-in-out;
}
</style>
