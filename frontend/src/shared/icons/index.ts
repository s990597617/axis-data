import { addCollection, Icon, type IconifyJSON } from '@iconify/vue'

import antDesign from '@iconify-json/ant-design/icons.json'
import { registerLocalIcons, type LocalIconsOptions } from './local-icons'
import { registerIconfont, type IconfontOptions } from './iconfont'

/**
 * 项目内置图标集（纯离线）
 *
 * @description
 * 在此追加需要全量预加载的图标集。每个集合对应一个 `@iconify-json/*` 包。
 * `meta.icon` 使用规范：`<prefix>:<name>`，例如：
 * - `ant-design:dashboard-outlined`
 * - `tabler:users`
 *
 * 追加新集合步骤：
 * 1. `pnpm add -D @iconify-json/<prefix>`
 * 2. 在下方 import 并加入 `BUILT_IN_COLLECTIONS`
 */
const BUILT_IN_COLLECTIONS: IconifyJSON[] = [antDesign as unknown as IconifyJSON]

let installed = false

/**
 * 图标系统初始化配置
 */
export interface SetupIconsOptions {
  /**
   * 本地 SVG（`src/assets/icons/*.svg`）集成配置
   */
  localIcons?: LocalIconsOptions

  /**
   * 阿里 iconfont（`src/assets/iconfont/*.js`）集成配置
   */
  iconfont?: IconfontOptions
}

/**
 * 在应用启动时调用，注册全部内置图标集
 * 之后任意位置直接使用 `<Icon icon="ant-design:xxx" />`
 */
export function setupIcons(options: SetupIconsOptions = {}) {
  if (installed) return
  BUILT_IN_COLLECTIONS.forEach(collection => addCollection(collection))
  registerLocalIcons(options.localIcons)
  registerIconfont(options.iconfont)
  installed = true
}

export type { IconfontOptions, LocalIconsOptions }
export { Icon }
