<script setup lang="ts">
  import { computed, ref, watch } from 'vue'
  import { listIcons } from '@iconify/vue'

  import { Icon } from '@/shared/icons'
  import { message } from '@/shared/utils/message.ts'

  defineOptions({ name: 'Icons' })

  /**
   * 全部已注册图标（构建时通过 setupIcons 内联）
   */
  const allIcons = computed<string[]>(() => listIcons())

  /**
   * 统计每个前缀的图标数量
   */
  const prefixStats = computed(() => {
    const map = new Map<string, number>()
    allIcons.value.forEach(name => {
      const [p] = name.split(':')
      if (!p) return
      map.set(p, (map.get(p) ?? 0) + 1)
    })
    return Array.from(map.entries())
      .map(([value, count]) => ({ value, count }))
      .sort((a, b) => a.value.localeCompare(b.value))
  })

  const prefixOptions = computed(() => [
    { label: `全部 (${allIcons.value.length})`, value: '' },
    ...prefixStats.value.map(({ value, count }) => ({
      label: `${value} (${count})`,
      value
    }))
  ])

  const search = ref('')
  const prefix = ref('')
  const useUnifiedColor = ref(true)
  const unifiedColor = ref('#18a058')
  const iconSize = ref(32)
  const pageSize = ref(72)
  const page = ref(1)

  /** 选择器体验区绑定值 */
  const demoPicked = ref<string | null>('ant-design:appstore-outlined')

  const filteredIcons = computed(() => {
    const q = search.value.trim().toLowerCase()
    return allIcons.value.filter(name => {
      if (prefix.value && !name.startsWith(`${prefix.value}:`)) return false
      if (q && !name.toLowerCase().includes(q)) return false
      return true
    })
  })

  const pagedIcons = computed(() => {
    const start = (page.value - 1) * pageSize.value
    return filteredIcons.value.slice(start, start + pageSize.value)
  })

  /**
   * 过滤条件变化时回到第一页
   */
  watch([search, prefix, pageSize], () => {
    page.value = 1
  })

  /**
   * 容器内联样式：统一色模式下通过 color 影响 currentColor 路径
   */
  const itemStyle = computed(() => ({
    color: useUnifiedColor.value ? unifiedColor.value : 'inherit',
    fontSize: `${iconSize.value}px`
  }))

  async function copyName(name: string) {
    try {
      await navigator.clipboard.writeText(name)
      message.success(`已复制：${name}`)
    } catch {
      message.error('复制失败，请手动复制')
    }
  }
</script>

