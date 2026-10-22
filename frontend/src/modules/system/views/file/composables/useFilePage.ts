import { computed, onMounted, ref, watch } from 'vue'
import type { UploadCustomRequestOptions, UploadFileInfo } from 'naive-ui'

import {
  deleteFile,
  downloadFile,
  getFileModules,
  getFilePage,
  uploadFile,
  type FileDto,
  type FileModuleStatDto
} from '@/modules/system/api/file'
import { systemPermissionCodes } from '@/modules/system/permissions/codes'
import { tryRequest } from '@/shared/http'
import { dialog, message } from '@/shared/utils/message'

import {
  formatFileSize,
  getFileKind,
  resolveFileUrl,
  type FileKind
} from '../utils/file-browser'

export interface FileModuleOption {
  key: string
  label: string
  count: number
  icon: string
}

const MODULE_META: Record<string, { label: string; icon: string }> = {
  '': { label: '未分类', icon: 'ant-design:inbox-outlined' },
  common: { label: '通用', icon: 'ant-design:appstore-outlined' },
  avatar: { label: '头像', icon: 'ant-design:user-outlined' },
  attachment: { label: '附件', icon: 'ant-design:paper-clip-outlined' },
  excel: { label: 'Excel', icon: 'ant-design:file-excel-outlined' },
  image: { label: '图片', icon: 'ant-design:picture-outlined' }
}

function normalizePage(data: unknown): { items: FileDto[]; total: number } {
  if (Array.isArray(data)) return { items: data as FileDto[], total: data.length }
  if (!data || typeof data !== 'object') return { items: [], total: 0 }
  const source = data as Record<string, unknown>
  const rawItems = source.items ?? source.Items ?? source.list ?? source.List
  const items = Array.isArray(rawItems) ? (rawItems as FileDto[]) : []
  const total = Number(source.total ?? source.Total ?? source.itemCount ?? items.length) || 0
  return { items, total }
}

function moduleLabel(module: string) {
  const meta = MODULE_META[module]
  if (meta) return meta.label
  return module || '未分类'
}

function moduleIcon(module: string) {
  return MODULE_META[module]?.icon ?? 'ant-design:folder-outlined'
}

function triggerBlobDownload(blob: Blob, fileName: string) {
  const url = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = url
  anchor.download = fileName
  anchor.click()
  URL.revokeObjectURL(url)
}

