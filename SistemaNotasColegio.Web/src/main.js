import { createApp } from 'vue'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap-icons/font/bootstrap-icons.css'
import './styles/main.css'

import App from './App.vue'
import router from './router'
import { toast } from './stores/toast'

const app = createApp(App)
app.use(router)
app.mount('#app')

window.addEventListener('snc:sesion-expirada', () => {
  toast.warning('Su sesión expiró. Inicie sesión nuevamente.')

  if (router.currentRoute.value.name !== 'login') {
    router.replace({ name: 'login', query: { sesionExpirada: '1' } })
  }
})
