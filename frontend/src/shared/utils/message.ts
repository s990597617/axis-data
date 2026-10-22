import { createDiscreteApi, darkTheme, type ConfigProviderProps } from 'naive-ui'
import { computed } from 'vue'

import { useThemeStore } from '@/store/modules/theme.store'
import { derivePrimaryPalette } from '@/shared/utils/color'

/* ============ 离散 API 主题 ============ */

// 通过 computed 惰性读 theme store，避免 Pinia 未安装时访问失败
const configProviderPropsRef = computed<ConfigProviderProps>(() => {
  try {
    const themeStore = useThemeStore()
    return {
      theme: themeStore.darkMode ? darkTheme : null,
      themeOverrides: {
        common: {
          ...derivePrimaryPalette(themeStore.primaryColor),
          borderRadius: `${themeStore.borderRadius}px`
        }
      }
    }
  } catch {
    return {}
  }
})

/* ============ 导出 ============ */

// notification 固定右下角，供 SignalR 新消息提醒等场景使用
const { message, notification, dialog, loadingBar } = createDiscreteApi(
  ['message', 'notification', 'dialog', 'loadingBar'],
  {
    configProviderProps: configProviderPropsRef,
    notificationProviderProps: {
      placement: 'bottom-right',
      max: 3,
      containerStyle: 'z-index: 5000;'
    }
  }
)

export { message, notification, dialog, loadingBar }
