<script setup lang="ts">
  import {
    NAlert,
    NButton,
    NInput,
    NSelect,
    NSpin,
    NSwitch
  } from 'naive-ui'

  import { Icon } from '@/shared/icons'

  import { useCodeGenPage } from './composables/useCodeGenPage'

  defineOptions({ name: 'SysCodeGen' })

  const {
    actionSummary,
    canRun,
    copySelected,
    entityOptions,
    fieldSource,
    fieldSourceOptions,
    fileList,
    form,
    loadingMeta,
    permissions,
    reload,
    result,
    run,
    selectFile,
    selectedFile,
    showAdvanced,
    status,
    statusLabel,
    statusTone,
    submitting,
    tableOptions
  } = useCodeGenPage()
</script>

<template>
  <div class="code-gen-page">
    <section class="code-gen-page__toolbar">
      <div class="code-gen-page__toolbar-top">
        <div class="code-gen-page__brand">
          <h2 class="code-gen-page__title">代码生成</h2>
          <span class="code-gen-page__badge" :class="`code-gen-page__badge--${statusTone}`">
            {{ statusLabel }}
          </span>
          <span class="code-gen-page__hint">{{ status?.message || '正在检查…' }}</span>
        </div>
        <div class="code-gen-page__actions">
          <n-button size="small" secondary :loading="loadingMeta" @click="reload">
            <template #icon>
              <Icon icon="ant-design:reload-outlined" />
            </template>
            刷新
          </n-button>
          <n-button size="small" quaternary @click="showAdvanced = !showAdvanced">
            {{ showAdvanced ? '收起选项' : '更多选项' }}
          </n-button>
        </div>
      </div>

      <n-alert v-if="status && !canRun" type="warning" :bordered="false">
        请确认开发环境且 <code>CodeGen:Enabled=true</code>。生产默认禁止。
      </n-alert>

      <div class="code-gen-page__form-row">
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">模块名 *</span>
          <n-input
            v-model:value="form.module"
            :disabled="!canRun || submitting"
            placeholder="Product"
          />
        </label>
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">中文名 *</span>
          <n-input
            v-model:value="form.cnName"
            :disabled="!canRun || submitting"
            placeholder="产品"
          />
        </label>
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">业务域</span>
          <n-input
            v-model:value="form.area"
            :disabled="!canRun || submitting"
            placeholder="Demo"
          />
        </label>
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">字段来源</span>
          <n-select
            v-model:value="fieldSource"
            :disabled="!canRun || submitting"
            :options="fieldSourceOptions"
          />
        </label>
      </div>

      <div v-if="fieldSource === 'table' || fieldSource === 'entity'" class="code-gen-page__form-row">
        <label v-if="fieldSource === 'table'" class="code-gen-page__field">
          <span class="code-gen-page__label">选择表</span>
          <n-select
            v-model:value="form.fromTable"
            filterable
            clearable
            :disabled="!canRun || submitting"
            :options="tableOptions"
            placeholder="从主库选表"
          />
        </label>
        <label v-if="fieldSource === 'entity'" class="code-gen-page__field">
          <span class="code-gen-page__label">选择实体</span>
          <n-select
            v-model:value="form.fromEntity"
            filterable
            clearable
            :disabled="!canRun || submitting"
            :options="entityOptions"
            placeholder="需已编译 Domain"
          />
        </label>
      </div>

      <div v-if="showAdvanced" class="code-gen-page__advanced">
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">Route Key</span>
          <n-input
            v-model:value="form.routeKey"
            :disabled="!canRun || submitting"
            placeholder="可选"
          />
        </label>
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">实体名</span>
          <n-input
            v-model:value="form.entity"
            :disabled="!canRun || submitting"
            placeholder="Sys{Module}"
          />
        </label>
        <label class="code-gen-page__field">
          <span class="code-gen-page__label">表名</span>
          <n-input
            v-model:value="form.table"
            :disabled="!canRun || submitting"
            placeholder="可选"
          />
        </label>
        <div class="code-gen-page__switches">
          <label class="code-gen-page__switch">
            <n-switch v-model:value="form.backendOnly" size="small" :disabled="form.frontendOnly" />
            仅后端
          </label>
          <label class="code-gen-page__switch">
            <n-switch v-model:value="form.frontendOnly" size="small" :disabled="form.backendOnly" />
            仅前端
          </label>
        </div>
      </div>

      <div class="code-gen-page__toolbar-top">
        <div class="code-gen-page__switches">
          <label class="code-gen-page__switch">
            <n-switch v-model:value="form.force" size="small" :disabled="!canRun || submitting" />
            覆盖已有
          </label>
          <label class="code-gen-page__switch">
            <n-switch v-model:value="form.noMenu" size="small" :disabled="!canRun || submitting" />
            跳过菜单种子
          </label>
        </div>
        <div class="code-gen-page__actions">
          <n-button
            v-permission="permissions.preview"
            type="primary"
            secondary
            :disabled="!canRun"
            :loading="submitting"
            @click="run('preview')"
          >
            <template #icon>
              <Icon icon="ant-design:eye-outlined" />
            </template>
            预览代码
          </n-button>
          <n-button
            v-permission="permissions.generate"
            type="primary"
            :disabled="!canRun"
            :loading="submitting"
            @click="run('generate')"
          >
            <template #icon>
              <Icon icon="ant-design:thunderbolt-outlined" />
            </template>
            生成落盘
          </n-button>
        </div>
      </div>

      <div v-if="result" class="code-gen-page__meta-bar">
        <span class="code-gen-page__chip">
          {{ result.dryRun ? '预览' : '已生成' }} · {{ result.area }}/{{ result.module }}
        </span>
        <span class="code-gen-page__chip">{{ result.routePrefix }}</span>
        <span class="code-gen-page__chip">{{ result.fieldSource }}</span>
        <span class="code-gen-page__chip code-gen-page__chip--muted">
          {{ result.fields.length }} 字段
        </span>
        <span class="code-gen-page__chip code-gen-page__chip--muted">
          可预览 {{ actionSummary.previewable }} / 共 {{ actionSummary.total }}
        </span>
        <span
          v-for="field in result.fields"
          :key="field.name"
          class="code-gen-page__chip code-gen-page__chip--muted"
          :title="`${field.label} · ${field.csType}`"
        >
          {{ field.name }}
        </span>
      </div>
    </section>

    <n-spin :show="loadingMeta || submitting">
      <div class="code-gen-page__workspace">
        <aside class="code-gen-page__files">
          <div class="code-gen-page__panel-head">
            <div>
              <h3 class="code-gen-page__panel-title">生成文件</h3>
              <div class="code-gen-page__panel-sub">
                {{
                  result
                    ? `CREATE ${actionSummary.create} · MERGE ${actionSummary.merge} · SKIP ${actionSummary.skip}`
                    : '预览后在此列出全部文件'
                }}
              </div>
            </div>
          </div>

          <ul v-if="fileList.length" class="code-gen-page__file-list">
            <li v-for="file in fileList" :key="file.path">
              <button
                type="button"
                class="code-gen-page__file"
                :class="{
                  'code-gen-page__file--active': selectedFile?.path === file.path,
                  'code-gen-page__file--skip': file.action === 'SKIP' || !file.content
                }"
                @click="selectFile(file.path)"
              >
                <span
                  class="code-gen-page__file-action"
                  :class="`code-gen-page__file-action--${file.action}`"
                >
                  {{ file.action }}
                </span>
                <span>
                  <div class="code-gen-page__file-path">{{ file.path }}</div>
                  <div v-if="file.detail" class="code-gen-page__file-detail">{{ file.detail }}</div>
                </span>
              </button>
            </li>
          </ul>

          <div v-else class="code-gen-page__empty">
            <div class="code-gen-page__empty-icon">
              <Icon icon="ant-design:file-text-outlined" />
            </div>
            <h4 class="code-gen-page__empty-title">还没有文件列表</h4>
            <p class="code-gen-page__empty-desc">填写参数后点「预览代码」，可逐个查看即将生成的源码。</p>
          </div>
        </aside>

        <section class="code-gen-page__viewer">
          <div class="code-gen-page__panel-head">
            <div style="min-width: 0">
              <h3 class="code-gen-page__panel-title">代码预览</h3>
              <div v-if="selectedFile" class="code-gen-page__viewer-path">
                {{ selectedFile.path }}
                <template v-if="selectedFile.language"> · {{ selectedFile.language }}</template>
              </div>
              <div v-else class="code-gen-page__panel-sub">选择左侧文件查看完整内容</div>
            </div>
            <n-button
              size="small"
              secondary
              :disabled="!selectedFile?.content"
              @click="copySelected"
            >
              <template #icon>
                <Icon icon="ant-design:copy-outlined" />
              </template>
              复制
            </n-button>
          </div>

          <div v-if="selectedFile?.content" class="code-gen-page__code-wrap">
            <pre class="code-gen-page__code">{{ selectedFile.content }}</pre>
          </div>
          <div v-else-if="selectedFile" class="code-gen-page__empty">
            <div class="code-gen-page__empty-icon">
              <Icon icon="ant-design:stop-outlined" />
            </div>
            <h4 class="code-gen-page__empty-title">跳过，无内容可预览</h4>
            <p class="code-gen-page__empty-desc">
              {{ selectedFile.detail || '文件已存在且未勾选覆盖，不会写入。' }}
            </p>
          </div>
          <div v-else class="code-gen-page__empty">
            <div class="code-gen-page__empty-icon">
              <Icon icon="ant-design:code-outlined" />
            </div>
            <h4 class="code-gen-page__empty-title">完整源码预览</h4>
            <p class="code-gen-page__empty-desc">
              预览会返回每个将 CREATE / OVERWRITE / MERGE 的文件全文，确认无误后再生成落盘。
            </p>
          </div>
        </section>
      </div>
    </n-spin>
  </div>
</template>

<style scoped lang="scss" src="./styles/index.scss"></style>
