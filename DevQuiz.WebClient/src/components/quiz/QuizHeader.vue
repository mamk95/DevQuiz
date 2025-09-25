import duckColor from '@/assets/duck_color.png';
import duckContour from '@/assets/duck_contour.png';

<template>
  <div class="bg-gradient-to-r from-blue-600 to-blue-700 text-white">
    <!-- Duck progress row -->
    <div class="flex justify-center items-center gap-4 py-4 border-b border-white/20">
      <img 
        v-for="duck in totalQuestions" 
        :key="duck"
        :src="duck <= currentIndex ? duckColor : duckContour"
        :alt="duck <= currentIndex ? 'Completed Duck' : 'Remaining Duck'"
        class="w-8 h-8 transition-all duration-300"
        :class="duck <= currentIndex 
          ? 'opacity-100 scale-110' 
          : 'opacity-70 filter brightness-0 invert'"
      />
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