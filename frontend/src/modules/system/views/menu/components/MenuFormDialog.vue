<script setup lang="ts">
  import { computed, nextTick, onMounted, reactive, ref } from 'vue'
  import { NSelect, NTag } from 'naive-ui'

  import type { MenuDto, PermissionCatalogModule, SaveMenuInput } from '@/modules/system/api/menu'
  import { createMenu, getMenuPermissionCatalog, updateMenu } from '@/modules/system/api/menu'
  import {
    AxisForm,
    type AxisFormExposed,
    type AxisFormSchema,
    type AxisFormSection
  } from '@/shared/components/axis-form'
  import AxisIconPicker from '@/shared/components/axis-icon-picker/index.vue'
  import { statusOptions } from '@/shared/components/axis-crud'
  import { FormDialog } from '@/shared/components/form-dialog'
  import { tryRequest } from '@/shared/http'
  import { message } from '@/shared/utils/message'

  import { defaultChildType, MenuType, menuTypeMeta, menuTypeOptions } from '../utils/menu-type'
  import {
    buildGroupedPermissionOptions,
    buildModuleActionOptions,
    extractModuleKey,
    findParentMenu,
    listSiblingButtons,
    suggestModuleKey
  } from '../utils/permission-catalog'
  import { collectDescendantIds, type MenuTreeOption, toParentOptions } from '../utils/tree'

  defineOptions({ name: 'MenuFormDialog' })

  const props = defineProps<{
    rows: MenuDto[]
    menuTree: MenuTreeOption[]
  }>()

  const emit = defineEmits<{
    saved: [preferredId?: string]
  }>()

  /* ============ 常量 ============ */

  const formDefaults: SaveMenuInput = {
    parentId: '0',
    type: MenuType.Menu,
    title: '',
    name: '',
    path: '',
    component: '',
    permission: '',
    icon: '',
    redirect: '',
    isFrame: false,
    isKeepAlive: true,
    isHide: false,
    orderNo: 100,
    status: 1,
    remark: ''
  }

  /* ============ 状态 & 引用 ============ */

  const formRef = ref<AxisFormExposed | null>(null)
  const visible = ref(false)
  const submitting = ref(false)
  const editing = ref(false)
  const lockParentField = ref(false)
  const lockTypeField = ref(false)
  const editId = ref<string>()
  const stayOnSelectedId = ref<string>()
  const titleAutoFilled = ref(false)
  const compactMode = ref(true)
  const catalog = ref<PermissionCatalogModule[]>([])
  const form = reactive<Record<string, unknown>>({ ...formDefaults })

  /* ============ 视图模型 ============ */

  const isButtonType = computed(() => Number(form.type) === MenuType.Button)

  const currentTypeMeta = computed(() => {
    const type = Number(form.type) as keyof typeof menuTypeMeta
    return menuTypeMeta[type] ?? menuTypeMeta[MenuType.Menu]
  })

  const formTitle = computed(
    () => `${editing.value ? '编辑' : '新增'}${currentTypeMeta.value.label}`
  )

  const formSubtitle = computed(() => {
    const parentTitle = parentMenu.value?.title?.trim()
    if (isButtonType.value) {
      return parentTitle
        ? `挂载于「${parentTitle}」· 选择动作后自动填充显示名称`
        : '从权限目录选择动作，显示名称可再改'
    }
    if (parentTitle) return `上级节点：${parentTitle}`
    return editing.value ? '调整路由、展示与状态配置' : '配置路由、图标与展示信息'
  })

  const dialogIcon = computed(() => currentTypeMeta.value.icon)

  const forbiddenParentIds = computed(() => {
    if (!editing.value || !editId.value) return new Set<string>()
    return collectDescendantIds(props.rows, editId.value)
  })

  const parentOptions = computed(() => [
    { key: '0', label: '顶级节点' },
    ...toParentOptions(props.menuTree, forbiddenParentIds.value)
  ])

  const parentMenu = computed(() => findParentMenu(props.rows, String(form.parentId ?? '0')))

  const siblingButtons = computed(() =>
    listSiblingButtons(props.rows, String(form.parentId ?? '0'), editId.value)
  )

  const suggestedModule = computed(() =>
    suggestModuleKey(parentMenu.value, siblingButtons.value, catalog.value)
  )

  const suggestedModuleName = computed(
    () => catalog.value.find(item => item.module === suggestedModule.value)?.moduleName
  )

  const useCompactPicker = computed(() => compactMode.value && Boolean(suggestedModule.value))

  const usedPermissionCodes = computed(() => {
    const parentId = String(form.parentId ?? '0')
    return new Set(
      props.rows
        .filter(
          row =>
            row.type === MenuType.Button &&
            row.parentId === parentId &&
            row.id !== editId.value &&
            row.permission
        )
        .map(row => row.permission as string)
    )
  })

  const permissionSelectOptions = computed(() =>
    useCompactPicker.value
      ? buildModuleActionOptions(catalog.value, usedPermissionCodes.value, suggestedModule.value)
      : buildGroupedPermissionOptions(catalog.value, usedPermissionCodes.value)
  )

  const actionNameByCode = computed(() => {
    const map = new Map<string, string>()
    catalog.value.forEach(module => {
      module.items.forEach(item => map.set(item.code, item.actionName))
    })
    return map
  })

  const selectedPermissionCode = computed(() =>
    typeof form.permission === 'string' ? form.permission : ''
  )

  const permissionPlaceholder = computed(() =>
    useCompactPicker.value ? '选择权限动作' : '搜索或按模块选择权限动作'
  )

  const permissionModeHint = computed(() =>
    useCompactPicker.value
      ? `推荐模块：${suggestedModuleName.value || suggestedModule.value || '当前菜单'}`
      : '已展开全部模块，可搜索权限码'
  )

  const headSchemas = computed<AxisFormSchema[]>(() => [
    {
      field: 'parentId',
      label: '上级节点',
      component: 'treeSelect',
      options: parentOptions.value,
      disabled: lockParentField.value
    },
    {
      field: 'type',
      label: '节点类型',
      component: 'radio',
      options: menuTypeOptions,
      required: true,
      disabled: lockTypeField.value
    }
  ])

  const titleSchema = computed<AxisFormSchema>(() => ({
    field: 'title',
    label: '显示名称',
    required: true,
    fullWidth: isButtonType.value,
    componentProps: {
      maxlength: 64,
      showCount: true,
      placeholder: isButtonType.value ? '选择权限后自动填充，可修改' : '请输入显示名称'
    }
  }))

  const permissionSchema = computed<AxisFormSchema>(() => ({
    field: 'permission',
    label: '权限标识',
    component: 'custom',
    required: true,
    requiredMessage: '请选择权限标识',
    fullWidth: true,
    formItemProps: { class: 'menu-form-item--permission', showRequireMark: true }
  }))

  const menuRouteSchemas = computed<AxisFormSchema[]>(() => [
    {
      field: 'name',
      label: '路由名称',
      required: true,
      componentProps: { maxlength: 64, placeholder: '请输入路由名称' }
    },
    {
      field: 'path',
      label: '路由路径',
      required: true,
      componentProps: { maxlength: 128, placeholder: '请输入路由路径' }
    },
    {
      field: 'component',
      label: '组件路径',
      required: true,
      ifShow: () => Number(form.type) === MenuType.Menu,
      componentProps: { maxlength: 128, placeholder: '请输入组件路径' }
    },
    {
      field: 'icon',
      label: '菜单图标',
      component: 'custom',
      required: true,
      requiredMessage: '请选择菜单图标'
    },
    {
      field: 'redirect',
      label: '重定向地址',
      componentProps: { maxlength: 256, placeholder: '可选，目录常用作默认跳转' }
    },
    {
      field: 'isFrame',
      label: '外链',
      component: 'switch',
      ifShow: () => Number(form.type) === MenuType.Menu
    },
    {
      field: 'isKeepAlive',
      label: '页面缓存',
      component: 'switch',
      ifShow: () => Number(form.type) === MenuType.Menu
    },
    {
      field: 'isHide',
      label: '隐藏菜单',
      component: 'switch'
    }
  ])

  const statusSchemas = computed<AxisFormSchema[]>(() => [
    {
      field: 'status',
      label: '状态',
      component: 'radio',
      options: statusOptions,
      required: true
    },
    {
      field: 'orderNo',
      label: '排序',
      component: 'inputNumber',
      required: true,
      componentProps: { min: 0, max: 9999, placeholder: '请输入排序' }
    },
    {
      field: 'remark',
      label: '备注',
      component: 'textarea',
      fullWidth: true,
      componentProps: {
        maxlength: 256,
        showCount: true,
        placeholder: '可选，补充说明用途或注意事项'
      }
    }
  ])

  const formSections = computed<AxisFormSection[]>(() => {
    if (isButtonType.value) {
      return [
        {
          key: 'belong',
          title: '节点归属',
          icon: 'ant-design:apartment-outlined',
          schemas: headSchemas.value
        },
        {
          key: 'permission',
          title: '权限配置',
          icon: 'ant-design:safety-certificate-outlined',
          schemas: [permissionSchema.value, titleSchema.value]
        },
        {
          key: 'status',
          title: '状态设置',
          icon: 'ant-design:setting-outlined',
          schemas: statusSchemas.value
        }
      ]
    }

    return [
      {
        key: 'belong',
        title: '节点归属',
        icon: 'ant-design:apartment-outlined',
        schemas: [...headSchemas.value, titleSchema.value]
      },
      {
        key: 'route',
        title: '路由与展示',
        icon: 'ant-design:compass-outlined',
        schemas: menuRouteSchemas.value
      },
      {
        key: 'status',
        title: '状态设置',
        icon: 'ant-design:setting-outlined',
        schemas: statusSchemas.value
      }
    ]
  })

  /* ============ 交互操作 ============ */

  function normalizeCatalog(raw: unknown): PermissionCatalogModule[] {
    if (!Array.isArray(raw)) return []
    return raw.map((module: Record<string, unknown>) => {
      const items = (module.items ?? module.Items ?? []) as Array<Record<string, unknown>>
      return {
        module: String(module.module ?? module.Module ?? ''),
        moduleName: String(module.moduleName ?? module.ModuleName ?? ''),
        items: items.map(item => ({
          code: String(item.code ?? item.Code ?? ''),
          actionName: String(item.actionName ?? item.ActionName ?? '')
        }))
      }
    })
  }

  async function loadCatalog() {
    if (catalog.value.length) return
    const [data, error] = await tryRequest(getMenuPermissionCatalog())
    if (error) return
    catalog.value = normalizeCatalog(data)
  }

  function syncPickerModeFromPermission() {
    const code = selectedPermissionCode.value
    if (!code || !suggestedModule.value) return
    if (extractModuleKey(code) !== suggestedModule.value) {
      compactMode.value = false
    }
  }

  function resetForm() {
    Object.keys(form).forEach(key => delete form[key])
    Object.assign(form, formDefaults)
    titleAutoFilled.value = false
    compactMode.value = true
  }

  function applyPermissionTitle(code: string) {
    if (!code) return
    const actionName = actionNameByCode.value.get(code)
    if (!actionName) return

    const currentTitle = String(form.title ?? '').trim()
    if (!currentTitle || titleAutoFilled.value) {
      form.title = actionName
      titleAutoFilled.value = true
    }
  }

  function commitPermission(value: unknown, setValue?: (next: unknown) => void) {
    const code = value == null || value === '' ? '' : String(value)
    form.permission = code
    setValue?.(code)
    applyPermissionTitle(code)
  }

  function handleFormChange(event: { field: string; value: unknown }) {
    if (event.field === 'title') {
      titleAutoFilled.value = false
      return
    }
    if (event.field === 'parentId' && isButtonType.value) {
      compactMode.value = Boolean(
        suggestModuleKey(
          findParentMenu(props.rows, String(form.parentId ?? '0')),
          listSiblingButtons(props.rows, String(form.parentId ?? '0'), editId.value),
          catalog.value
        )
      )
      if (form.permission) {
        const code = String(form.permission)
        const stillAvailable = permissionSelectOptions.value.some(option => {
          if (option.type === 'group') {
            return option.children?.some(child => child.value === code)
          }
          return option.value === code
        })
        if (!stillAvailable) {
          form.permission = ''
        } else {
          syncPickerModeFromPermission()
        }
      }
    }
  }

  async function showForm(
    row?: MenuDto,
    defaults?: Partial<SaveMenuInput>,
    locks: { parent?: boolean; type?: boolean } = {},
    staySelectedId?: string
  ) {
    await loadCatalog()
    resetForm()
    editing.value = Boolean(row)
    lockParentField.value = !row && Boolean(locks.parent)
    lockTypeField.value = !row && Boolean(locks.type)
    editId.value = row?.id
    stayOnSelectedId.value = staySelectedId

    if (row) {
      Object.keys(formDefaults).forEach(key => {
        form[key] = row[key as keyof MenuDto] ?? formDefaults[key as keyof SaveMenuInput]
      })
    } else {
      Object.assign(form, defaults)
    }

    compactMode.value = Boolean(
      suggestModuleKey(
        findParentMenu(props.rows, String(form.parentId ?? '0')),
        listSiblingButtons(props.rows, String(form.parentId ?? '0'), editId.value),
        catalog.value
      )
    )
    syncPickerModeFromPermission()

    visible.value = true
    await nextTick()
    formRef.value?.restoreValidation()
  }

  function openCreateRoot() {
    void showForm(undefined, { parentId: '0', type: MenuType.Directory })
  }

  function openCreateChild(parent: MenuDto) {
    void showForm(
      undefined,
      { parentId: parent.id, type: defaultChildType(parent.type) },
      { parent: true }
    )
  }

  function openCreateButton(parent: MenuDto) {
    void showForm(
      undefined,
      { parentId: parent.id, type: MenuType.Button },
      { parent: true, type: true },
      parent.id
    )
  }

  function openEdit(row: MenuDto) {
    void showForm(row, undefined, {}, row.type === MenuType.Button ? row.parentId : row.id)
  }

  function buildSubmitData(): SaveMenuInput {
    return {
      parentId: String(form.parentId ?? '0'),
      type: Number(form.type ?? MenuType.Menu),
      title: String(form.title ?? '').trim(),
      name: String(form.name ?? '').trim(),
      path: String(form.path ?? '').trim(),
      component: String(form.component ?? '').trim(),
      permission: String(form.permission ?? '').trim(),
      icon: String(form.icon ?? '').trim(),
      redirect: String(form.redirect ?? '').trim(),
      isFrame: Boolean(form.isFrame),
      isKeepAlive: Boolean(form.isKeepAlive),
      isHide: Boolean(form.isHide),
      orderNo: Number(form.orderNo ?? 100),
      status: Number(form.status ?? 1),
      remark: String(form.remark ?? '').trim()
    }
  }

  async function handleSubmit() {
    try {
      await formRef.value?.validate()
    } catch {
      return
    }

    submitting.value = true
    const data = buildSubmitData()
    const operation =
      editing.value && editId.value ? updateMenu(editId.value, data) : createMenu(data)
    const [, error] = await tryRequest(operation)
    submitting.value = false
    if (error) return

    message.success(editing.value ? '更新成功' : '新增成功')
    const preferredId =
      data.type === MenuType.Button
        ? stayOnSelectedId.value
        : editId.value || stayOnSelectedId.value
    visible.value = false
    emit('saved', preferredId)
  }

  onMounted(() => {
    void loadCatalog()
  })

  defineExpose({
    openCreateRoot,
    openCreateChild,
    openCreateButton,
    openEdit
  })
