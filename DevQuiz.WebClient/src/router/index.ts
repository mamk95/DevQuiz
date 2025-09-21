import { createRouter, createWebHistory } from 'vue-router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'join',
      component: () => import('../views/JoinView.vue'),
    },
    {
      path: '/quiz',
      name: 'quiz',
      component: () => import('../views/QuizView.vue'),
    },
    {
      path: '/finish',
      name: 'finish',
      component: () => import('../views/FinishView.vue'),
    },
    {
      path: '/kiosk',
      name: 'kiosk',
      component: () => import('../views/KioskView.vue'),
    },
  ],
})

export default router
