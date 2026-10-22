import { computed, onMounted, reactive, ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'

import { getCaptcha } from '@/modules/system/api/author'
import { useLoading } from '@/shared/composables/use-loading'
import { tryRequest } from '@/shared/http'
import { message } from '@/shared/utils/message'
import { usePermissionStore } from '@/store/modules/permission.store'
import { useUserStore } from '@/store/modules/user.store'

type LoginState = 'default' | 'loading' | 'success' | 'error'

/** 登录页的表单状态、验证码刷新和跳转流程。 */
export function useLogin() {
  const route = useRoute()
  const router = useRouter()
  const userStore = useUserStore()
  const permissionStore = usePermissionStore()
  const { loading, withLoading } = useLoading()

  const isShaking = ref(false)
  const isRefreshing = ref(false)
  const rememberMe = ref(userStore.rememberMe)
  const loginState = ref<LoginState>('default')
  const captchaImg = ref('')
  const captchaId = ref('')

  const formModel = reactive({
    username: '',
    password: '',
    captcha: ''
  })

  const isSubmitting = computed(() => loginState.value === 'loading' || loading.value)

  async function refreshCaptcha(clear = true) {
    if (clear) formModel.captcha = ''
    isRefreshing.value = true

    const [res, err] = await tryRequest(getCaptcha(), { fallbackMessage: '获取验证码失败' })
    if (!err) {
      captchaId.value = res.id
      captchaImg.value = res.img
    }

    setTimeout(() => {
      isRefreshing.value = false
    }, 300)
  }

  function triggerError() {
    isShaking.value = true
    setTimeout(() => {
      isShaking.value = false
    }, 600)
  }

  async function handleLogin() {
    if (isSubmitting.value) return

    if (!formModel.username || !formModel.password) {
      message.warning('请输入用户名和密码')
      triggerError()
      return
    }

    if (!formModel.captcha) {
      message.warning('请输入验证码')
      triggerError()
      return
    }

    if (!captchaId.value) {
      message.warning('请先获取验证码')
      triggerError()
      void refreshCaptcha(true)
      return
    }

    loginState.value = 'loading'
    userStore.setRememberMe(rememberMe.value)

    const [, err] = await tryRequest(
      withLoading(() =>
        userStore.login({
          username: formModel.username,
          password: formModel.password,
          codeId: captchaId.value,
          code: formModel.captcha
        })
      ),
      { fallbackMessage: '登录失败' }
    )

    if (err) {
      loginState.value = 'default'
      triggerError()
      void refreshCaptcha(true)
      return
    }

    permissionStore.reset(router)
    loginState.value = 'success'
    message.success('登录成功，正在进入系统')

    const redirect = route.query.redirect
    const target =
      typeof redirect === 'string' && redirect.startsWith('/') && !redirect.startsWith('//')
        ? redirect
        : '/home'
    setTimeout(() => {
      router.replace(target)
    }, 700)
  }

  onMounted(() => {
    void refreshCaptcha(true)
  })

  return {
    captchaImg,
    formModel,
    handleLogin,
    isRefreshing,
    isShaking,
    isSubmitting,
    loginState,
    refreshCaptcha,
    rememberMe
  }
}
