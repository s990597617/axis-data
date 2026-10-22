declare module '*.vue' {
  import type { DefineComponent } from 'vue'
  const component: DefineComponent<Record<string, unknown>, Record<string, unknown>, unknown>
  export default component
}

declare global {
  type Recordable<T = unknown> = Record<string, T>
  type Nullable<T> = T | null
  type Arrayable<T> = T | T[]
}

export {}
