import type { SetupIconsOptions } from '@/shared/icons'

/**
 * 图标系统默认配置
 *
 * @description
 * `setupIcons` 的入参集中在此声明，业务方只需在此调整即可全局生效：
 * - `localIcons`：`src/assets/icons/*.svg` 注册为 iconify 图标，使用 `axis:<name>`
 * - `iconfont`：`src/assets/iconfont/*.js`（阿里 iconfont symbol 脚本），使用 `iconfont:<name>`
 *
 * @remarks
 * 默认开启 `forceCurrentColor`，让硬编码 `fill="#xxxxxx"` 的 SVG 也能跟随 `color`
 * 属性继承主题色。若某些品牌图标需要保留原色，在 `preserveColorFor` 中声明即可。
 */
export const iconsConfig: SetupIconsOptions = {
  localIcons: {
    forceCurrentColor: true,
    // preserveColorFor: ['axis-logo', 'brand-github'],
  },
  iconfont: {
    forceCurrentColor: true,
    // preserveColorFor: (name) => name.startsWith('brand-'),
  },
}
