<template>
  <div class="KioskView bg-primary text-white p-8">
    <div class="flex gap-8">
      <LeaderboardDisplay
        title="Noob Quiz"
        quiz-name="noob"
        :leaderboard="noobLeaderboard"
        :format-time="formatTime"
      />

      <LeaderboardDisplay
        title="Nerd Quiz"
        quiz-name="nerd"
        :leaderboard="nerdLeaderboard"
        :format-time="formatTime"
      />

      <!-- QR Code -->
      <div class="w-96">
        <div class="bg-secondary rounded-2xl p-8 flex flex-col items-center justify-center">
          <h2 class="text-3xl font-bold mb-6">Join the Quiz!</h2>

          <div class="bg-white p-6 rounded-lg mb-6">
            <canvas ref="qrCanvas" class="max-w-full"></canvas>
          </div>

          <div class="text-center">
            <p class="text-xl font-mono mb-2">{{ quizUrl }}</p>
            <p class="text-white/70 text-lg">Scan or visit to start</p>
          </div>
          
        </div>
        <div v-if="mostRecentParticipantsNoob.entries.length > 0 || mostRecentParticipantsNerd.entries.length > 0" class="mt-16">
            <div v-if="mostRecentParticipantsNoob.entries.length > 0">
              <p class="text-white text-2xl">New Noobs</p>
              <hr class="border-white/30 mt-4 mb-4" />
              <MostRecentParticipants 
                :participants="mostRecentParticipantsNoob.entries" 
                difficulty="noob"
                @celebrate="handleCelebration"
              />
            </div>
            <div v-if="mostRecentParticipantsNerd.entries.length > 0">
              <p class="text-white text-2xl mt-16">New Nerds</p>
              <hr class="border-white/30 mt-4 mb-4" />
              <MostRecentParticipants 
                :participants="mostRecentParticipantsNerd.entries" 
                difficulty="nerd"
                @celebrate="handleCelebration"
              />
            </div>
        </div>
        
      </div>

      
    </div>
  </div>

  <!-- Shared Celebration overlay for both difficulties -->
  <Teleport to="body">
    <Transition name="fade">
      <div
        v-if="activeCelebrations.length > 0"
        class="fixed inset-0 z-50 bg-black/60 backdrop-blur-sm flex items-center justify-center p-8"
        @click="dismissAllCelebrations"
      >
        <div class="max-w-4xl w-full max-h-screen overflow-y-auto" @click.stop>
          <CompletionCelebration
            v-for="celebration in activeCelebrations"
            :key="celebration.id"
            :participant-name="celebration.name"
            :position="celebration.position"
            :difficulty="celebration.difficulty"
            :show="true"
            @dismiss="dismissCelebration(celebration.id)"
          />
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted } from 'vue'
import { useLeaderboardStore } from '@/stores/leaderboard'
import type { QuizDifficulty } from '@/types/quiz'
import QRCode from 'qrcode'
import LeaderboardDisplay from '@/components/quiz/LeaderboardDisplay.vue'
import MostRecentParticipants from '@/components/quiz/MostRecentParticipants.vue'
import CompletionCelebration from '@/components/quiz/CompletionCelebration.vue'

const leaderboardStore = useLeaderboardStore()
const qrCanvas = ref<HTMLCanvasElement>()

const quizUrl = window.location.origin

const noobLeaderboard = leaderboardStore.noobLeaderboard
const nerdLeaderboard = leaderboardStore.nerdLeaderboard

const mostRecentParticipantsNoob = leaderboardStore.mostRecentParticipantsNoob
const mostRecentParticipantsNerd = leaderboardStore.mostRecentParticipantsNerd

interface ActiveCelebration {
  id: string
  name: string
  position: number
  difficulty: QuizDifficulty
}

const activeCelebrations = ref<ActiveCelebration[]>([])

let pollInterval: ReturnType<typeof setInterval>

onMounted(() => {
  generateQRCode()
  loadLeaderboards()
  loadMostRecentParticipants()
  pollInterval = setInterval(() => {
    loadLeaderboards()
    loadMostRecentParticipants()
  }, 10000)
})

onUnmounted(() => {
  clearInterval(pollInterval)
})

const formatTime = (ms: number) => {
  const seconds = ms / 1000
  return `${seconds.toFixed(1)}s`
}

const loadLeaderboards = async () => {
  try {
    await leaderboardStore.fetchBothLeaderboards(15)
  } catch {
    // Silently ignore - store handles error state
  }
}

const loadMostRecentParticipants = async () => {
  try {
    await leaderboardStore.fetchBothMostRecentParticipants(5)
  } catch {
  }
}

const generateQRCode = async () => {
  if (qrCanvas.value) {
    try {
      await QRCode.toCanvas(qrCanvas.value, quizUrl, {
        width: 300,
        margin: 2,
        color: {
          dark: '#1e3a8a',
          light: '#ffffff',
        },
      })
    } catch {
      // Silently ignore QR generation failure
    }
  }
}

const handleCelebration = (name: string, position: number, difficulty: QuizDifficulty, index: number) => {
  const id = `${name}-${position}-${difficulty}-${Date.now()}`
  activeCelebrations.value.push({ id, name, position, difficulty })

  // Remove the entry for cleaner display, the polling would remove this anyways, but there would be a delay
  const entries = difficulty === 'noob' ? mostRecentParticipantsNoob : mostRecentParticipantsNerd
  entries.entries.splice(index, 1)

}

const dismissCelebration = (id: string) => {
  activeCelebrations.value = activeCelebrations.value.filter(c => c.id !== id)
}

const dismissAllCelebrations = () => {
  activeCelebrations.value = []
}
</script>

<style scoped lang="scss">
.KioskView {
  // Desktop-only optimizations
  min-width: 1400px; // Minimum width for desktop displays

  // Prevent text selection on kiosk
  user-select: none;
  -webkit-user-select: none;
  -moz-user-select: none;
  -ms-user-select: none;
}

.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>