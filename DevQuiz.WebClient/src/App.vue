<template>
  <div class="bg-primary h-dvh flex flex-col gap-4">
    <AppHeader />
    <RouterView />
  </div>
</template>

<script setup lang="ts">
import AppHeader from '@/components/AppHeader.vue'
import { RouterView } from 'vue-router'
</script>

<style scoped lang="scss"></style>
