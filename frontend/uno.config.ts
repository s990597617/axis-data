import { defineConfig } from 'unocss'
import presetWind4 from '@unocss/preset-wind4'
import presetAttributify from '@unocss/preset-attributify'

export default defineConfig({
  shortcuts: {
    'wh-full': 'w-full h-full',
  },
  presets: [presetWind4(), presetAttributify()],
})
