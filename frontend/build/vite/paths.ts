import path from 'node:path'
import { fileURLToPath, URL } from 'node:url'

/** 项目根目录，供 Vite 配置层统一解析绝对路径。 */
export const projectRoot = fileURLToPath(new URL('../../', import.meta.url))

/** 源码目录绝对路径。 */
export const srcRoot = path.resolve(projectRoot, 'src')

/** shared 目录绝对路径，自动导入声明文件会输出到该目录下的 types 子目录。 */
export const sharedRoot = path.resolve(srcRoot, 'shared')

/** 将项目根目录下的相对路径解析为绝对路径。 */
export function resolveRootPath(...segments: string[]) {
  return path.resolve(projectRoot, ...segments)
}