</script>

<template>
  <FormDialog
    v-model="visible"
    layout="sectioned"
    :title="formTitle"
    :subtitle="formSubtitle"
    :icon="dialogIcon"
    :icon-theme="isButtonType ? 'warning' : 'primary'"
    :status-text="currentTypeMeta.label"
    :status-type="currentTypeMeta.tagType"
    :width="760"
    extra-class="menu-form-dialog"
    :confirm-loading="submitting"
    confirm-text="保存"
    :mask-closable="!submitting"
    @confirm="handleSubmit"
  >
    <AxisForm
      ref="formRef"
      :model="form"
      :sections="formSections"
      :columns="2"
      :disabled="submitting"
      @change="handleFormChange"
    >
      <template #field-permission="{ disabled, setValue }">
        <div class="menu-permission-panel" :class="{ 'is-selected': !!selectedPermissionCode }">
          <div class="menu-permission-panel__meta">
            <span class="menu-permission-panel__hint">{{ permissionModeHint }}</span>
            <button
              v-if="suggestedModule"
              type="button"
              class="menu-permission-panel__switch"
              :disabled="disabled"
              @click="compactMode = !compactMode"
            >
              {{ compactMode ? '查看其他模块' : '回到推荐模块' }}
            </button>
          </div>

          <NSelect
            class="menu-permission-panel__picker"
            :value="selectedPermissionCode || null"
            :options="permissionSelectOptions"
            :disabled="disabled"
            clearable
            filterable
            :placeholder="permissionPlaceholder"
            :to="true"
            @update:value="value => commitPermission(value, setValue)"
          />

          <div v-if="selectedPermissionCode" class="menu-permission-panel__preview">
            <span class="menu-permission-panel__preview-label">权限码</span>
            <NTag size="small" round :bordered="false" type="info" class="menu-permission-panel__tag">
              {{ selectedPermissionCode }}
            </NTag>
          </div>
        </div>
      </template>

      <template #field-icon="{ value, disabled, setValue }">
        <AxisIconPicker
          :value="typeof value === 'string' ? value : ''"
          :disabled="disabled"
          @update:value="setValue"
        />
      </template>
    </AxisForm>
  </FormDialog>
