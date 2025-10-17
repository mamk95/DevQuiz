import { defineStore } from 'pinia'
import { reactive } from 'vue'
import { api, type LeaderboardEntry, type MostRecentParticipant } from '@/lib/api'
import type { QuizDifficulty } from '@/types/quiz'

interface LeaderboardData {
  entries: LeaderboardEntry[]
  loading: boolean
  error: string | null
}


interface MostRecentParticipantsData {
  entries: MostRecentParticipant[]
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
    // Only show loading state if we don't have data yet
    if (leaderboardData.entries.length === 0) {
      leaderboardData.loading = true
    }
    leaderboardData.error = null

    try {
      const data = await api.getLeaderboard(limit, difficulty)
      leaderboardData.entries = data
      return data
    } catch (err) {
      leaderboardData.error = err instanceof Error ? err.message : 'Failed to load leaderboard'
      // Keep existing entries on error to avoid flash of empty content
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

  const mostRecentParticipantsNerd = reactive<MostRecentParticipantsData>({
    entries: [],
    loading: false,
    error: null
  })

  const mostRecentParticipantsNoob = reactive<MostRecentParticipantsData>({
    entries: [],
    loading: false,
    error: null
  })

  async function fetchMostRecentParticipants(difficulty: QuizDifficulty, limit: number = 10): Promise<MostRecentParticipant[]> {
    const mostRecentParticipants = difficulty === 'noob' ? mostRecentParticipantsNoob : mostRecentParticipantsNerd
    mostRecentParticipants.loading = true
    mostRecentParticipants.error = null

    try {
      const data = await api.getMostRecentParticipantsByDifficulty(limit, difficulty)
      mostRecentParticipants.entries = data
      return data
    } catch (err) {
      mostRecentParticipants.error = err instanceof Error ? err.message : 'Failed to load most recent participants'
      throw err
    } finally {
      mostRecentParticipants.loading = false
    }
  }

  async function fetchBothMostRecentParticipants(limit: number = 10): Promise<void> {
    await Promise.all([
      fetchMostRecentParticipants('noob', limit),
      fetchMostRecentParticipants('nerd', limit)
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
    clearLeaderboards,

    // Most recent participants
    mostRecentParticipantsNerd,
    mostRecentParticipantsNoob,
    fetchBothMostRecentParticipants,
    fetchMostRecentParticipants
  }
})