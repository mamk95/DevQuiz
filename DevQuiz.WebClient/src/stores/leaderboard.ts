import { defineStore } from 'pinia'
import { reactive } from 'vue'
import { api, type LeaderboardEntry } from '@/lib/api'

interface LeaderboardData {
  entries: LeaderboardEntry[]
  loading: boolean
  error: string | null
}

export const useLeaderboardStore = defineStore('leaderboard', () => {
  const noobLeaderboard = reactive<LeaderboardData>({
    entries: [],
    loading: false,
    error: null
  })

  const nerdLeaderboard = reactive<LeaderboardData>({
    entries: [],
    loading: false,
    error: null
  })

  async function fetchLeaderboardByDifficulty(difficulty: 'Noob' | 'Nerd', limit: number = 10): Promise<LeaderboardEntry[]> {
    const leaderboardData = difficulty === 'Noob' ? noobLeaderboard : nerdLeaderboard
    leaderboardData.loading = true
    leaderboardData.error = null

    try {
      const data = await api.getLeaderboard(limit, difficulty)
      leaderboardData.entries = data
      return data
    } catch (err) {
      leaderboardData.error = err instanceof Error ? err.message : 'Failed to load leaderboard'
      leaderboardData.entries = []
      throw err
    } finally {
      leaderboardData.loading = false
    }
  }

  async function fetchBothLeaderboards(limit: number = 10): Promise<void> {
    await Promise.all([
      fetchLeaderboardByDifficulty('Noob', limit),
      fetchLeaderboardByDifficulty('Nerd', limit)
    ])
  }

  function clearLeaderboards() {
    noobLeaderboard.entries = []
    noobLeaderboard.loading = false
    noobLeaderboard.error = null

    nerdLeaderboard.entries = []
    nerdLeaderboard.loading = false
    nerdLeaderboard.error = null
  }

  return {
    noobLeaderboard,
    nerdLeaderboard,
    fetchLeaderboardByDifficulty,
    fetchBothLeaderboards,
    clearLeaderboards
  }
})