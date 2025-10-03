<template>
  <div class="bg-primary min-h-screen">
    <AppHeader />
    <RouterView />
  </div>
</template>

<script setup lang="ts">
import AppHeader from '@/components/AppHeader.vue'
import { RouterView } from 'vue-router'
</script>
