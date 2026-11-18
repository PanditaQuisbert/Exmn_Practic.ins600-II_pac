import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'

// El backend permite CORS para http://localhost:5173 (desarrollo) y
// http://localhost:4173 (vista previa de la compilación).
export default defineConfig({
  plugins: [vue()],
  server: {
    port: 5173,
    strictPort: true
  },
  preview: {
    port: 4173,
    strictPort: true
  },
  build: {
    outDir: 'dist',
    sourcemap: false
  }
})
