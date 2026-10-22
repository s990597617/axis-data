import { computed, ref, watch, type Ref } from 'vue'
import type { SelectOption } from 'naive-ui'

import { getDictItems, type DictCacheItemDto } from '@/modules/system/api/dict'

export type { DictSelectOption } from '@/modules/system/api/dict'

/** 同 code 并发请求合并，避免表格多列同时拉字典时重复打接口。 */
const pendingByCode = new Map<string, Promise<SelectOption[]>>()

/** 会话内内存缓存（写字典后刷新页面或调用 clearDictCache 失效）。 */
const cacheByCode = new Map<string, SelectOption[]>()

function coerceDictValue(raw: string): string | number {
  if (/^-?\d+$/.test(raw)) return Number(raw)
  return raw
}

function toSelectOption(item: DictCacheItemDto): SelectOption {
  return {
    label: item.label,
    value: coerceDictValue(item.value),
    tagStyle: item.tagStyle,
    listClass: item.listClass,
    isDefault: item.isDefault,
    orderNo: item.orderNo
  }
}

function sameDictValue(left: unknown, right: unknown) {
  if (left === right) return true
  if (left == null || right == null) return false
  return String(left) === String(right)
}

/**
 * 拉取字典选项（带内存缓存）。可直接交给 AxisTable：
 * `dict: () => loadDictOptions('gender')`
 */
export async function loadDictOptions(code: string): Promise<SelectOption[]> {
  const key = code.trim()
  if (!key) return []

  const cached = cacheByCode.get(key)
  if (cached) return cached

  const pending = pendingByCode.get(key)
  if (pending) return pending

  const task = (async () => {
    const items = (await getDictItems(key)) ?? []
    const options = items.map(toSelectOption)
    cacheByCode.set(key, options)
    return options
  })().finally(() => {
    pendingByCode.delete(key)
  })

  pendingByCode.set(key, task)
  return task
}

/** 清空指定或全部字典前端缓存（管理端改完字典后可调用）。 */
export function clearDictCache(code?: string) {
  if (code) {
    cacheByCode.delete(code.trim())
    pendingByCode.delete(code.trim())
    return
  }
  cacheByCode.clear()
  pendingByCode.clear()
}

/**
 * 在已加载选项中把 value 翻译成 label；找不到则回退原值字符串。
 * AxisTable 列配置了 `dict` 时会自动做同样匹配，一般不必手写。
 */
export function matchDictLabel(
  options: readonly SelectOption[],
  value: unknown,
  fallback = '-'
) {
  if (value === null || value === undefined || value === '') return fallback
  const hit = options.find(item => sameDictValue(item.value, value))
  return hit?.label != null ? String(hit.label) : String(value)
}

/**
 * 业务页按字典编码使用缓存字典。
 *
 * @example
 * const { options, getLabel } = useDict('gender')
 * // 表格：dict: () => loadDictOptions('gender')
 */
export function useDict(code: string | Ref<string>) {
  const codeRef = computed(() => (typeof code === 'string' ? code : code.value).trim())
  const options = ref<SelectOption[]>([])
  const loading = ref(false)
  const loaded = ref(false)

  async function reload() {
    const key = codeRef.value
    if (!key) {
      options.value = []
      loaded.value = true
      return
    }

    loading.value = true
    try {
      clearDictCache(key)
      options.value = await loadDictOptions(key)
      loaded.value = true
    } finally {
      loading.value = false
    }
  }

  async function ensureLoaded() {
    if (loaded.value && options.value.length) return options.value
    const key = codeRef.value
    if (!key) return []
    loading.value = true
    try {
      options.value = await loadDictOptions(key)
      loaded.value = true
      return options.value
    } finally {
      loading.value = false
    }
  }

  function getLabel(value: unknown, fallback = '-') {
    return matchDictLabel(options.value, value, fallback)
  }

  watch(
    codeRef,
    () => {
      loaded.value = false
      void ensureLoaded()
    },
    { immediate: true }
  )

  return {
    options,
    loading,
    loaded,
    reload,
    ensureLoaded,
    getLabel
  }
}
