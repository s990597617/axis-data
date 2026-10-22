# iconfont (阿里图标库) 集成

此目录用于放置从 [iconfont.cn](https://www.iconfont.cn/) 下载的 **Symbol JS** 文件。
构建时会自动提取其中的 `<symbol>` 并注册为 iconify 集合 `iconfont`。

## 获取方式

1. 登录 <https://www.iconfont.cn/>，进入你的项目
2. 点击 "Symbol" 方式 → "查看在线链接"
3. 右键 "下载至本地" —— 得到一个 `iconfont.js`
4. 放到本目录 `src/assets/iconfont/iconfont.js`

也可以直接把 iconfont 提供的在线 URL（`//at.alicdn.com/t/font_xxx.js`）的响应内容保存下来。

## 使用

```vue
<template>
  <!-- iconfont 上 id="icon-alipay" 的图标 -->
  <Icon icon="iconfont:alipay" />
</template>

<script setup lang="ts">
import { Icon } from '@/shared/icons'
</script>
```

路由 `meta.icon` 同理：

```ts
meta: {
  icon: 'iconfont:alipay'
}
```

## 命名规则

iconfont 的 id 约定是 `icon-<name>`，本模块解析时会自动剥掉 `icon-` 前缀。
所以 iconfont 里的 `icon-alipay` -> `iconfont:alipay`。

## 多个 iconfont 项目

本目录下的所有 `*.js` 都会被扫描、合并。如果不同项目有重名图标，**后加载的会覆盖先加载的**，请避免命名冲突。

## 色彩建议

iconfont 上的图标可能存在以下几种形态：

- **单色（线性/面性）** → 下载前在 iconfont 编辑页面把颜色清空（或保持默认 `#333`），配合 CSS `color: ...` 即可跟随主题色
- **多色图标** → 无法通过 `color` 统一控色；要么保留原色，要么下载 SVG 手动改成 `currentColor`

## 配置（`setupIcons`）

在 `src/main.ts` 里调用 `setupIcons()` 时可以传入配置：

```ts
import { setupIcons } from '@/shared/icons'

setupIcons({
  iconfont: {
    /**
     * 是否把 body 中硬编码的 fill="#xxxxxx" 统一替换为 fill="currentColor"
     * 让图标可以通过 CSS color 跟随主题色
     * @default true
     */
    forceCurrentColor: true,

    /**
     * 即使开启 forceCurrentColor，也要保留原色的图标白名单
     * 支持数组或谓词函数
     */
    preserveColorFor: ['brand-logo', 'alipay'],
  },
})
```

### 场景示例

**默认（无需改动）** —— 全部单色跟随主题色：

```ts
setupIcons()
```

**关闭强制替换** —— 全部保留 iconfont 原色：

```ts
setupIcons({ iconfont: { forceCurrentColor: false } })
```

**替换 + 白名单（数组）** —— 绝大多数跟随主题色，指定图标保留原色：

```ts
setupIcons({
  iconfont: {
    preserveColorFor: ['brand-logo', 'alipay', 'wechat-pay'],
  },
})
```

**替换 + 白名单（函数）** —— 按命名约定一类一类豁免：

```ts
setupIcons({
  iconfont: {
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