</template>

<style scoped lang="scss">
  :deep(.menu-form-item--permission) {
    .n-form-item-blank {
      width: 100%;
    }

    .n-form-item-feedback-wrapper {
      min-height: 18px;
    }
  }

  .menu-permission-panel {
    width: 100%;
    padding: 12px 14px;
    border: 1px solid var(--axis-border-color);
    border-radius: 10px;
    background:
      linear-gradient(
        180deg,
        color-mix(in srgb, var(--axis-primary-color) 5%, var(--axis-card-color)) 0%,
        var(--axis-card-color) 56%
      );
    transition:
      border-color 0.18s ease,
      box-shadow 0.18s ease;

    &.is-selected {
      border-color: color-mix(in srgb, var(--axis-primary-color) 35%, var(--axis-border-color));
      box-shadow: 0 0 0 3px color-mix(in srgb, var(--axis-primary-color) 10%, transparent);
    }

    &__meta {
      display: flex;
      min-width: 0;
      align-items: center;
      justify-content: space-between;
      gap: 12px;
      margin-bottom: 10px;
    }

    &__hint {
      min-width: 0;
      overflow: hidden;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 18px;
      text-overflow: ellipsis;
      white-space: nowrap;
    }

    &__switch {
      flex: 0 0 auto;
      padding: 0;
      border: 0;
      background: transparent;
      color: var(--axis-primary-color);
      font-size: 12px;
      line-height: 18px;
      white-space: nowrap;
      cursor: pointer;

      &:hover:not(:disabled) {
        text-decoration: underline;
        text-underline-offset: 2px;
      }

      &:disabled {
        opacity: 0.45;
        cursor: not-allowed;
      }
    }

    &__picker {
      width: 100%;
    }

    &__preview {
      display: flex;
      min-width: 0;
      align-items: center;
      gap: 8px;
      margin-top: 10px;
      padding-top: 10px;
      border-top: 1px dashed color-mix(in srgb, var(--axis-border-color) 80%, transparent);
    }

    &__preview-label {
      flex: 0 0 auto;
      color: var(--axis-text-color-3);
      font-size: 12px;
      line-height: 18px;
    }

    &__tag {
      max-width: 100%;
      overflow: hidden;
      text-overflow: ellipsis;
      font-family: Consolas, Monaco, 'Courier New', monospace;
      font-size: 12px;
    }
  }
</style>
