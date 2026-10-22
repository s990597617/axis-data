/**
 * 应用布局模式
 *
 * @description
 * - `vertical` 左侧菜单 + 顶部工具栏（默认）
 * - `horizontal` 顶部一级菜单 + 工具栏
 * - `split` 左侧一级图标 + 中间二级菜单
 * - `mix` 顶部一级 + 左侧二级
 */
export type LayoutMode = 'vertical' | 'horizontal' | 'split' | 'mix'

/**
 * 页面切换动画类型
 */
export type PageAnimateType = 'fade-slide' | 'fade' | 'slide-left' | 'none'

/**
 * 应用布局配置
 *
 * @description
 * 布局系统的全局运行时配置，由 `app.store` 持有并持久化。
 * 所有布局模式与公共组件通过该配置消费用户偏好。
 */
export interface LayoutConfig {
  /** 布局模式 */
  mode: LayoutMode
  /** 侧边栏是否折叠 */
  collapsed: boolean
  /** 是否显示侧边栏 */
  showSidebar: boolean
  /** 是否显示 Logo */
  showLogo: boolean
  /** 是否固定顶部 Header */
  fixedHeader: boolean
  /** 是否显示顶部 Header */
  showHeader: boolean
  /** 是否显示标签栏 */
  showTabs: boolean
  /** 是否显示面包屑 */
  showBreadcrumb: boolean
  /** 是否显示页脚 */
  showFooter: boolean
  /** 是否启用菜单手风琴模式（同级只展开一个） */
  accordionMenu: boolean
  /** 侧边栏展开宽度（px） */
  siderWidth: number
  /** 侧边栏折叠后宽度（px） */
  siderCollapsedWidth: number
  /** 顶部 Header 高度（px） */
  headerHeight: number
  /** 标签栏高度（px） */
  tabsHeight: number
  /** 是否启用页面切换动画 */
  pageAnimate: boolean
  /** 页面切换动画类型 */
  pageAnimateType: PageAnimateType
  /** 是否显示全局水印 */
  showWatermark: boolean
  /** 水印文案；为空时使用当前用户姓名与机构 */
  watermarkContent: string
}
