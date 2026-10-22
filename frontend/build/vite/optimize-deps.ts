import type { UserConfig } from 'vite'

/** 依赖预构建配置，提前处理高频依赖，降低开发环境首次访问等待时间。 */
export const optimizeDepsConfig: UserConfig['optimizeDeps'] = {
  include: [
    'vue',
    'vue-router',
    'pinia',
    'naive-ui',
    'axios',
    '@vueuse/core',
    '@iconify/vue',
    'lodash-es',
    'qs'
  ]
}
