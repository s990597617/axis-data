import { createApp } from 'vue'

import App from './App.vue'
import { setupStore } from '@/store'
import { setupRouter } from '@/router'
import { setupIcons } from '@/shared/icons'
import { iconsConfig } from '@/config/icons.config'
import { setupDirectives } from '@/shared/directives'
import { setupGlobalErrorHandler } from '@/shared/error'

import '@/assets/styles/theme-variables.scss'
import '@/assets/styles/nprogress.scss'
import '@/assets/styles/inbox-notify.scss'
import 'uno.css'
// 系统字体优先；Noto Sans SC 仅作为缺少中文系统字体时的兜底。
import '@/assets/styles/fonts.scss'

async function bootstrap() {
  const app = createApp(App)

  setupGlobalErrorHandler(app)
  setupIcons(iconsConfig)
  setupDirectives(app)
  setupStore(app)
  setupRouter(app)

  app.mount('#app')

  // 应用挂载完成，淡出并移除首屏 Splash，避免闪一下白屏
  const splash = document.getElementById('app-splash')
  if (splash) {
    splash.classList.add('is-hidden')
    splash.addEventListener('transitionend', () => splash.remove(), { once: true })
    // 兜底：过渡时长 400ms，多留一点余量以防 transitionend 因为某些原因没触发
    setTimeout(() => splash.remove(), 700)
  }
}

await bootstrap()
