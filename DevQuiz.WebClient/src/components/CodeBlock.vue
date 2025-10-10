<template>
  <div class="border-2 border-gray-200 rounded-lg overflow-hidden">
    <div class="bg-gray-800 text-gray-200 px-4 py-2 text-sm font-mono flex items-center">
      <svg class="w-4 h-4 mr-2 text-green-400" fill="currentColor" viewBox="0 0 20 20">
        <path
          fill-rule="evenodd"
          d="M12.316 3.051a1 1 0 01.633 1.265l-4 12a1 1 0 11-1.898-.632l4-12a1 1 0 011.265-.633zM5.707 6.293a1 1 0 010 1.414L3.414 10l2.293 2.293a1 1 0 11-1.414 1.414l-3-3a1 1 0 010-1.414l3-3a1 1 0 011.414 0zm8.586 0a1 1 0 011.414 0l3 3a1 1 0 010 1.414l-3 3a1 1 0 01-1.414-1.414L16.586 10l-2.293-2.293a1 1 0 010-1.414z"
          clip-rule="evenodd"
        />
      </svg>
      {{ language || 'Code' }}
    </div>
    <div class="px-4 py-3 bg-gray-50 text-gray-800 font-mono text-sm">
      <pre class="whitespace-pre-wrap"><code>{{ code }}</code></pre>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  code: string
  language?: string
}>()
</script>