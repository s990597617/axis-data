<script setup lang="ts">
  import { FormDialog } from '@/shared/components/form-dialog'

  defineOptions({ name: 'OpenApiUsageDialog' })

  const visible = defineModel<boolean>({ default: false })

  function handleConfirm() {
    visible.value = false
  }
</script>

<template>
  <FormDialog
    v-model="visible"
    title="OpenAPI 使用"
    subtitle="Signature 认证接入与签名计算说明"
    icon="ant-design:api-outlined"
    icon-theme="info"
    :width="720"
    max-body-height="min(72vh, 680px)"
    :show-cancel="false"
    confirm-text="知道了"
    mask-closable
    @confirm="handleConfirm"
  >
    <div class="openapi-usage">
      <section class="openapi-usage__section">
        <h3 class="openapi-usage__title">接入方式</h3>
        <p class="openapi-usage__text">在需要使用 Signature 身份验证的 Api 中声明认证方案：</p>
        <pre class="openapi-usage__code">AuthSchemes(SignatureAuthenticationDefaults.AuthenticationScheme);</pre>
        <p class="openapi-usage__text">若需同时保留 Jwt：</p>
        <pre class="openapi-usage__code">this.AllowJwtOrSignature();
// 或
AuthSchemes("Bearer", SignatureAuthenticationDefaults.AuthenticationScheme);</pre>
        <p class="openapi-usage__text">OpenAPI 接口请同时跳过管理端防重放头：</p>
        <pre class="openapi-usage__code">Options(x => x.SkipAntiReplay("OpenAPI"));</pre>
        <ul class="openapi-usage__list">
          <li>免登录识别访问接口用户的身份（绑定用户 Claims）</li>
          <li>通过时间戳 + Nonce 防止潜在的重放攻击</li>
        </ul>
      </section>

      <section class="openapi-usage__section">
        <h3 class="openapi-usage__title">OpenAPI 签名流程</h3>
        <p class="openapi-usage__text">
          客户端在请求时生成签名
          <code>sign</code>
          ，并添加公共 Header：
        </p>
        <div class="openapi-usage__headers">
          <div class="openapi-usage__header-row">
            <code>accessKey</code>
            <span>身份标识</span>
          </div>
          <div class="openapi-usage__header-row">
            <code>timestamp</code>
            <span>时间戳，精确到秒</span>
          </div>
          <div class="openapi-usage__header-row">
            <code>nonce</code>
            <span>唯一随机数（建议 ≥ 6 位）</span>
          </div>
          <div class="openapi-usage__header-row">
            <code>sign</code>
            <span>签名数据</span>
          </div>
        </div>
      </section>

      <section class="openapi-usage__section">
        <h3 class="openapi-usage__title">计算签名</h3>
        <p class="openapi-usage__text">
          按如下顺序拼接参数（中间用
          <code>&amp;</code>
          ，不含空格）：
        </p>
        <pre class="openapi-usage__code">METHOD&amp;/api/xxx&amp;accessKey&amp;timestamp&amp;nonce</pre>
        <ul class="openapi-usage__list">
          <li>
            <code>METHOD</code>
            需大写，如
            <code>GET</code>
          </li>
          <li>
            <code>url</code>
            去除协议、域名、查询参数，以
            <code>/</code>
            开头，如
            <code>/api/open-api/whoami</code>
          </li>
          <li>
            使用
            <strong>HMAC-SHA256</strong>
            ，以
            <code>accessSecret</code>
            为密钥对上述字符串签名，结果做
            <strong>Base64</strong>
            编码即为
            <code>sign</code>
          </li>
        </ul>
        <div class="openapi-usage__footer-tip">
          <div>
            联调示例：
            <code>GET /api/open-api/whoami</code>
          </div>
          <div>
            HMAC 在线计算：
            <a href="https://1024tools.com/hmac" target="_blank" rel="noreferrer">
              https://1024tools.com/hmac
            </a>
          </div>
        </div>
      </section>
    </div>
  </FormDialog>
</template>

<style scoped lang="scss">
  @use '../styles/usage-dialog.scss';
</style>
