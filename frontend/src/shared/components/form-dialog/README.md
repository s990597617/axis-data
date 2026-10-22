# FormDialog

新增/编辑等表单场景的通用容器：统一提交状态、标题区与底部操作。

支持两种呈现：
- `presentation="modal"`（默认）：居中卡片弹窗
- `presentation="drawer"`：右侧（或左侧）侧滑抽屉，适合分步向导与长表单

```vue
<script setup lang="ts">
  import { ref } from 'vue'
  import { AxisForm } from '@/shared/components/axis-form'
  import { FormDialog } from '@/shared/components/form-dialog'

  const visible = ref(false)
  const saving = ref(false)
</script>

<template>
  <!-- 分区卡片底：layout="sectioned" -->
  <FormDialog
    v-model="visible"
    layout="sectioned"
    presentation="drawer"
    title="新建用户"
    subtitle="填写用户的基本信息"
    icon="ant-design:user-add-outlined"
    :width="560"
    :confirm-loading="saving"
    @confirm="handleSubmit"
  >
    <AxisForm :model="form" :sections="sections" />
  </FormDialog>
</template>
```

- `layout="sectioned"`：主体 12px 间距 + 浅色底，配合 `AxisForm` 的 `sections` 分区卡片。
- `presentation="drawer"` 时 `draggable` 无效；主体默认不限制 maxBodyHeight，可纵向铺满滚动。
- 确定事件不会自动关闭对话框，业务提交成功后再改 `v-model`。