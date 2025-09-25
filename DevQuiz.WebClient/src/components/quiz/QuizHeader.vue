<template>
  <div class="bg-gradient-to-r from-blue-600 to-blue-700 text-white p-4">
    <div class="flex justify-between items-center">
      <span class="text-sm opacity-90">
        Question {{ currentIndex + 1 }} of {{ totalQuestions }}
      </span>
      <span class="text-xs bg-white/20 px-3 py-1 rounded-full">
        {{ questionType === 'CodeFix' ? 'Code Fix' : 'Multiple Choice' }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  currentIndex: number
  totalQuestions: number
  questionType?: 'MultipleChoice' | 'CodeFix'
}>()
</script>

<style scoped lang="scss"></style>