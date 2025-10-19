<template>
  <div class="min-h-screen bg-gray-100 p-4">
    <div class="max-w-6xl mx-auto">
      <h1 class="text-3xl font-bold mb-6 text-gray-900">Admin Panel</h1>

      <!-- Login Form -->
      <div v-if="!isAuthenticated" class="bg-white rounded-lg shadow p-6 max-w-md">
        <h2 class="text-xl font-semibold mb-4 text-gray-900">Login</h2>
        <form @submit.prevent="handleLogin">
          <div class="mb-4">
            <label for="password" class="block text-sm font-medium text-gray-700 mb-2">
              Password
            </label>
            <input
              id="password"
              v-model="password"
              type="password"
              class="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500 text-gray-900 bg-white"
              required
            />
          </div>
          <div v-if="loginError" class="mb-4 text-red-600 text-sm">
            {{ loginError }}
          </div>
          <button
            type="submit"
            class="w-full bg-blue-600 text-white py-2 px-4 rounded-md hover:bg-blue-700 transition"
            :disabled="isLoggingIn"
          >
            {{ isLoggingIn ? 'Logging in...' : 'Login' }}
          </button>
        </form>
      </div>

      <!-- Leaderboard Management -->
      <div v-else>
        <div class="mb-4 flex justify-between items-center">
          <div class="space-x-2">
            <button
              @click="selectedView = 'leaderboard'"
              :class="[
                'px-4 py-2 rounded-md transition',
                selectedView === 'leaderboard'
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
              ]"
            >
              Leaderboard
            </button>
            <button
              @click="selectedView = 'contacts'; loadContacts()"
              :class="[
                'px-4 py-2 rounded-md transition',
                selectedView === 'contacts'
                  ? 'bg-blue-600 text-white'
                  : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
              ]"
            >
              Contacts
            </button>
          </div>
          <button
            @click="handleLogout"
            class="px-4 py-2 bg-red-600 text-white rounded-md hover:bg-red-700 transition"
          >
            Logout
          </button>
        </div>

        <!-- Leaderboard Difficulty Filter -->
        <div v-if="selectedView === 'leaderboard'" class="mb-4 space-x-2">
          <button
            @click="selectedDifficulty = 'Noob'"
            :class="[
              'px-4 py-2 rounded-md transition',
              selectedDifficulty === 'Noob'
                ? 'bg-green-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            ]"
          >
            Noob
          </button>
          <button
            @click="selectedDifficulty = 'Nerd'"
            :class="[
              'px-4 py-2 rounded-md transition',
              selectedDifficulty === 'Nerd'
                ? 'bg-green-600 text-white'
                : 'bg-white text-gray-700 border border-gray-300 hover:bg-gray-50'
            ]"
          >
            Nerd
          </button>
        </div>

        <div v-if="error" class="mb-4 bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
          {{ error }}
        </div>

        <!-- Leaderboard View -->
        <div v-if="selectedView === 'leaderboard'">
          <div v-if="isLoading" class="bg-white rounded-lg shadow p-6">
            <p class="text-center text-gray-600">Loading...</p>
          </div>

          <div v-else class="bg-white rounded-lg shadow overflow-x-auto">
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Rank
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Name
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Phone Number
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Email
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Time
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Actions
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="(entry, index) in leaderboard" :key="entry.participantId">
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ index + 1 }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap">
                    <div class="flex items-center">
                      <img
                        :src="entry.avatarUrl"
                        :alt="entry.name"
                        class="h-8 w-8 rounded-full mr-3"
                      />
                      <span class="text-sm font-medium text-gray-900">{{ entry.name }}</span>
                    </div>
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ entry.phone }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ entry.email || '-' }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatTime(entry.totalMs) }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm">
                    <button
                      @click="handleDelete(entry.participantId, entry.name)"
                      class="text-red-600 hover:text-red-900 font-medium"
                      :disabled="isDeletingId === entry.participantId"
                    >
                      {{ isDeletingId === entry.participantId ? 'Deleting...' : 'Delete' }}
                    </button>
                  </td>
                </tr>
                <tr v-if="leaderboard.length === 0">
                  <td colspan="6" class="px-6 py-4 text-center text-sm text-gray-500">
                    No participants yet
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- Contacts View -->
        <div v-else-if="selectedView === 'contacts'">
          <div v-if="isLoadingContacts" class="bg-white rounded-lg shadow p-6">
            <p class="text-center text-gray-600">Loading contacts...</p>
          </div>

          <div v-else class="bg-white rounded-lg shadow overflow-x-auto">
            <div class="px-6 py-4 bg-gray-50 border-b border-gray-200 flex justify-between items-start">
              <div>
                <h2 class="text-lg font-semibold text-gray-900">
                  Participants Who Want to be Contacted by Capgemini
                </h2>
                <p class="text-sm text-gray-600 mt-1">
                  Total: {{ contacts.length }} participant{{ contacts.length !== 1 ? 's' : '' }}
                </p>
              </div>
              <button
                v-if="contacts.length > 0"
                @click="exportContactsAsCSV"
                class="px-4 py-2 bg-green-600 text-white rounded-md hover:bg-green-700 transition text-sm font-medium"
              >
                Export CSV
              </button>
            </div>
            <table class="min-w-full divide-y divide-gray-200">
              <thead class="bg-gray-50">
                <tr>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Name
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Email
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Phone Number
                  </th>
                  <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                    Registered On
                  </th>
                </tr>
              </thead>
              <tbody class="bg-white divide-y divide-gray-200">
                <tr v-for="contact in contacts" :key="contact.participantId">
                  <td class="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900">
                    {{ contact.name }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ contact.email }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ contact.phone }}
                  </td>
                  <td class="px-6 py-4 whitespace-nowrap text-sm text-gray-900">
                    {{ formatDate(contact.createdAtUtc) }}
                  </td>
                </tr>
                <tr v-if="contacts.length === 0">
                  <td colspan="4" class="px-6 py-4 text-center text-sm text-gray-500">
                    No participants have provided their email yet
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { api, type AdminLeaderboardEntry, type AdminContact } from '@/lib/api'

