<script setup lang="ts">
  import { Icon } from '@iconify/vue'

  import logoUrl from '@/assets/images/logo.png'
  import { appConfig } from '@/config/app.config'
  import { useLogin } from './hooks/use-login'

  const {
    captchaImg,
    formModel,
    handleLogin,
    isRefreshing,
    isShaking,
    isSubmitting,
    loginState,
    refreshCaptcha,
    rememberMe
  } = useLogin()
</script>

<template>
  <div class="login-container">
    <!-- 左侧品牌展示 -->
    <section class="login-banner">
      <div class="banner-bg">
        <div class="blob blob-1" />
        <div class="blob blob-2" />
        <div class="blob blob-3" />
        <div class="grid-mask" />
        <div class="light-beam light-beam-1" />
        <div class="light-beam light-beam-2" />
      </div>

      <div class="brand">
        <div class="logo-wrapper">
          <img :src="logoUrl" alt="logo" class="brand-logo" />
        </div>
        <div class="brand-meta">
          <span class="brand-text">{{ appConfig.appName }}</span>
          <span class="brand-subtext">MANUFACTURING EXECUTION SYSTEM</span>
        </div>
      </div>

      <div class="banner-center">
        <div class="banner-item">
          <div class="banner-tag">AXIS MES</div>
          <h1 class="banner-title">
            从工单到报工
            <br />
            把现场跑起来
          </h1>
          <div class="banner-divider" />
          <p class="banner-desc">
            面向车间现场的制造执行系统。物料档案、工艺路线、工单下达与报工，跑在同一套权限与租户底座上。
          </p>

          <div class="banner-features">
            <div class="feature-item">
              <span class="feature-dot" />
              <span>物料与工艺主数据</span>
            </div>
            <div class="feature-item">
              <span class="feature-dot" />
              <span>工单下达到报工闭环</span>
            </div>
            <div class="feature-item">
              <span class="feature-dot" />
              <span>权限与多租户一体</span>
            </div>
          </div>
        </div>
      </div>

      <div class="banner-footer">
        <span>{{ appConfig.appShortName }}</span>
        <span>v{{ appConfig.appVersion }}</span>
      </div>
    </section>

    <!-- 右侧登录区 -->
    <section class="login-form-wrapper">
      <div class="form-bg-decoration form-bg-decoration--1" />
      <div class="form-bg-decoration form-bg-decoration--2" />

      <div class="form-container" :class="{ 'shake-active': isShaking }">
        <header class="form-header">
          <div class="form-badge">WELCOME BACK</div>
          <h2>欢迎登录 {{ appConfig.appShortName }}</h2>
          <p>请输入账号，进入制造执行控制台</p>
        </header>

        <n-form :model="formModel" size="large" :show-label="false" class="login-form">
          <n-form-item-row>
            <n-input
              v-model:value="formModel.username"
              placeholder="请输入用户名"
              class="custom-input"
              @keyup.enter="handleLogin"
            >
              <template #prefix>
                <Icon icon="ant-design:user-outlined" />
              </template>
            </n-input>
          </n-form-item-row>

          <n-form-item-row>
            <n-input
              v-model:value="formModel.password"
              type="password"
              show-password-on="mousedown"
              placeholder="请输入密码"
              class="custom-input"
              @keyup.enter="handleLogin"
            >
              <template #prefix>
                <Icon icon="ant-design:lock-outlined" />
              </template>
            </n-input>
          </n-form-item-row>

          <n-form-item-row>
            <div class="captcha-row">
              <n-input
                v-model:value="formModel.captcha"
                placeholder="请输入验证码"
                maxlength="4"
                class="custom-input captcha-input"
                @keyup.enter="handleLogin"
              >
                <template #prefix>
                  <Icon icon="ant-design:safety-certificate-outlined" />
                </template>
              </n-input>

              <div
                class="captcha-img"
                :class="{ refreshing: isRefreshing }"
                title="点击刷新验证码"
                @click="refreshCaptcha()"
              >
                <img
                  v-if="captchaImg"
                  :src="captchaImg"
                  alt="验证码"
                  class="captcha-image"
                  draggable="false"
                />
                <span v-else class="captcha-placeholder">加载中...</span>
                <div class="captcha-refresh-icon">
                  <Icon icon="ant-design:reload-outlined" />
                </div>
              </div>
            </div>
          </n-form-item-row>

          <div class="form-options">
            <n-checkbox v-model:checked="rememberMe">记住我</n-checkbox>
            <n-button text type="primary" class="forget-pwd">忘记密码？</n-button>
          </div>

          <button
            type="button"
            class="action-btn"
            :class="loginState"
            :disabled="isSubmitting || loginState === 'success'"
            @click="handleLogin"
          >
            <div class="btn-content">
              <template v-if="loginState === 'default'">
                <Icon icon="ant-design:login-outlined" />
                <span>登 录</span>
              </template>

              <template v-else-if="loginState === 'loading'">
                <Icon icon="ant-design:loading-outlined" class="loading-icon" />
                <span>验证中...</span>
              </template>

              <template v-else>
                <Icon icon="ant-design:check-circle-filled" />
                <span>登录成功</span>
              </template>
            </div>
          </button>
        </n-form>
      </div>
    </section>
  </div>
</template>

<style scoped lang="scss" src="./styles.scss"></style>