export function useFilePage() {
  /* ============ 状态 & 引用 ============ */

  const permissions = systemPermissionCodes.file
  const modulesLoading = ref(false)
  const listLoading = ref(false)
  const uploading = ref(false)
  const keyword = ref('')
  const selectedModule = ref<string | null>(null)
  const page = ref(1)
  const pageSize = ref(24)
  const total = ref(0)
  const files = ref<FileDto[]>([])
  const moduleStats = ref<FileModuleStatDto[]>([])
  const previewVisible = ref(false)
  const previewFile = ref<FileDto | null>(null)
  let keywordTimer: ReturnType<typeof setTimeout> | null = null

  const totalCount = computed(() =>
    moduleStats.value.reduce((sum, item) => sum + (Number(item.count) || 0), 0)
  )

  const moduleOptions = computed<FileModuleOption[]>(() => {
    const items = moduleStats.value.map(item => {
      const key = item.module ?? ''
      return {
        key,
        label: moduleLabel(key),
        count: Number(item.count) || 0,
        icon: moduleIcon(key)
      }
    })
    return [
      {
        key: '__all__',
        label: '全部文件',
        count: totalCount.value,
        icon: 'ant-design:folder-open-outlined'
      },
      ...items
    ]
  })

  const activeModuleKey = computed(() =>
    selectedModule.value === null ? '__all__' : selectedModule.value
  )

  const uploadModule = computed(() =>
    selectedModule.value && selectedModule.value.length > 0
      ? selectedModule.value
      : 'common'
  )

  const currentTitle = computed(() => {
    if (selectedModule.value === null) return '全部文件'
    return moduleLabel(selectedModule.value)
  })

  /* ============ 数据加载 ============ */

  async function loadModules() {
    modulesLoading.value = true
    const [data, error] = await tryRequest(getFileModules())
    modulesLoading.value = false
    if (error || !data) return
    moduleStats.value = Array.isArray(data) ? data : []
  }

  async function loadFiles(options?: { keepPage?: boolean }) {
    if (!options?.keepPage) page.value = 1
    listLoading.value = true
    const [raw, error] = await tryRequest(
      getFilePage({
        page: page.value,
        pageSize: pageSize.value,
        keyword: keyword.value.trim() || undefined,
        module: selectedModule.value === null ? undefined : selectedModule.value
      })
    )
    listLoading.value = false
    if (error) return

    const result = normalizePage(raw)
    files.value = result.items
    total.value = result.total

    if (!options?.keepPage && result.items.length === 0 && page.value > 1 && result.total > 0) {
      page.value = Math.max(1, Math.ceil(result.total / pageSize.value))
      await loadFiles({ keepPage: true })
    }
  }

  async function reloadAll() {
    await Promise.all([loadModules(), loadFiles({ keepPage: true })])
  }

  function selectModule(key: string) {
    selectedModule.value = key === '__all__' ? null : key
  }

  function handleKeywordUpdate(value: string) {
    keyword.value = value
    if (keywordTimer) clearTimeout(keywordTimer)
    keywordTimer = setTimeout(() => {
      loadFiles()
    }, 280)
  }

  function handlePageChange(value: number) {
    page.value = value
    loadFiles({ keepPage: true })
  }

  function handlePageSizeChange(value: number) {
    pageSize.value = value
    loadFiles()
  }

  /* ============ 交互操作 ============ */

  function openPreview(file: FileDto) {
    previewFile.value = file
    previewVisible.value = true
  }

  function closePreview() {
    previewVisible.value = false
    previewFile.value = null
  }

  async function handleDownload(row: FileDto) {
    const [blob, error] = await tryRequest(downloadFile(row.fileId))
    if (error || !blob) return
    triggerBlobDownload(blob, row.originalName)
    message.success('开始下载')
  }

  function handleDelete(row: FileDto) {
    dialog.error({
      title: '删除文件',
      content: `确认删除「${row.originalName}」？物理文件将一并删除。`,
      positiveText: '删除',
      negativeText: '取消',
      onPositiveClick: async () => {
        const [, error] = await tryRequest(deleteFile(row.fileId))
        if (error) return
        message.success('已删除')
        if (previewFile.value?.fileId === row.fileId) closePreview()
        await reloadAll()
      }
    })
  }

  async function customUpload({ file, onFinish, onError }: UploadCustomRequestOptions) {
    const raw = file.file
    if (!raw) {
      onError()
      return
    }

    uploading.value = true
    const [, error] = await tryRequest(uploadFile(raw, uploadModule.value))
    uploading.value = false
    if (error) {
      onError()
      return
    }
    message.success('上传成功')
    onFinish()
    await reloadAll()
  }

  function beforeUpload(options: { file: UploadFileInfo }) {
    if (!options.file.file) {
      message.warning('请选择文件')
      return false
    }
    return true
  }

  function fileKindOf(file: FileDto): FileKind {
    return getFileKind(file)
  }

  function fileUrlOf(file: FileDto) {
    return resolveFileUrl(file)
  }

  watch(selectedModule, () => {
    loadFiles()
  })

  onMounted(async () => {
    await Promise.all([loadModules(), loadFiles()])
  })

  return {
    permissions,
    modulesLoading,
    listLoading,
    uploading,
    keyword,
    selectedModule,
    activeModuleKey,
    page,
    pageSize,
    total,
    files,
    moduleOptions,
    currentTitle,
    previewVisible,
    previewFile,
    selectModule,
    handleKeywordUpdate,
    handlePageChange,
    handlePageSizeChange,
    openPreview,
    closePreview,
    handleDownload,
    handleDelete,
    customUpload,
    beforeUpload,
    fileKindOf,
    fileUrlOf,
    formatFileSize,
    reloadAll
  }
}
