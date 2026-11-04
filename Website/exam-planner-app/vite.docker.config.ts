import { defineConfig, mergeConfig } from 'vite'
import baseConfig from './vite.config'

export default mergeConfig(baseConfig, {
  server: {
    proxy: {
      '/api': {
        target: 'http://api:8080',
        changeOrigin: true
      }
    }
  }
})