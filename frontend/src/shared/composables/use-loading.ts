import { ref } from 'vue'

export function useLoading(initialValue = false) {
  const loading = ref(initialValue)

  function startLoading() {
    loading.value = true
  }

  function endLoading() {
    loading.value = false
  }

  async function withLoading<T>(task: () => Promise<T>) {
    startLoading()
    try {
      return await task()
    } finally {
      endLoading()
    }
  }

  return {
    loading,
    startLoading,
    endLoading,
    withLoading,
  }
}
