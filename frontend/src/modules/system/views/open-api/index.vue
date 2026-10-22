<script setup lang="ts">
  import { NButton, NInput, NInputGroup } from 'naive-ui'

  import { AxisCrudPage } from '@/shared/components/axis-crud'
  import { Icon } from '@/shared/icons'

  import OpenApiUsageDialog from './components/OpenApiUsageDialog.vue'
  import { useOpenApiPage } from './composables/useOpenApiPage'

  defineOptions({ name: 'SysOpenApi' })

  const { resource, generateSecret, usageVisible } = useOpenApiPage()
</script>

<template>
  <div class="open-api-page">
    <AxisCrudPage :resource="resource">
      <template #toolbar-extra>
        <NButton size="small" secondary @click="usageVisible = true">
          <template #icon>
            <Icon icon="ant-design:question-circle-outlined" />
          </template>
          用法说明
        </NButton>
      </template>

      <template #field-accessSecret="{ value, setValue, disabled }">
        <NInputGroup>
          <NInput
            :value="typeof value === 'string' ? value : ''"
            :disabled="disabled"
            maxlength="128"
            placeholder="可留空自动生成，或点击生成"
            @update:value="setValue"
          />
          <NButton :disabled="disabled" secondary @click="setValue(generateSecret())">
            生成密钥
          </NButton>
        </NInputGroup>
      </template>
    </AxisCrudPage>

    <OpenApiUsageDialog v-model="usageVisible" />
  </div>
</template>

<style scoped lang="scss">
  @use './styles/index.scss';
</style>