<template>
  <div class="icons-page">
    <!-- 顶部标题 + 前缀统计 -->
    <div class="icons-page__header">
      <div class="icons-page__title">
        <Icon icon="ant-design:appstore-outlined" class="icons-page__title-icon" />
        <div>
          <h2 class="icons-page__heading">图标浏览</h2>
          <p class="icons-page__subtitle">管理项目内所有已注册的 iconify 图标，点击即可复制</p>
        </div>
      </div>

      <div class="icons-page__stats">
        <div class="stat-card stat-card--primary">
          <div class="stat-card__value">{{ allIcons.length }}</div>
          <div class="stat-card__label">总图标数</div>
        </div>
        <div v-for="item in prefixStats" :key="item.value" class="stat-card">
          <div class="stat-card__value">{{ item.count }}</div>
          <div class="stat-card__label">{{ item.value }}</div>
        </div>
      </div>
    </div>

    <!-- 图标选择器体验区 -->
    <n-card class="icons-page__picker-demo" :bordered="false">
      <div class="picker-demo">
        <div class="picker-demo__text">
          <div class="picker-demo__title">图标选择器组件</div>
          <div class="picker-demo__subtitle">
            配菜单 / 表单时可直接使用 <code>&lt;AxisIconPicker /&gt;</code>，支持 v-model
          </div>
        </div>
        <div class="picker-demo__control">
          <AxisIconPicker v-model:value="demoPicked" :width="280" placeholder="点击选择图标" />
          <n-tag v-if="demoPicked" type="info" size="small" :bordered="false">
            当前：{{ demoPicked }}
          </n-tag>
        </div>
      </div>
    </n-card>

    <!-- 控制栏 -->
    <n-card class="icons-page__toolbar" :bordered="false">
      <div class="toolbar-grid">
        <n-input
          v-model:value="search"
          placeholder="搜索图标名（如 user / dashboard）"
          clearable
          class="toolbar-grid__search"
        >
          <template #prefix>
            <Icon icon="ant-design:search-outlined" />
          </template>
        </n-input>

        <n-select
          v-model:value="prefix"
          :options="prefixOptions"
          placeholder="图标集"
          class="toolbar-grid__prefix"
        />

        <n-select
          v-model:value="pageSize"
          :options="[
            { label: '每页 36', value: 36 },
            { label: '每页 72', value: 72 },
            { label: '每页 144', value: 144 }
          ]"
          class="toolbar-grid__page-size"
        />

        <div class="toolbar-field toolbar-grid__size">
          <span class="toolbar-field__label">尺寸</span>
          <n-slider
            v-model:value="iconSize"
            :min="16"
            :max="64"
            :step="2"
            class="toolbar-field__slider"
          />
          <span class="toolbar-field__value">{{ iconSize }}px</span>
        </div>

        <div class="toolbar-field toolbar-grid__color">
          <n-switch v-model:value="useUnifiedColor" size="medium">
            <template #checked>统一色</template>
            <template #unchecked>原色</template>
          </n-switch>

          <div class="toolbar-field__color">
            <n-color-picker
              v-model:value="unifiedColor"
              :disabled="!useUnifiedColor"
              size="small"
              :show-alpha="false"
              :modes="['hex']"
            />
          </div>
        </div>
      </div>
    </n-card>

    <!-- 结果条 -->
    <div class="icons-page__summary">
      <span>
        匹配 <strong>{{ filteredIcons.length }}</strong> 个图标
        <template v-if="filteredIcons.length > pageSize">
          （第 {{ page }} / {{ Math.ceil(filteredIcons.length / pageSize) }} 页）
        </template>
      </span>
    </div>

    <!-- 图标网格 -->
    <div v-if="pagedIcons.length" class="icon-grid" :style="{ '--icon-size': `${iconSize}px` }">
      <button
        v-for="name in pagedIcons"
        :key="name"
        type="button"
        class="icon-cell"
        :title="`点击复制：${name}`"
        @click="copyName(name)"
      >
        <div class="icon-cell__icon" :style="itemStyle">
          <Icon :icon="name" />
        </div>
        <div class="icon-cell__name">{{ name }}</div>
      </button>
    </div>

    <div v-else class="icons-page__empty">
      <n-empty size="huge" description="没有匹配的图标" />
    </div>

    <!-- 分页 -->
    <div v-if="filteredIcons.length > pageSize" class="icons-page__pagination">
      <n-pagination
        v-model:page="page"
        :page-size="pageSize"
        :item-count="filteredIcons.length"
        :page-slot="7"
        show-quick-jumper
        :prefix="info => `共 ${info.itemCount ?? 0} 条`"
      />
    </div>
  </div>
</template>

