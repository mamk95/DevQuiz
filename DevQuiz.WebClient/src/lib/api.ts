export interface ApiError {
  message: string
  statusCode?: number
}

// API response interfaces
interface RawQuestion {
  type?: string
  prompt?: string
  choices?: string[]
  codeSnippet?: string
  initialCode?: string
  testCode?: string
  done?: boolean
  totalMs?: number
  questionIndex?: number
}

interface RawAnswerResponse {
  correct?: boolean
  penaltyMsAdded?: number
  quizCompleted?: boolean
  totalMs?: number
}

interface RawLeaderboardEntry {
  name?: string
  totalMs?: number
}

export interface StartSessionRequest {
  name: string
  phone: string
}

export interface StartSessionResponse {
  success: boolean
  message?: string
}

export interface ResumeSessionResponse {
  questionIndex: number
  finished: boolean
  participantName: string
  participantPhone: string
  totalTimeMs: number | null
  success: boolean
  message?: string
}

export interface Question {
  type?: 'MultipleChoice' | 'CodeFix'
  prompt?: string
  choices?: string[]
  codeSnippet?: string
  initialCode?: string
  testCode?: string
  done?: boolean
  totalMs?: number
  questionIndex?: number
}

export interface AnswerRequest {
  answerText: string
}

export interface AnswerResponse {
  correct: boolean
  penaltyMsAdded?: number
  quizCompleted?: boolean
  totalMs?: number
}

export interface LeaderboardEntry {
  name: string
  totalMs: number
}

class ApiClient {
  private baseUrl: string

  constructor() {
    this.baseUrl = import.meta.env.VITE_API_URL || 'https://localhost:7271/api'
  }

  private async handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
      let errorMessage = 'An error occurred'
      try {
        const errorData = await response.json()
        errorMessage = errorData.message || errorData.error || errorMessage
      } catch {
        if (response.status === 400) {
          errorMessage = 'Invalid request'
        } else if (response.status === 401) {
          errorMessage = 'Unauthorized'
        } else if (response.status === 500) {
          errorMessage = 'Server error'
        }
      }
      throw new Error(errorMessage)
    }

    try {
      return await response.json()
    } catch {
      return {} as T
    }
  }

  async startSession(name: string, phone: string): Promise<StartSessionResponse> {
    const response = await fetch(`${this.baseUrl}/session/start`, {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ name, phone }),
    })

    const data = await this.handleResponse<{ success: boolean; message?: string }>(response)
    return {
      success: data.success ?? false,
      message: data.message,
    }
  }

  async resumeSession(): Promise<ResumeSessionResponse> {
    const response = await fetch(`${this.baseUrl}/session/resume`, {
      method: 'GET',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
    })

    const data = await this.handleResponse<ResumeSessionResponse>(response)

    return data
  }

  async getCurrentQuestion(): Promise<Question> {
    const response = await fetch(`${this.baseUrl}/quiz/current`, {
      method: 'GET',
      credentials: 'include',
    })

    const data = await this.handleResponse<RawQuestion>(response)

    // Map API response to frontend format
    // API sends "MC" but frontend expects "MultipleChoice"
    let type: 'MultipleChoice' | 'CodeFix' | undefined
    if (data.type === 'MC') {
      type = 'MultipleChoice'
    } else if (data.type === 'CodeFix') {
      type = 'CodeFix'
    }

    return {
      type,
      prompt: data.prompt,
      choices: data.choices,
      codeSnippet: data.codeSnippet,
      initialCode: data.initialCode,
      testCode: data.testCode,
      done: data.done,
      totalMs: data.totalMs,
      questionIndex: data.questionIndex,
    }
  }

  async submitAnswer(answerText: string): Promise<AnswerResponse> {
    const response = await fetch(`${this.baseUrl}/quiz/answer`, {
      method: 'POST',
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ answerText }),
    })

    const data = await this.handleResponse<RawAnswerResponse>(response)

    return {
      correct: data.correct ?? false,
      penaltyMsAdded: data.penaltyMsAdded,
      quizCompleted: data.quizCompleted,
      totalMs: data.totalMs,
    }
  }

  async getLeaderboard(limit: number = 10): Promise<LeaderboardEntry[]> {
    const response = await fetch(`${this.baseUrl}/leaderboard/top?limit=${limit}`, {
      method: 'GET',
    })

    const data = await this.handleResponse<RawLeaderboardEntry[]>(response)

    return data.map((entry) => ({
      name: entry.name ?? '',
      totalMs: entry.totalMs ?? 0,
    }))
  }
}

export const api = new ApiClient()
