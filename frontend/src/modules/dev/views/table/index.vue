<script setup lang="ts">
  import { computed } from 'vue'
  import { RouterView, useRoute, useRouter } from 'vue-router'

  import { Icon } from '@/shared/icons'

  defineOptions({ name: 'TableDemo' })

  const route = useRoute()
  const router = useRouter()

  const tabs = [
    {
      label: '基础用法',
      value: '/table-demo/basic',
      icon: 'ant-design:table-outlined'
    },
    {
      label: '远程请求',
      value: '/table-demo/remote',
      icon: 'ant-design:cloud-server-outlined'
    },
    {
      label: '进阶组合',
      value: '/table-demo/advanced',
      icon: 'ant-design:control-outlined'
    }
  ]

  const activeTab = computed(() => route.path)

  function handleTabChange(path: string) {
    void router.push(path)
  }
</script>

<template>
  <div class="table-demo">
    <div class="table-demo__header">
      <div class="table-demo__title">
        <Icon icon="ant-design:table-outlined" class="table-demo__title-icon" />
        <div>
          <h2>AxisTable 演示</h2>
          <p>集中展示静态数据、远程请求、搜索字典、行选择、操作列、缓存和导出等表格能力</p>
        </div>
      </div>

      <n-tabs
        :value="activeTab"
        type="segment"
        animated
        class="table-demo__tabs"
        @update:value="value => handleTabChange(String(value))"
      >
        <n-tab v-for="tab in tabs" :key="tab.value" :name="tab.value">
          <span class="table-demo__tab">
            <Icon :icon="tab.icon" />
            {{ tab.label }}
          </span>
        </n-tab>
      </n-tabs>
    </div>

    <RouterView />
  </div>
</template>

<style scoped lang="scss">
  .table-demo {
    display: flex;
    flex-direction: column;
    gap: 16px;
    padding: 16px;

    &__header {
      display: flex;
      flex-wrap: wrap;
      align-items: flex-end;
      justify-content: space-between;
      gap: 16px;
    }

    &__title {
      display: flex;
      align-items: center;
      gap: 12px;
      min-width: 280px;

      h2 {
        margin: 0;
        color: var(--axis-text-color-1);
        font-size: 22px;
        font-weight: 650;
      }

      p {
        margin: 6px 0 0;
        color: var(--axis-text-color-3);
        font-size: 13px;
      }
    }

    &__title-icon {
      width: 38px;
      height: 38px;
      padding: 8px;
      border-radius: 8px;
      background: var(--axis-action-color);
      color: var(--axis-primary-color);
      font-size: 22px;
    }

    &__tabs {
      width: 360px;
      max-width: 100%;
    }

    &__tab {
      display: inline-flex;
      align-items: center;
      gap: 6px;
    }
  }
</style>
