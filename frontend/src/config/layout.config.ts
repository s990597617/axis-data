import type { LayoutConfig, PageAnimateType } from '@/shared/types/app-layout'

/**
 * 默认布局配置
 *
 * @description
 * 应用首次运行或重置布局时使用的基线配置。
 * 用户偏好会在 `app.store` 中基于此对象合并并持久化到 localStorage。
 */
export const defaultLayoutConfig: LayoutConfig = {
  mode: 'split',
  collapsed: false,
  showSidebar: true,
  showLogo: true,
  showHeader: true,
  fixedHeader: true,
  showTabs: true,
  showBreadcrumb: true,
  showFooter: false,
  accordionMenu: false,
  siderWidth: 180,
  siderCollapsedWidth: 64,
  headerHeight: 56,
  tabsHeight: 40,
  pageAnimate: true,
  pageAnimateType: 'fade-slide',
  showWatermark: false,
  watermarkContent: '',
}

/**
 * 页面切换动画可选项
 */
export interface PageAnimateOption {
  label: string
  value: PageAnimateType
}

export const pageAnimateOptions: readonly PageAnimateOption[] = [
  { label: '淡入滑动', value: 'fade-slide' },
  { label: '淡入', value: 'fade' },
  { label: '左滑', value: 'slide-left' },
  { label: '无', value: 'none' }
] as const

// Storage key 集中写在 `@/config/storage-keys.ts`