<style scoped>
  .icons-page {
    display: flex;
    flex-direction: column;
    gap: 20px;
  }

  /* --- 顶部 --- */
  .icons-page__header {
    display: flex;
    flex-wrap: wrap;
    align-items: flex-start;
    justify-content: space-between;
    gap: 20px;
    padding: 24px 28px;
    border-radius: 12px;
    background: linear-gradient(135deg, rgba(32, 128, 240, 0.08), rgba(32, 128, 240, 0.02));
    border: 1px solid rgba(32, 128, 240, 0.12);
  }

  .icons-page__title {
    display: flex;
    align-items: center;
    gap: 16px;
  }

  .icons-page__title-icon {
    font-size: 40px;
    color: #18a058;
    flex-shrink: 0;
  }

  .icons-page__heading {
    margin: 0;
    font-size: 20px;
    font-weight: 600;
    line-height: 1.2;
  }

  .icons-page__subtitle {
    margin: 4px 0 0;
    font-size: 13px;
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.5));
  }

  .icons-page__stats {
    display: flex;
    flex-wrap: wrap;
    gap: 12px;
  }

  .stat-card {
    min-width: 88px;
    padding: 10px 16px;
    border-radius: 8px;
    background: var(--n-card-color, #fff);
    border: 1px solid var(--n-border-color, #efeff5);
    text-align: center;
    transition: transform 0.2s ease;
  }

  .stat-card:hover {
    transform: translateY(-1px);
  }

  .stat-card--primary {
    background: #18a058;
    border-color: #18a058;
    color: #fff;
  }

  .stat-card__value {
    font-size: 18px;
    font-weight: 600;
    line-height: 1.2;
  }

  .stat-card__label {
    margin-top: 2px;
    font-size: 11px;
    opacity: 0.75;
  }

  /* --- 选择器体验区 --- */
  .icons-page__picker-demo {
    border-radius: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
  }

  .picker-demo {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 16px;
    flex-wrap: wrap;
  }

  .picker-demo__title {
    font-size: 15px;
    font-weight: 600;
    line-height: 1.3;
  }

  .picker-demo__subtitle {
    margin-top: 2px;
    font-size: 12px;
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.5));
  }

  .picker-demo__subtitle code {
    padding: 1px 6px;
    background: var(--n-action-color, #f5f5f5);
    border-radius: 4px;
    font-family: ui-monospace, 'SF Mono', Menlo, Consolas, monospace;
    font-size: 11px;
  }

  .picker-demo__control {
    display: flex;
    align-items: center;
    gap: 10px;
    flex-wrap: wrap;
  }

  /* --- 工具栏 --- */
  .icons-page__toolbar {
    background: var(--n-card-color, #fff);
    border-radius: 12px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
  }

  .toolbar-grid {
    display: grid;
    grid-template-columns:
      minmax(200px, 2fr) minmax(140px, 1fr) minmax(120px, 1fr) minmax(200px, 1.5fr)
      minmax(220px, 1.5fr);
    gap: 12px 16px;
    align-items: center;
  }

  @media (max-width: 1200px) {
    .toolbar-grid {
      grid-template-columns: 1fr 1fr;
    }
  }

  @media (max-width: 640px) {
    .toolbar-grid {
      grid-template-columns: 1fr;
    }
  }

  .toolbar-field {
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .toolbar-field__label {
    font-size: 13px;
    color: var(--n-text-color-2, rgba(0, 0, 0, 0.65));
    flex-shrink: 0;
  }

  .toolbar-field__slider {
    flex: 1;
    min-width: 80px;
  }

  .toolbar-field__value {
    width: 44px;
    font-size: 13px;
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.5));
    text-align: right;
    flex-shrink: 0;
  }

  .toolbar-field__color {
    flex: 1;
    min-width: 120px;
  }

  /* --- 结果条 --- */
  .icons-page__summary {
    font-size: 13px;
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.5));
    padding: 0 4px;
  }

  .icons-page__summary strong {
    color: #18a058;
    font-weight: 600;
    padding: 0 2px;
  }

  /* --- 网格 --- */
  .icon-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(120px, 1fr));
    gap: 12px;
  }

  .icon-cell {
    position: relative;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    gap: 10px;
    padding: 18px 8px 12px;
    background: var(--n-card-color, #fff);
    border: 1px solid var(--n-border-color, #efeff5);
    border-radius: 10px;
    cursor: pointer;
    transition:
      border-color 0.2s ease,
      box-shadow 0.2s ease,
      transform 0.2s ease;
    overflow: hidden;
    font-family: inherit;
    color: inherit;
    text-align: center;
  }

  .icon-cell::before {
    content: '';
    position: absolute;
    inset: 0;
    background: linear-gradient(135deg, rgba(32, 128, 240, 0.06), transparent);
    opacity: 0;
    transition: opacity 0.2s ease;
    pointer-events: none;
  }

  .icon-cell:hover {
    border-color: #18a058;
    box-shadow: 0 6px 16px rgba(32, 128, 240, 0.12);
    transform: translateY(-2px);
  }

  .icon-cell:hover::before {
    opacity: 1;
  }

  .icon-cell:active {
    transform: translateY(0);
    box-shadow: 0 2px 8px rgba(32, 128, 240, 0.08);
  }

  .icon-cell__icon {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    line-height: 1;
    min-height: var(--icon-size);
    position: relative;
    z-index: 1;
  }

  .icon-cell__name {
    font-size: 11px;
    color: var(--n-text-color-3, rgba(0, 0, 0, 0.5));
    width: 100%;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
    line-height: 1.3;
    position: relative;
    z-index: 1;
    font-family: ui-monospace, 'SF Mono', Menlo, Consolas, monospace;
  }

  .icon-cell:hover .icon-cell__name {
    color: #18a058;
  }

  /* --- 空状态 + 分页 --- */
  .icons-page__empty {
    padding: 60px 0;
  }

  .icons-page__pagination {
    display: flex;
    justify-content: center;
    padding: 12px 0 4px;
  }
</style>