const password = ref('')
const isAuthenticated = ref(false)
const token = ref<string | null>(null)
const loginError = ref('')
const isLoggingIn = ref(false)

const selectedDifficulty = ref<'Noob' | 'Nerd'>('Noob')
const leaderboard = ref<AdminLeaderboardEntry[]>([])
const contacts = ref<AdminContact[]>([])
const isLoading = ref(false)
const isLoadingContacts = ref(false)
const error = ref('')
const isDeletingId = ref<string | null>(null)
const selectedView = ref<'leaderboard' | 'contacts'>('leaderboard')

onMounted(() => {
  const storedToken = sessionStorage.getItem('adminToken')
  if (storedToken) {
    token.value = storedToken
    isAuthenticated.value = true
    loadLeaderboard()
  }
})

watch(selectedDifficulty, () => {
  if (isAuthenticated.value) {
    loadLeaderboard()
  }
})

async function handleLogin() {
  isLoggingIn.value = true
  loginError.value = ''

  try {
    const response = await api.adminLogin(password.value)

    if (response.success && response.token) {
      token.value = response.token
      sessionStorage.setItem('adminToken', response.token)
      isAuthenticated.value = true
      await loadLeaderboard()
    } else {
      loginError.value = response.message || 'Invalid password'
    }
  } catch {
    loginError.value = 'Login failed. Please try again.'
  } finally {
    isLoggingIn.value = false
  }
}

function handleLogout() {
  token.value = null
  isAuthenticated.value = false
  sessionStorage.removeItem('adminToken')
  password.value = ''
}

async function loadLeaderboard() {
  if (!token.value) return

  isLoading.value = true
  error.value = ''

  try {
    leaderboard.value = await api.getAdminLeaderboard(token.value, selectedDifficulty.value)
  } catch (err) {
    if (err instanceof Error && err.message.includes('Unauthorized')) {
      error.value = 'Session expired. Please login again.'
      handleLogout()
    } else {
      error.value = 'Failed to load leaderboard'
    }
  } finally {
    isLoading.value = false
  }
}

async function handleDelete(participantId: string, name: string) {
  if (!token.value) return

  if (!confirm(`Are you sure you want to delete ${name}? This will remove all their data.`)) {
    return
  }

  isDeletingId.value = participantId
  error.value = ''

  try {
    await api.deleteParticipant(token.value, participantId)
    await loadLeaderboard()
  } catch (err) {
    if (err instanceof Error && err.message.includes('Unauthorized')) {
      error.value = 'Session expired. Please login again.'
      handleLogout()
    } else {
      error.value = 'Failed to delete participant'
    }
  } finally {
    isDeletingId.value = null
  }
}

function formatTime(ms: number): string {
  return (ms / 1000).toFixed(3) + 's'
}

function formatDate(dateString: string): string {
  const date = new Date(dateString)
  return date.toLocaleDateString() + ' ' + date.toLocaleTimeString()
}

async function loadContacts() {
  if (!token.value) return

  isLoadingContacts.value = true
  error.value = ''

  try {
    contacts.value = await api.getAdminContacts(token.value)
  } catch (err) {
    if (err instanceof Error && err.message.includes('Unauthorized')) {
      error.value = 'Session expired. Please login again.'
      handleLogout()
    } else {
      error.value = 'Failed to load contacts'
    }
  } finally {
    isLoadingContacts.value = false
  }
}

function exportContactsAsCSV() {
  if (contacts.value.length === 0) {
    return
  }

  // CSV headers
  const headers = ['Name', 'Email', 'Phone Number', 'Registered On']

  // CSV rows
  const rows = contacts.value.map(contact => [
    contact.name,
    contact.email,
    contact.phone,
    formatDate(contact.createdAtUtc)
  ])

  // Escape CSV fields properly (handle quotes, commas, newlines)
  const escapeCSVField = (field: string): string => {
    if (field.includes('"') || field.includes(',') || field.includes('\n') || field.includes('\r')) {
      return `"${field.replace(/"/g, '""')}"`
    }
    return field
  }

  // Combine headers and rows with proper escaping
  const csvContent = [
    headers.map(h => escapeCSVField(h)).join(','),
    ...rows.map(row => row.map(field => escapeCSVField(field)).join(','))
  ].join('\n')

  // Create blob and download
  const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' })
  const link = document.createElement('a')
  const url = URL.createObjectURL(blob)

  link.setAttribute('href', url)
  link.setAttribute('download', `devquiz-contacts-${new Date().toISOString().split('T')[0]}.csv`)
  link.style.visibility = 'hidden'

  document.body.appendChild(link)
  link.click()
  document.body.removeChild(link)

  URL.revokeObjectURL(url)
}
</script>
