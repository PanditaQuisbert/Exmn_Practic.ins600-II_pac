<script setup>
import LoadingBar from './components/LoadingBar.vue'
import ToastHost from './components/ToastHost.vue'
import ConfirmDialog from './components/ConfirmDialog.vue'
</script>

<template>
  <LoadingBar />
  <RouterView />
  <ToastHost />
  <ConfirmDialog />
</template>
