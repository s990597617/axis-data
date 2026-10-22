# FormDialog

面向新增、编辑等表单场景的通用弹窗，内置提交状态与标准底部操作区。

```vue
<script setup lang="ts">
  import { ref } from 'vue'
  import { AxisForm } from '@/shared/components/axis-form'
  import { FormDialog } from '@/shared/components/form-dialog'

  const visible = ref(false)
  const saving = ref(false)
</script>

<template>
  <!-- 分区卡片表单：layout="sectioned" -->
  <FormDialog
    v-model="visible"
    layout="sectioned"
    title="新增用户"
    subtitle="填写用户的基本信息"
    icon="ant-design:user-add-outlined"
    :confirm-loading="saving"
    @confirm="handleSubmit"
  >
    <AxisForm :model="form" :sections="sections" />
  </FormDialog>
</template>
```

- `layout="sectioned"`：内容区 12px 间距 + 浅色底，配合 `AxisForm` 的 `sections` 分区卡片。
- 确认事件不会自动关闭弹窗，业务提交成功后再改 `v-model`。
