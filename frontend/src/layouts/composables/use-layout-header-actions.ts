import { computed, onMounted, onUnmounted, ref } from 'vue'
import { useRouter } from 'vue-router'

import defaultAvatar from '@/assets/images/avatar.svg'
import { useLayoutCollapse } from '@/layouts/composables/use-layout-collapse'
import { useLayoutTabs } from '@/layouts/composables/use-layout-tabs'
import { dialog, message } from '@/shared/utils/message'
import { useAppStore } from '@/store/modules/app.store'
import { useThemeStore } from '@/store/modules/theme.store'
import { useUserStore } from '@/store/modules/user.store'

export function useLayoutHeaderActions() {
  const router = useRouter()
  const appStore = useAppStore()
  const themeStore = useThemeStore()
  const userStore = useUserStore()
  const { collapsed, toggle } = useLayoutCollapse()
  const { refresh } = useLayoutTabs()

  const nickname = computed(
    () => userStore.userInfo?.nickname ?? userStore.userInfo?.username ?? '未登录',
  )
  const avatarSrc = computed(() => userStore.userInfo?.avatar || defaultAvatar)
  const userRole = computed(() => {
    const roles = userStore.userInfo?.roles ?? []
    if (roles.includes('super-admin')) return '超级管理员'
    if (roles.includes('sys-admin')) return '系统管理员'
    return roles[0] ?? '用户'
  })
  const userEmail = computed(() => userStore.userInfo?.email)

  const isFullscreen = ref(!!document.fullscreenElement)
  function toggleFullscreen() {
    if (!document.fullscreenElement) {
      document.documentElement.requestFullscreen()
    } else {
      document.exitFullscreen()
    }
  }
  function onFullscreenChange() {
    isFullscreen.value = !!document.fullscreenElement
  }

  onMounted(() => document.addEventListener('fullscreenchange', onFullscreenChange))
  onUnmounted(() => document.removeEventListener('fullscreenchange', onFullscreenChange))

  function toggleTheme() {
    const mode = themeStore.darkMode ? 'light' : 'dark'
    themeStore.setThemeMode(mode)
    themeStore.update({
      sidebarTheme: mode,
      sidebarColor: '',
      topbarTheme: mode,
      topbarColor: '',
    })
  }

  const themeIcon = computed(() =>
    themeStore.darkMode ? 'ant-design:moon-outlined' : 'ant-design:sun-outlined',
  )
  const themeTitle = computed(() => (themeStore.darkMode ? '切换亮色' : '切换暗色'))

  const searchVisible = ref(false)

  function handleLogout() {
    dialog.warning({
      title: '退出确认',
      content: '确定要退出登录吗？',
      positiveText: '确认退出',
      negativeText: '取消',
      onPositiveClick: async () => {
        const currentPath = router.currentRoute.value.fullPath
        await userStore.logout(router)
        message.success('已退出登录')
        router.replace({
          name: 'Login',
          query: currentPath && currentPath !== '/' ? { redirect: currentPath } : undefined,
        })
      },
    })
  }

  return {
    appStore,
    avatarSrc,
    collapsed,
    handleLogout,
    isFullscreen,
    nickname,
    refresh,
    searchVisible,
    themeIcon,
    themeTitle,
    toggle,
    toggleFullscreen,
    toggleTheme,
    userEmail,
    userRole,
    userStore,
  }
}
