import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api, type LeaderboardEntry } from '@/lib/api'

export const useLeaderboardStore = defineStore('leaderboard', () => {
  const entries = ref<LeaderboardEntry[]>([])
  const loading = ref(false)
  const error = ref<string | null>(null)

  async function fetchLeaderboard(limit: number = 10): Promise<LeaderboardEntry[]> {
    loading.value = true
    error.value = null

    try {
      const data = await api.getLeaderboard(limit)
      entries.value = data
      return data
    } catch (err) {
      error.value = err instanceof Error ? err.message : 'Failed to load leaderboard'
      entries.value = []
      throw err
    } finally {
      loading.value = false
    }
  }

  function clearLeaderboard() {
    entries.value = []
    loading.value = false
    error.value = null
  }

  return {
    entries,
    loading,
    error,
    fetchLeaderboard,
    clearLeaderboard
  }
})