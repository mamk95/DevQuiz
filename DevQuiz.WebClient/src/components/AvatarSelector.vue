<template>
  <div class="AvatarSelector relative inline-block" :style="circleStyle">
    <div class="relative" :style="circleStyle" @click="toggleMenu">
      <img
        :src="selectedAvatar"
        alt="Selected avatar"
        class="avatar-img rounded-full border border-gray-300 shadow w-full h-full object-cover cursor-pointer"
      />
      <div
        class="avatar-arrow absolute cursor-pointer"
        style="width: 32px; height: 32px; bottom: -8px; right: -8px"
        @click.stop="toggleMenu"
        aria-label="Choose avatar"
      >
        <svg
          class="w-4 h-4 text-blue-600 arrow-icon"
          :class="menuOpen ? 'rotate-90' : 'rotate-0'"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M19 9l-7 7-7-7"
          />
        </svg>
      </div>
    </div>

    <div v-if="menuOpen" class="avatar-menu" :style="menuStyle">
      <div class="avatar-grid">
        <button
          v-for="avatar in avatars"
          :key="avatar"
          type="button"
          class="avatar-option"
          :class="{ selected: selectedAvatar === avatar }"
          @click="selectAvatar(avatar)"
          aria-label="Select avatar"
        >
          <img :src="avatar" alt="Avatar" class="w-full h-full object-cover" />
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'

// Props: inputHeight and containerWidth for sizing
const props = defineProps<{ inputHeight?: number; containerWidth?: number }>()

// v-model for selected avatar
const selectedAvatar = defineModel<string>()

// Auto-load avatars from src/assets/avatars
const avatarModules = import.meta.glob('@/assets/avatars/*.{png,jpg,jpeg,gif,svg,webp}', {
  eager: true,
})
const avatars = Object.values(avatarModules).map((mod: any) => mod.default ?? mod)

// Menu open state
const menuOpen = ref(false)

// Set a random default avatar on mount
onMounted(() => {
  if (!selectedAvatar.value && avatars.length > 0) {
    const randomIdx = Math.floor(Math.random() * avatars.length)
    selectedAvatar.value = avatars[randomIdx]
  }
})

const circleStyle = computed(() => {
  const size = props.inputHeight ? `${props.inputHeight * 1.5}px` : '72px'
  return {
    width: size,
    height: size,
    minWidth: size,
    minHeight: size,
    maxWidth: size,
    maxHeight: size,
  }
})

// Menu overlay style: center it relative to the avatar button
const menuStyle = computed(() => {
  const containerWidth = props.containerWidth || 300
  return {
    left: '50%',
    transform: 'translateX(-50%)',
    width: `${Math.min(containerWidth, 320)}px`,
    maxWidth: '90vw',
  }
})

function toggleMenu() {
  menuOpen.value = !menuOpen.value
}

// Select an avatar and close menu
function selectAvatar(avatar: string) {
  selectedAvatar.value = avatar
  menuOpen.value = false
}
</script>

<style scoped lang="scss">
.avatar-img {
  transition: box-shadow 0.2s;
}
.avatar-arrow {
  box-shadow: 0 2px 6px rgba(0, 0, 0, 0.08);
  background: #fff;
  border-radius: 50%;
  border: 1px solid #d1d5db;
  display: flex;
  align-items: center;
  justify-content: center;
  transition: 0.2s;
  z-index: 1;
}
.arrow-icon {
  transition: transform 0.3s cubic-bezier(0.4, 0, 0.2, 1);
}
.rotate-0 {
  transform: rotate(0deg);
}
.rotate-90 {
  transform: rotate(90deg);
}
.avatar-menu {
  position: absolute;
  top: 100%;
  left: 0;
  margin-top: 1rem;
  z-index: 50;
  background: #fff;
  border-radius: 0.5rem;
  box-shadow: 0 4px 24px rgba(0, 0, 0, 0.12);
  padding: 1rem;
  border: 1px solid #e5e7eb;
  width: 100%;
  max-width: 100%;
}

.avatar-grid {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 4px;
  width: 100%;
}

.avatar-option {
  width: 4rem;
  height: 4rem;
  border-radius: 9999px;
  border: 1px solid #d1d5db;
  display: flex;
  align-items: center;
  justify-content: center;
  background: none;
  cursor: pointer;
  transition:
    border-color 0.2s,
    box-shadow 0.2s;
  outline: none;
  overflow: hidden;
  &:hover {
    border-color: #2563eb;
  }
  &.selected {
    border-color: #3b82f6;
    box-shadow: 0 0 0 2px #93c5fd;
  }
  img {
    width: 3rem;
    height: 3rem;
    border-radius: 9999px;
    display: block;
    object-fit: cover;
  }
}
</style>
