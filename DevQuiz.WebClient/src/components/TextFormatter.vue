<template>
  <div class="formatted-text">
    <div v-for="(block, index) in parsedBlocks" :key="index">
      <CodeBlock 
        v-if="block.type === 'code'"
        :code="block.content"
        :language="block.language"
        class="mb-4"
      />
      <p v-else-if="block.type === 'text'" class="mb-4">
        <template v-for="(segment, idx) in parseInlineCode(block.content)" :key="idx">
          <code v-if="segment.isCode" class="inline-code">{{ segment.text }}</code>
          <span v-else>{{ segment.text }}</span>
        </template>
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import CodeBlock from './CodeBlock.vue'

const props = defineProps<{
  text: string
}>()

interface TextBlock {
  type: 'text' | 'code'
  content: string
  language?: string
}

const parsedBlocks = computed(() => {
  const blocks: TextBlock[] = []
  const lines = props.text.split('\n')
  let i = 0
  
  while (i < lines.length) {
    const line = lines[i]
    
    if (line.startsWith('```')) {
      const language = line.slice(3).trim()
      i++
      let codeContent = ''
      
      while (i < lines.length && !lines[i].startsWith('```')) {
        codeContent += lines[i] + '\n'
        i++
      }
      
      blocks.push({
        type: 'code',
        content: codeContent.trimEnd(),
        language: language || undefined
      })
      i++
    } else {
      let textContent = line
      i++
      
      while (i < lines.length && !lines[i].startsWith('```')) {
        textContent += '\n' + lines[i]
        i++
      }
      
      if (textContent.trim()) {
        blocks.push({
          type: 'text',
          content: textContent.trim()
        })
      }
    }
  }
  
  return blocks
})

const parseInlineCode = (text: string) => {
  const segments = []
  const regex = /`([^`]+)`/g
  let lastIndex = 0
  let match

  while ((match = regex.exec(text)) !== null) {
    if (match.index > lastIndex) {
      segments.push({ text: text.slice(lastIndex, match.index), isCode: false })
    }
    segments.push({ text: match[1], isCode: true })
    lastIndex = regex.lastIndex
  }

  if (lastIndex < text.length) {
    segments.push({ text: text.slice(lastIndex), isCode: false })
  }

  return segments
}
</script>

<style scoped>
.formatted-text :deep(.inline-code) {
  background-color: #f3f4f6;
  padding: 0.25rem 0.5rem;
  border-radius: 0.25rem;
  font-size: 0.875rem;
  font-family: ui-monospace, SFMono-Regular, 'SF Mono', Consolas, 'Liberation Mono', Menlo, monospace;
  color: #2563eb;
}
</style>