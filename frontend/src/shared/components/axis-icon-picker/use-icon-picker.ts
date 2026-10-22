import { computed, ref, watch } from 'vue'
import { listIcons } from '@iconify/vue'

export interface IconPickerStateProps {
  disabled?: boolean
  pageSize: number
  value?: string | null
}

export interface IconPickerStateEmit {
  (event: 'update:value', value: string | null): void
  (event: 'change', value: string | null): void
}

export function useIconPickerState(props: IconPickerStateProps, emit: IconPickerStateEmit) {
  const visible = ref(false)
  const search = ref('')
  const prefix = ref('')
  const page = ref(1)
  const hoverName = ref<string | null>(null)

  const allIcons = computed<string[]>(() => listIcons())

  const prefixStats = computed(() => {
    const map = new Map<string, number>()
    allIcons.value.forEach(name => {
      const [scope] = name.split(':')
      if (!scope) return
      map.set(scope, (map.get(scope) ?? 0) + 1)
    })
    return Array.from(map.entries())
      .map(([value, count]) => ({ value, count }))
      .sort((a, b) => a.value.localeCompare(b.value))
  })

  const prefixTabs = computed(() => [
    { value: '', label: '全部', count: allIcons.value.length },
    ...prefixStats.value.map(item => ({
      value: item.value,
      label: item.value,
      count: item.count
    }))
  ])

  const filteredIcons = computed(() => {
    const query = search.value.trim().toLowerCase()
    return allIcons.value.filter(name => {
      if (prefix.value && !name.startsWith(`${prefix.value}:`)) return false
      if (query && !name.toLowerCase().includes(query)) return false
      return true
    })
  })

  const totalPages = computed(() =>
    Math.max(1, Math.ceil(filteredIcons.value.length / props.pageSize))
  )

  const pagedIcons = computed(() => {
    const start = (page.value - 1) * props.pageSize
    return filteredIcons.value.slice(start, start + props.pageSize)
  })

  watch([search, prefix], () => {
    page.value = 1
  })

  watch(visible, next => {
    if (!next) return
    search.value = ''
    const current = props.value
    if (current) {
      const [scope] = current.split(':')
      prefix.value = scope ?? ''
      const index = filteredIcons.value.indexOf(current)
      page.value = index >= 0 ? Math.floor(index / props.pageSize) + 1 : 1
    } else {
      prefix.value = ''
      page.value = 1
    }
    hoverName.value = props.value || null
  })

  function select(name: string) {
    emit('update:value', name)
    emit('change', name)
    visible.value = false
  }

  function clear(event: MouseEvent) {
    event.stopPropagation()
    emit('update:value', null)
    emit('change', null)
  }

  function toggleOpen() {
    if (props.disabled) return
    visible.value = !visible.value
  }

  function prevPage() {
    if (page.value > 1) page.value -= 1
  }

  function nextPage() {
    if (page.value < totalPages.value) page.value += 1
  }

  return {
    clear,
    filteredIcons,
    hoverName,
    nextPage,
    page,
    pagedIcons,
    prefix,
    prefixTabs,
    prevPage,
    search,
    select,
    toggleOpen,
    totalPages,
    visible
  }
}
