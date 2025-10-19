import * as signalR from '@microsoft/signalr'

const API_URL = import.meta.env.VITE_API_URL || 'https://localhost:7271/api'
const HUB_URL = API_URL.replace('/api', '/quizhub')

let connection: signalR.HubConnection | null = null
let reconnectAttempt = 0
const MAX_RECONNECT_ATTEMPTS = 10

export interface OngoingParticipant {
  sessionId: string
  name: string
  avatarUrl: string
  difficulty: string
  startedAtMs: number
  lastActivityMs: number
  currentQuestionIndex: number
  totalQuestions: number
  totalPenaltyMs: number
}

export interface ParticipantCompletion {
  sessionId: string
  name: string
  avatarUrl: string
  difficulty: string
  totalMs: number
  ranking: number
  isTopThree: boolean
  isOnLeaderboard: boolean
}

export interface LeaderboardEntry {
  name: string
  totalMs: number
  avatarUrl: string
}

type LeaderboardUpdateCallback = (difficulty: string, entries: LeaderboardEntry[]) => void
type ParticipantStartedCallback = (participant: OngoingParticipant) => void
type ParticipantProgressCallback = (participant: OngoingParticipant) => void
type ParticipantCompletedCallback = (completion: ParticipantCompletion) => void

const callbacks = {
  leaderboardUpdate: [] as LeaderboardUpdateCallback[],
  participantStarted: [] as ParticipantStartedCallback[],
  participantProgress: [] as ParticipantProgressCallback[],
  participantCompleted: [] as ParticipantCompletedCallback[]
}

async function startConnection(): Promise<void> {
  if (connection?.state === signalR.HubConnectionState.Connected) {
    return
  }

  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(HUB_URL, {
        withCredentials: true
      })
      .withAutomaticReconnect({
        nextRetryDelayInMilliseconds: () => {
          // Exponential backoff: 0s, 2s, 4s, 8s, 16s, 32s (max)
          const delay = Math.min(1000 * Math.pow(2, reconnectAttempt), 32000)
          reconnectAttempt++
          return delay
        }
      })
      .build()

    // Register event handlers
    connection.on('LeaderboardUpdate', (difficulty: string, entries: LeaderboardEntry[]) => {
      callbacks.leaderboardUpdate.forEach(cb => cb(difficulty, entries))
    })

    connection.on('ParticipantStarted', (participant: OngoingParticipant) => {
      callbacks.participantStarted.forEach(cb => cb(participant))
    })

    connection.on('ParticipantProgress', (participant: OngoingParticipant) => {
      callbacks.participantProgress.forEach(cb => cb(participant))
    })

    connection.on('ParticipantCompleted', (completion: ParticipantCompletion) => {
      callbacks.participantCompleted.forEach(cb => cb(completion))
    })

    connection.onreconnecting(() => {
      console.log('SignalR reconnecting...')
    })

    connection.onreconnected(() => {
      console.log('SignalR reconnected')
      reconnectAttempt = 0
    })

    connection.onclose(async (error) => {
      console.error('SignalR connection closed', error)
      if (reconnectAttempt < MAX_RECONNECT_ATTEMPTS) {
        await new Promise(resolve => setTimeout(resolve, 5000))
        await startConnection()
      }
    })
  }

  try {
    await connection.start()
    console.log('SignalR connected')
    reconnectAttempt = 0
  } catch (error) {
    console.error('SignalR connection failed', error)
    if (reconnectAttempt < MAX_RECONNECT_ATTEMPTS) {
      reconnectAttempt++
      await new Promise(resolve => setTimeout(resolve, 5000))
      await startConnection()
    }
  }
}

async function stopConnection(): Promise<void> {
  if (connection) {
    await connection.stop()
    connection = null
    reconnectAttempt = 0
  }
}

function onLeaderboardUpdate(callback: LeaderboardUpdateCallback): () => void {
  callbacks.leaderboardUpdate.push(callback)
  return () => {
    const index = callbacks.leaderboardUpdate.indexOf(callback)
    if (index > -1) {
      callbacks.leaderboardUpdate.splice(index, 1)
    }
  }
}

function onParticipantStarted(callback: ParticipantStartedCallback): () => void {
  callbacks.participantStarted.push(callback)
  return () => {
    const index = callbacks.participantStarted.indexOf(callback)
    if (index > -1) {
      callbacks.participantStarted.splice(index, 1)
    }
  }
}

function onParticipantProgress(callback: ParticipantProgressCallback): () => void {
  callbacks.participantProgress.push(callback)
  return () => {
    const index = callbacks.participantProgress.indexOf(callback)
    if (index > -1) {
      callbacks.participantProgress.splice(index, 1)
    }
  }
}

function onParticipantCompleted(callback: ParticipantCompletedCallback): () => void {
  callbacks.participantCompleted.push(callback)
  return () => {
    const index = callbacks.participantCompleted.indexOf(callback)
    if (index > -1) {
      callbacks.participantCompleted.splice(index, 1)
    }
  }
}

export default {
  startConnection,
  stopConnection,
  onLeaderboardUpdate,
  onParticipantStarted,
  onParticipantProgress,
  onParticipantCompleted
}
