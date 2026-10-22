# 本地 SVG 图标

此目录下的所有 `*.svg` 会在构建时被扫描并注册为 iconify 图标集 `axis`。

## 使用

```vue
<template>
  <Icon icon="axis:axis-logo" />
</template>

<script setup lang="ts">
import { Icon } from '@/shared/icons'
</script>
```

路由 `meta.icon` 也可以直接写：

```ts
meta: {
  icon: 'axis:axis-logo'
}
```

## 命名规则

- 文件名（去掉 `.svg`）即图标名，建议使用 **短横线命名**：`arrow-up.svg` -> `axis:arrow-up`
- 支持子目录：`brand/github.svg` -> `axis:brand-github`

## SVG 规范建议

为了让图标能正常跟随文字颜色缩放，**推荐**：

- 保留 `viewBox` 属性（例：`viewBox="0 0 24 24"`）
- 颜色使用 `currentColor`，放弃硬编码填充色
- 去掉 `width` / `height` 或设为 `1em`
- 不需要 `xmlns`；若保留也无影响

示例：

```xml
<svg viewBox="0 0 24 24" fill="currentColor">
  <path d="..."/>
</svg>
```

## 新增流程

1. 把优化后的 SVG 丢到本目录（或子目录）
2. Vite HMR 会自动刷新，无需任何配置
3. 模板 / meta 里使用 `axis:<name>`

## 配置（`setupIcons`）

在 `src/main.ts` 里调用 `setupIcons()` 时可以传入配置：

```ts
import { setupIcons } from '@/shared/icons'

setupIcons({
  localIcons: {
    /**
     * 是否把 body 中硬编码的 fill="#xxxxxx" 统一替换为 fill="currentColor"
     * 让图标可以通过 CSS color 跟随主题色
     * @default true
     */
    forceCurrentColor: true,

    /**
     * 即使开启 forceCurrentColor，也要保留原色的图标白名单
     * 支持数组或谓词函数，图标名不含 `axis:` 前缀
     */
    preserveColorFor: ['axis-logo', 'brand-github'],
  },
})
```

### 场景示例

**默认（无需改动）** —— 全部单色跟随主题色：

```ts
setupIcons()
```

**关闭强制替换** —— 全部保留 SVG 原色：

```ts
setupIcons({ localIcons: { forceCurrentColor: false } })
```

**替换 + 白名单（数组）** —— 绝大多数跟随主题色，指定图标保留原色：

```ts
setupIcons({
  localIcons: {
    preserveColorFor: ['axis-logo', 'brand-github', 'brand-alipay'],
  },
})
```

**替换 + 白名单（函数）** —— 按命名约定一类一类豁免：

```ts
setupIcons({
  localIcons: {
    preserveColorFor: (name) => name.startsWith('brand-') || name.endsWith('-color'),
  },
})
```

### 优先级

| forceCurrentColor | preserveColorFor 命中 | 结果                       |
| ----------------- | --------------------- | -------------------------- |
| `true`（默认）    | ✅                    | **保留原色**（白名单优先） |
| `true`（默认）    | ❌                    | 替换为 `currentColor`      |
| `false`           | 无关                  | 保留原色                   |

> 和 `iconfont` 模块的配置接口完全一致，两者可以独立配置。
