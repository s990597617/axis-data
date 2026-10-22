import type { AppRouteRecord } from '@/shared/types/app-route.ts'

export const RootLayout = () => import('@/layouts/index.vue')

/** 嵌套目录占位：仅透传子路由，不再套一层完整布局壳。 */
export const ParentView = () => import('@/layouts/parent-view.vue')

/**
 * 开发专用路由
 *
 * @description
 * 仅在 `import.meta.env.DEV` 为真时注入到路由表，生产构建通过 tree-shaking 整体剔除，
 * 对应组件也不会出现在 `dist` 产物中。
 */
const devOnlyRoutes: AppRouteRecord[] = import.meta.env.DEV
  ? [
      {
        path: '/icons',
        name: 'Icons',
        component: () => import('@/modules/dev/views/icons/index.vue'),
        meta: {
          title: '图标浏览',
          keepAlive: true,
          icon: 'ant-design:appstore-outlined',
          order: 99,
          hidden: true
        }
      },
      {
        path: '/table-demo',
        name: 'TableDemo',
        component: () => import('@/modules/dev/views/table/index.vue'),
        redirect: '/table-demo/basic',
        meta: {
          title: '表格演示',
          keepAlive: true,
          icon: 'ant-design:table-outlined',
          order: 98,
          hidden: true
        },
        children: [
          {
            path: 'basic',
            name: 'TableDemoBasic',
            component: () => import('@/modules/dev/views/table/basic.vue'),
            meta: {
              title: '基础表格',
              keepAlive: true,
              icon: 'ant-design:table-outlined',
              hidden: true
            }
          },
          {
            path: 'remote',
            name: 'TableDemoRemote',
            component: () => import('@/modules/dev/views/table/remote.vue'),
            meta: {
              title: '远程表格',
              keepAlive: true,
              icon: 'ant-design:cloud-server-outlined',
              hidden: true
            }
          },
          {
            path: 'advanced',
            name: 'TableDemoAdvanced',
            component: () => import('@/modules/dev/views/table/advanced.vue'),
            meta: {
              title: '进阶表格',
              keepAlive: true,
              icon: 'ant-design:control-outlined',
              hidden: true
            }
          }
        ]
      }
    ]
  : []

export const staticRoutes: AppRouteRecord[] = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/pages/login/index.vue'),
    meta: {
      title: '登录',
      layout: 'full'
    }
  },
  {
    path: '/',
    name: 'Root',
    component: RootLayout,
    redirect: '/home',
    meta: {
      title: '根路由',
      hidden: true,
      layout: 'default'
    },
    children: [
      ...devOnlyRoutes
    ]
  },
  {
    path: '/403',
    name: 'Forbidden',
    component: () => import('@/pages/exception/403.vue'),
    meta: {
      title: '无权限',
      layout: 'full',
      hidden: true
    }
  },
  {
    path: '/404',
    name: 'NotFound',
    component: () => import('@/pages/exception/404.vue'),
    meta: {
      title: '页面不存在',
      layout: 'full',
      hidden: true
    }
  }
]
