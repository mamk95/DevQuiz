<template>
  <div class="flex justify-center items-end gap-8 h-64 mt-8">
    <!-- 1 entry -->
    <div v-if="entries.length === 1" class="flex flex-col items-center">
      <div
        class="w-32 bg-yellow-400 flex justify-center items-center text-white font-bold h-32 flex-col"
      >
        <div>1</div>
        <span class="text-xs text-white -bottom-1">
          {{ formatTime(entries[0].totalMs) }}
        </span>
      </div>
      <span class="mt-2 text-sm text-center">
        {{ entries[0].name || 'Unknown' }}
      </span>
    </div>

    <!-- 2 entries -->
    <template v-else-if="entries.length === 2">
      <!-- 2nd Place -->
      <div class="flex flex-col items-center">
        <div
          class="w-32 bg-gray-300 flex justify-center items-center text-white font-bold h-24 flex-col"
        >
          <div>2</div>
          <span class="text-xs text-white -bottom-1">
            {{ formatTime(entries[1].totalMs) }}
          </span>
        </div>
        <span class="mt-2 text-sm text-center">
          {{ entries[1].name || 'Unknown' }}
        </span>
      </div>

      <!-- 1st Place -->
      <div class="flex flex-col items-center">
        <div
          class="w-32 bg-yellow-400 flex justify-center items-center text-white font-bold h-32 flex-col"
        >
          <div>1</div>
          <span class="text-xs text-white -bottom-1">
            {{ formatTime(entries[0].totalMs) }}
          </span>
        </div>
        <span class="mt-2 text-sm text-center">
          {{ entries[0].name || 'Unknown' }}
        </span>
      </div>
    </template>

    <!-- 3 entries -->
    <template v-else-if="entries.length >= 3">
      <!-- 2nd Place -->
      <div class="flex flex-col items-center">
        <div
          class="w-32 bg-gray-300 flex justify-center items-center text-white font-bold h-24 flex-col"
        >
          <div>2</div>
          <span class="text-xs text-white -bottom-1">
            {{ formatTime(entries[1].totalMs) }}
          </span>
        </div>
        <span class="mt-2 text-sm text-center">
          {{ entries[1].name || 'Unknown' }}
        </span>
      </div>

      <!-- 1st Place -->
      <div class="flex flex-col items-center">
        <div
          class="w-32 bg-yellow-400 flex justify-center items-center text-white font-bold h-32 flex-col"
        >
          <div>1</div>
          <span class="text-xs text-white -bottom-1">
            {{ formatTime(entries[0].totalMs) }}
          </span>
        </div>
        <span class="mt-2 text-sm text-center">
          {{ entries[0].name || 'Unknown' }}
        </span>
      </div>

      <!-- 3rd Place -->
      <div class="flex flex-col items-center">
        <div
          class="w-32 bg-amber-700 flex justify-center items-center text-white font-bold h-20 flex-col"
        >
          <div>3</div>
          <span class="text-xs text-white -bottom-1">
            {{ formatTime(entries[2].totalMs) }}
          </span>
        </div>
        <span class="mt-2 text-sm text-center">
          {{ entries[2].name || 'Unknown' }}
        </span>
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

<style scoped lang="scss"></style>
