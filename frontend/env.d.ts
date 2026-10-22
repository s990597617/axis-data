/// <reference types="vite/client" />

/** 由 Vite 从 package.json 注入，确保界面展示版本与发布版本一致。 */
declare const __APP_VERSION__: string

/** Vite 环境变量类型声明。 */
interface ImportMetaEnv {
  /** 应用展示名称，用于浏览器标题、系统名称展示等场景。 */
  readonly VITE_APP_TITLE: string
  /** Vite 开发服务器端口。 */
  readonly VITE_PORT: string
  /** 应用公共基础路径，对应 Vite 的 base 配置。 */
  readonly VITE_PUBLIC_PATH: string
  /** Vite 开发代理目标地址。 */
  readonly VITE_PROXY_TARGET: string
  /** 直连接口基础地址。 */
  readonly VITE_API_BASE_URL: string
  /** 接口统一前缀。 */
  readonly VITE_API_PREFIX: string
  /** 是否启用 Vite 开发代理，通常取值为 true 或 false。 */
  readonly VITE_USE_PROXY: string
  /** 是否开启构建产物分析，通常取值为 true 或 false。 */
  readonly VITE_ANALYZE?: string
  /** SM2 加密公钥（HEX，未压缩点，含 04 前缀）；与后端 System.PublicKey 保持一致。 */
  readonly VITE_SM2_PUBLIC_KEY?: string
}

/** ImportMeta 类型扩展。 */
interface ImportMeta {
  /** 当前模块可访问的环境变量对象。 */
  readonly env: ImportMetaEnv
}

/**
 * sm-crypto 0.3.x 类型补丁。
 * 该包未发布官方 d.ts，仅声明项目实际用到的 sm2 加解密 API。
 */
declare module 'sm-crypto' {
  /** SM2 加密模式：0 = C1C2C3，1 = C1C3C2（与后端 BouncyCastle 默认一致）。 */
  export type SM2CipherMode = 0 | 1

  export const sm2: {
    doEncrypt(msg: string | Uint8Array, publicKey: string, cipherMode?: SM2CipherMode): string
    doDecrypt(
      cipher: string,
      privateKey: string,
      cipherMode?: SM2CipherMode,
      options?: { output?: 'string' | 'array' }
    ): string
    generateKeyPairHex(): { publicKey: string; privateKey: string }
  }
}
