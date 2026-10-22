import { nextTick, onActivated, onBeforeUnmount, onMounted, ref, watch, type Ref } from 'vue'

const MIN_TABLE_BODY_HEIGHT = 200
const FALLBACK_PAGINATION_HEIGHT = 48
const BOTTOM_BUFFER = 12

function readViewportBottom() {
  const viewportHeight = window.visualViewport?.height ?? window.innerHeight
  const footer = document.querySelector('.layout-footer')
  if (footer instanceof HTMLElement) {
    const footerTop = footer.getBoundingClientRect().top
    if (footerTop > 0) return Math.min(footerTop, viewportHeight)
  }
  return viewportHeight
}

function readPaginationHeight(body: HTMLElement) {
  const pagination = body.querySelector('.n-data-table__pagination') as HTMLElement | null
  return pagination?.getBoundingClientRect().height || FALLBACK_PAGINATION_HEIGHT
}

function readMainPaddingBottom(body: HTMLElement) {
  const main = body.closest('.axis-table__main')
  if (!(main instanceof HTMLElement)) return 0
  const paddingBottom = Number.parseFloat(getComputedStyle(main).paddingBottom)
  return Number.isFinite(paddingBottom) ? paddingBottom : 0
}

/** 按屏幕可见底边给表体封顶，不依赖会随内容变高的容器。 */
export function useAxisTableViewportMaxHeight(options: {
  enabled: () => boolean
  rootRef: Ref<HTMLElement | null>
  bodyRef: Ref<HTMLElement | null>
}) {
  const maxHeight = ref<number>()
  let observer: ResizeObserver | null = null

  function measure() {
    if (!options.enabled()) {
      maxHeight.value = undefined
      return
    }

    const body = options.bodyRef.value
    if (!body) return

    const top = body.getBoundingClientRect().top
    const available =
      readViewportBottom() - top - readPaginationHeight(body) - readMainPaddingBottom(body) - BOTTOM_BUFFER
    maxHeight.value = Math.max(MIN_TABLE_BODY_HEIGHT, Math.floor(available))
  }

  function scheduleMeasure() {
    void nextTick(measure)
  }

  function bindObserver() {
    observer?.disconnect()
    const root = options.rootRef.value
    if (!root || typeof ResizeObserver === 'undefined') return
    observer = new ResizeObserver(() => {
      scheduleMeasure()
    })
    observer.observe(root)
  }

  onMounted(() => {
    bindObserver()
    scheduleMeasure()
    window.addEventListener('resize', measure)
    window.visualViewport?.addEventListener('resize', measure)
  })

  onActivated(() => {
    scheduleMeasure()
  })

  onBeforeUnmount(() => {
    observer?.disconnect()
    observer = null
    window.removeEventListener('resize', measure)
    window.visualViewport?.removeEventListener('resize', measure)
  })

  watch(
    () => options.enabled(),
    enabled => {
      if (enabled) scheduleMeasure()
      else maxHeight.value = undefined
    }
  )

  return { maxHeight, measure: scheduleMeasure }
}
