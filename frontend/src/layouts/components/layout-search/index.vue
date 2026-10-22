<script setup lang="ts">
import { useLayoutSearch } from '@/layouts/composables/use-layout-search'
import { Icon } from '@/shared/icons'

const visible = defineModel<boolean>('visible', { default: false })

const {
  activeIndex,
  clearKeyword,
  filteredItems,
  handleKeydown,
  inputRef,
  keyword,
  listRef,
  navigate,
} = useLayoutSearch(visible)
</script>

<template>
  <Teleport to="body">
    <Transition name="search-fade">
      <div v-if="visible" class="search-mask" @click.self="visible = false">
        <div class="search-dialog">
          <div class="search-header">
            <Icon icon="ant-design:search-outlined" class="search-icon" />
            <input
              ref="inputRef"
              v-model="keyword"
              class="search-input"
              placeholder="输入内容，支持中文、拼音、路径搜索"
              @keydown="handleKeydown"
            />
            <button v-if="keyword" type="button" class="header-btn" title="清空" @click="clearKeyword">
              <Icon icon="ant-design:close-circle-outlined" />
            </button>
            <button type="button" class="header-btn" title="关闭" @click="visible = false">
              <Icon icon="ant-design:close-outlined" />
            </button>
          </div>

          <div ref="listRef" class="search-body">
            <template v-if="keyword && filteredItems.length">
              <div
                v-for="(item, index) in filteredItems"
                :key="item.path"
                :class="['search-item', { active: index === activeIndex }]"
                @mouseenter="activeIndex = index"
                @click="navigate(item)"
              >
                <Icon :icon="item.icon || 'ant-design:file-outlined'" class="item-icon" />
                <div class="item-content">
                  <div class="item-title">{{ item.title }}</div>
                  <div class="item-path">{{ item.path }}</div>
                </div>
                <div v-if="index === activeIndex" class="item-actions">
                  <button type="button" class="action-btn" title="选择" @click.stop="navigate(item)">
                    <Icon icon="ant-design:link-outlined" />
                  </button>
                  <button
                    type="button"
                    class="action-btn"
                    title="新窗口打开"
                    @click.stop="navigate(item, true)"
                  >
                    <Icon icon="ant-design:export-outlined" />
                  </button>
                </div>
              </div>
            </template>

            <div v-else class="search-empty">
              <Icon icon="ant-design:search-outlined" class="empty-icon" />
              <p>{{ keyword ? '没有找到相关结果' : '输入搜索关键字开始搜索' }}</p>
            </div>
          </div>

          <div class="search-footer">
            <span class="footer-hint"><kbd>Enter</kbd> 选择</span>
            <span class="footer-hint"><kbd class="kbd-text">Ctrl</kbd><kbd>Enter</kbd> 新窗口</span>
            <span class="footer-hint"><kbd>↑</kbd><kbd>↓</kbd> 切换</span>
            <span class="footer-hint"><kbd class="kbd-text">Esc</kbd> 退出</span>
          </div>
        </div>
      </div>
    </Transition>
  </Teleport>
</template>

<style scoped lang="scss">
.search-mask {
  position: fixed;
  inset: 0;
  z-index: 1000;
  display: flex;
  align-items: center;
  justify-content: center;
  background: rgba(0, 0, 0, 0.25);
}

.search-dialog {
  width: min(580px, calc(100vw - 32px));
  max-height: 480px;
  background: var(--axis-card-color, #fff);
  border-radius: 12px;
  box-shadow:
    0 12px 40px rgba(0, 0, 0, 0.12),
    0 0 0 1px rgba(0, 0, 0, 0.04);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  animation: search-slide-in 0.2s ease;
}

@keyframes search-slide-in {
  from {
    opacity: 0;
    transform: translateY(-10px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.search-header {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 16px 18px;
  border-bottom: 1px solid var(--axis-border-color, #f2f3f5);
}

.search-icon,
.header-btn {
  flex-shrink: 0;
  color: var(--axis-text-color-3, #86909c);
}

.search-input {
  flex: 1;
  min-width: 0;
  border: none;
  outline: none;
  font-size: 15px;
  color: var(--axis-text-color-1, #1d2129);
  background: transparent;

  &::placeholder {
    color: var(--axis-text-color-3, #c9cdd4);
  }
}

.header-btn,
.action-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  border: none;
  border-radius: 6px;
  background: transparent;
  cursor: pointer;
  transition: all 0.15s;
}

.header-btn {
  width: 28px;
  height: 28px;
  font-size: 16px;

  &:hover {
    color: var(--axis-text-color-1, #4e5969);
  }
}

.search-body {
  flex: 1;
  overflow-y: auto;
  padding: 8px 10px;
  min-height: 180px;
}

.search-item {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 8px;
  border: 1px solid transparent;
  cursor: pointer;
  transition: all 0.15s;
  margin-bottom: 4px;

  &:hover {
    background: var(--axis-action-color, #fafafa);
  }

  &.active {
    background: var(--axis-action-color, #f7f8fa);
    border-color: var(--axis-primary-color, #40a9ff);
    border-style: dashed;
  }
}

.item-icon {
  flex-shrink: 0;
  font-size: 18px;
  color: var(--axis-primary-color, #165dff);
}

.item-content {
  flex: 1;
  min-width: 0;
}

.item-title {
  font-size: 14px;
  font-weight: 500;
  color: var(--axis-text-color-1, #1d2129);
}

.item-path {
  font-size: 12px;
  color: var(--axis-text-color-3, #a0a4aa);
  margin-top: 2px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  font-family: 'Menlo', 'Consolas', 'Courier New', monospace;
}

.item-actions {
  display: flex;
  align-items: center;
  gap: 4px;
  flex-shrink: 0;
}

.action-btn {
  width: 28px;
  height: 28px;
  color: var(--axis-text-color-3, #86909c);

  &:hover {
    background: rgba(var(--axis-primary-color-rgb, 22, 93, 255), 0.08);
    color: var(--axis-primary-color, #165dff);
  }
}

.search-empty {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  height: 180px;
  color: var(--axis-text-color-3, #c9cdd4);

  .empty-icon {
    font-size: 48px;
  }

  p {
    margin-top: 12px;
    font-size: 13px;
    color: var(--axis-text-color-3, #86909c);
  }
}

.search-footer {
  display: flex;
  align-items: center;
  gap: 18px;
  padding: 10px 18px;
  border-top: 1px solid var(--axis-border-color, #f2f3f5);
  flex-wrap: wrap;
}

.footer-hint {
  display: flex;
  align-items: center;
  gap: 5px;
  font-size: 12px;
  color: var(--axis-text-color-3, #86909c);

  kbd {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 20px;
    height: 20px;
    padding: 0 5px;
    font-size: 11px;
    font-family: inherit;
    border-radius: 4px;
    background: var(--axis-action-color, #f7f8fa);
    color: var(--axis-text-color-2, #4e5969);
  }

  .kbd-text {
    font-size: 10px;
    padding: 0 6px;
  }
}

.search-fade-enter-active,
.search-fade-leave-active {
  transition: opacity 0.2s ease;
}

.search-fade-enter-from,
.search-fade-leave-to {
  opacity: 0;
}
</style>
