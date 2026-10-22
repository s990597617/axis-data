# AxisForm

面向新增、编辑场景的配置式表单。组件负责字段渲染、分组、响应式布局和校验，弹窗开关、接口请求与提交转换仍由业务页面负责。

```vue
<script setup lang="ts">
  import { reactive, ref } from 'vue'
  import {
    AxisForm,
    type AxisFormExposed,
    type AxisFormSection
  } from '@/shared/components/axis-form'

  const formRef = ref<AxisFormExposed | null>(null)
  const form = reactive({ name: '', status: 1, remark: '' })

  const sections: AxisFormSection[] = [
    {
      key: 'basic',
      title: '基本信息',
      icon: 'ant-design:profile-outlined',
      schemas: [
        { field: 'name', label: '名称', required: true },
        {
          field: 'status',
          label: '状态',
          component: 'select',
          options: [
            { label: '启用', value: 1 },
            { label: '禁用', value: 0 }
          ]
        },
        {
          field: 'remark',
          label: '备注',
          component: 'textarea',
          fullWidth: true
        }
      ]
    }
  ]

  async function submit() {
    await formRef.value?.validate()
    // 校验通过后调用业务接口
  }
</script>

<template>
  <AxisForm ref="formRef" :model="form" :sections="sections" />
</template>
```

## 字段类型

内置 `input`、`textarea`、`password`、`inputNumber`、`select`、`treeSelect`、`date`、`dateRange`、`radio`、`checkbox` 和 `switch`。

复杂业务字段使用专属插槽，插槽名为 `field-字段名`：

```vue
<AxisForm :model="form" :schemas="schemas">
  <template #field-avatar="{ value, setValue, disabled }">
    <ImageUpload :value="value" :disabled="disabled" @update:value="setValue" />
  </template>
</AxisForm>
```

`#before` 默认在分区上方。需要左预览右表单时设 `before-placement="aside"`。

`componentProps`、`options`、`required`、`ifShow`、`disabled` 和 `formItemProps` 均支持接收字段上下文的函数，可根据当前模型动态计算。

组件通过 `ref` 暴露 `validate`、`restoreValidation`、`getFormInstance`、`getValue` 和 `setValue`。
