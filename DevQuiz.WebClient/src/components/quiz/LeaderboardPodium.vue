<template>
  <div class="flex justify-center items-end gap-8 h-64 mt-8">
    <!-- 1 entry -->
    <div v-if="entries.length === 1" class="flex flex-col items-center">
      <span class="mb-4 text-sm text-center">{{ entries[0].name || 'Unknown' }}</span>

      <div
        class="w-40 flex justify-center items-center text-white font-bold h-60 flex-col p-12 shadow-lg relative border-4 podium-bg"
        style="border-color: #fdd209; border-radius: 16px"
      >
        <span class="position-text">1</span>
        <span class="text-xs text-white -bottom-1 mt-4">
          {{ formatTime(entries[0].totalMs) }}
        </span>
      </div>
    </div>

    <!-- 2 entries -->
    <template v-else-if="entries.length === 2">
      <!-- 2nd Place -->
      <div class="flex flex-col items-center">
        <span class="mb-4 text-sm text-center">{{ entries[1].name || 'Unknown' }}</span>

        <div
          class="w-40 flex justify-center items-center text-white font-bold h-48 flex-col p-12 shadow-lg relative border-4 podium-bg"
          style="border-color: #d3d3d3; border-radius: 16px"
        >
          <span class="position-text">2</span>
          <span class="text-xs text-white -bottom-1 mt-4">
            {{ formatTime(entries[1].totalMs) }}
          </span>
        </div>
      </div>

      <!-- 1st Place -->
      <div class="flex flex-col items-center">
        <span class="mb-4 text-sm text-center">{{ entries[0].name || 'Unknown' }}</span>
        <div
          class="w-40 flex justify-center items-center text-white font-bold h-60 flex-col p-12 shadow-lg relative border-4 podium-bg"
          style="border-color: #fdd209; border-radius: 16px"
        >
          <span class="position-text">1</span>
          <span class="text-xs text-white -bottom-1 mt-4">
            {{ formatTime(entries[0].totalMs) }}
          </span>
        </div>
      </div>
    </template>

    <!-- 3 entries -->
    <template v-else-if="entries.length >= 3">
      <!-- 2nd Place -->
      <div class="flex flex-col items-center">
        <span class="mb-4 text-sm text-center">
          {{ entries[1].name || 'Unknown' }}
        </span>
        <div
          class="w-40 flex justify-center items-center text-white font-bold h-48 flex-col p-12 shadow-lg border-4 podium-bg"
          style="border-color: #d3d3d3; border-radius: 16px"
        >
          <span class="position-text">2</span>
          <span class="text-xs text-white -bottom-1 mt-4">
            {{ formatTime(entries[1].totalMs) }}
          </span>
        </div>
      </div>

      <!-- 1st Place -->
      <div class="flex flex-col items-center">
        <span class="mb-4 text-sm text-center">
          {{ entries[0].name || 'Unknown' }}
        </span>
        <div
          class="w-40 flex justify-center items-center text-white font-bold h-60 flex-col p-12 shadow-lg border-4 podium-bg"
          style="border-color: #fdd209; border-radius: 16px"
        >
          <span class="position-text">1</span>
          <span class="text-xs text-white -bottom-1 mt-4">
            {{ formatTime(entries[0].totalMs) }}
          </span>
        </div>
      </div>

      <!-- 3rd Place -->
      <div class="flex flex-col items-center">
        <span class="mb-4 text-sm text-center">
          {{ entries[2].name || 'Unknown' }}
        </span>
        <div
          class="w-40 flex justify-center items-center text-white font-bold h-40 flex-col p-12 shadow-lg border-4 podium-bg"
          style="border-color: #99550b; border-radius: 16px"
        >
          <span class="position-text">3</span>
          <span class="text-xs text-white -bottom-8 mt-4">
            {{ formatTime(entries[2].totalMs) }}
          </span>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { LeaderboardEntry } from '@/lib/api'
defineProps<{
  entries: LeaderboardEntry[]
  formatTime: (ms: number) => string
}>()
</script>

<style scoped>
.podium-bg {
  background-color: #3a3447;
}

.position-text {
  font-style: normal;
  font-weight: 400;
  font-size: 32px;
  line-height: 40px;
  text-align: center;
  color: #ffffff;
  width: 19px;
  height: 40px;
}
</style>
