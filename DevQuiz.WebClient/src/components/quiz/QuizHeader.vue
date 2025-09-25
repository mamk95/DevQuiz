<template>
  <div class="bg-gradient-to-r from-blue-600 to-blue-700 text-white">
    <!-- Duck progress row -->
    <div class="flex justify-center items-center gap-4 py-4 border-b border-white/20">
      <svg 
        v-for="duck in totalQuestions" 
        :key="duck"
        xmlns="http://www.w3.org/2000/svg" 
        viewBox="0 0 24 24"
        class="w-8 h-8 transition-all duration-300 scale-x-[-1]"
        :class="{
          'opacity-100 scale-110 fill-yellow-400': duck < currentIndex + 1,
          'opacity-100 scale-110 fill-none stroke-yellow-400 stroke-2': duck === currentIndex + 1,
          'opacity-70 fill-white': duck > currentIndex + 1
        }"
      >
        <path d="M8.5,5A1.5,1.5 0 0,0 7,6.5A1.5,1.5 0 0,0 8.5,8A1.5,1.5 0 0,0 10,6.5A1.5,1.5 0 0,0 8.5,5M10,2A5,5 0 0,1 15,7C15,8.7 14.15,10.2 12.86,11.1C14.44,11.25 16.22,11.61 18,12.5C21,14 22,12 22,12C22,12 21,21 15,21H9C9,21 4,21 4,16C4,13 7,12 6,10C2,10 2,6.5 2,6.5C3,7 4.24,7 5,6.65C5.19,4.05 7.36,2 10,2Z" />
      </svg>
    </div>


    <!-- Header info -->
    <div class="p-4">
      <div class="flex justify-between items-center">
        <span class="text-sm opacity-90">
          Question {{ currentIndex + 1 }} of {{ totalQuestions }}
        </span>
        <span class="text-xs bg-white/20 px-3 py-1 rounded-full">
          {{ questionType === 'CodeFix' ? 'Code Fix' : 'Multiple Choice' }}
        </span>
      </div>
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