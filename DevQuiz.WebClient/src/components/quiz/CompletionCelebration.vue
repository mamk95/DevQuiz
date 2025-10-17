<template>
  <Transition name="celebration">
    <div
      v-if="showCelebration"
      class="celebration-card bg-gradient-to-br from-primary to-secondary p-8 rounded-2xl shadow-2xl text-white text-center mb-4 cursor-pointer hover:scale-105 transition-transform"
      @click="dismissCelebration"
    >
      <div class="flex items-center gap-6">
        <div class="text-6xl">🎉</div>
        <div class="flex-1 text-left">
          <h2 class="text-3xl font-bold mb-1">{{ participantName }}</h2>
          <p class="text-lg opacity-90">Just completed the <span class="capitalize">{{ difficulty }}</span> quiz!</p>
        </div>
        <div class="p-4 bg-white/10 rounded-xl backdrop-blur-md min-w-[120px]">
          <p class="text-sm opacity-80 mb-1">Position</p>
          <div class="text-5xl font-black text-yellow-300 animate-pulse-scale">
            #{{ position }}
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'
import type { QuizDifficulty } from '@/types/quiz'

interface Props {
  participantName: string
  position: number
  difficulty: QuizDifficulty
  show: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  dismiss: []
}>()

const showCelebration = ref(false)

watch(() => props.show, (newShow) => {
  if (newShow) {
    showCelebration.value = true
    
    setTimeout(() => {
      dismissCelebration()
    }, 5000)
  }
}, { immediate: true })

const dismissCelebration = () => {
  showCelebration.value = false
  setTimeout(() => {
    emit('dismiss')
  }, 300)
}
</script>

<style scoped>
.celebration-enter-active,
.celebration-leave-active {
  transition: opacity 0.3s ease;
}

.celebration-enter-from,
.celebration-leave-to {
  opacity: 0;
}

.celebration-enter-active .celebration-card {
  animation: bounce-in 0.6s cubic-bezier(0.68, -0.55, 0.265, 1.55);
}

.celebration-leave-active .celebration-card {
  animation: bounce-out 0.3s ease-in;
}

@keyframes bounce-in {
  0% {
    transform: scale(0.3);
    opacity: 0;
  }
  50% {
    transform: scale(1.05);
  }
  70% {
    transform: scale(0.9);
  }
  100% {
    transform: scale(1);
    opacity: 1;
  }
}

@keyframes bounce-out {
  0% {
    transform: scale(1);
    opacity: 1;
  }
  100% {
    transform: scale(0.8);
    opacity: 0;
  }
}

@keyframes pulse-scale {
  0%, 100% {
    transform: scale(1);
  }
  50% {
    transform: scale(1.1);
  }
}

.animate-pulse-scale {
  animation: pulse-scale 2s ease-in-out infinite;
}
</style>
