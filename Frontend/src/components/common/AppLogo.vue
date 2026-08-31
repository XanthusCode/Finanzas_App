<template>
  <div class="app-logo">
    <svg class="app-logo-mark" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.8" stroke-linecap="round" stroke-linejoin="round">
      <rect x="2" y="2" width="20" height="20" rx="6" opacity="0.3" />
      <path d="M7.5 16.5V12.5" />
      <path d="M12 16.5V9" />
      <path d="M16.5 16.5V7" />
      <circle cx="16.5" cy="7" r="1.4" fill="currentColor" stroke="none" />
    </svg>
    <span class="app-logo-text">Zaldo</span>
  </div>
</template>

<style scoped>
.app-logo {
  display: inline-flex;
  align-items: center;
  gap: 0.45em;
  font: inherit;
}
.app-logo-mark {
  width: 1.35em;
  height: 1.35em;
  color: var(--accent);
  flex-shrink: 0;
}
.app-logo-text {
  font: inherit;
  color: inherit;
}
</style>
