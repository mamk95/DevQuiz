<template>
  <div class="DifficultySelector w-full flex justify-between">
    <button
      type="button"
      :class="['difficulty-btn', selected === 'noob' ? 'selected' : '']"
      @click="select('noob')"
      aria-label="Select Noob difficulty"
    >
      🐣 Noob
    </button>
    <button
      type="button"
      :class="['difficulty-btn', selected === 'nerd' ? 'selected' : '']"
      @click="select('nerd')"
      aria-label="Select Nerd difficulty"
    >
      🤓 Nerd
    </button>
  </div>
</template>

<script setup lang="ts">

const selected = defineModel<string>()

function select(difficulty: string) {
  selected.value = difficulty
}

// Default to 'noob' if not set
if (!selected.value) selected.value = 'noob'
</script>

<style scoped lang="scss">
.DifficultySelector {
  .difficulty-btn {
  width: 47%;
  min-width: 10rem;
    padding: 0.75rem 0;
    border-radius: 9999px;
    border: 2px solid #d1d5db;
    background: #fff;
    color: #374151;
    font-size: 1.1rem;
    font-weight: 500;
    cursor: pointer;
    transition: border-color 0.2s, background 0.2s;
    outline: none;
    &.selected {
      border-color: #2563eb;
      background: #eff6ff;
      color: #2563eb;
      box-shadow: 0 0 0 2px #93c5fd;
    }
    &:hover {
      border-color: #2563eb;
      background: #f3f4f6;
    }
  }
}
</style>
