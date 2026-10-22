import { sm2 } from 'sm-crypto'

/** SM2 加密模式：1 = C1C3C2（与后端 BouncyCastle 默认一致）。 */
const SM2_MODE = 1

/** 默认从环境变量读取 SM2 公钥；可在调用处覆盖（如多环境切换）。 */
function getDefaultPublicKey(): string {
  return (import.meta.env.VITE_SM2_PUBLIC_KEY ?? '').trim()
}

/**
 * 使用 SM2 公钥加密敏感字符串（账号 / 密码等）。
 *
 * 当公钥未配置时返回原文，便于本地未启用 SM2 的临时调试；生产环境务必配置 `VITE_SM2_PUBLIC_KEY`
 * 并与后端 `appsettings.json -> System.PublicKey` 保持一致。
 *
 * 后端解密：参见 `Axis.Infrastructure.Security.SmTools.Sm2Decrypt`，自动兼容是否携带 04 前缀。
 */
export function sm2Encrypt(plain: string, publicKey?: string): string {
  if (plain == null || plain === '') return plain
  const key = (publicKey ?? getDefaultPublicKey()).trim()
  if (!key) return plain
  return sm2.doEncrypt(plain, key, SM2_MODE)
}

/** 是否已配置 SM2 公钥。 */
export function isSm2Enabled(): boolean {
  return getDefaultPublicKey().length > 0
}
