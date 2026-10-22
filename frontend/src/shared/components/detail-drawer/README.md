# DetailDrawer

通用详情抽屉，只负责标题、状态、加载态、信息字段与分组内容，不包含业务统计、记录 Tab 或表格逻辑。

```vue
<script setup lang="ts">
  import { ref } from 'vue'
  import { DetailDrawer, type DetailField } from '@/shared/components/detail-drawer'

  const visible = ref(false)
  const fields: DetailField[] = [
    { key: 'name', label: '名称', value: '示例名称' },
    { key: 'status', label: '状态', value: '启用', type: 'tag', tagType: 'success' },
    { key: 'remark', label: '备注', value: '补充说明', fullRow: true }
  ]
</script>

<template>
  <DetailDrawer v-model:show="visible" title="详情" :fields="fields" />
</template>
```

需要完全自定义内容时可直接使用默认插槽；字段需要特殊展示时使用单一的 `field` 作用域插槽，通过 `field.key` 区分。
