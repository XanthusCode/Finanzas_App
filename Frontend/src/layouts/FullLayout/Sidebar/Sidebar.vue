<template>
    <aside class="sidebar">
      <div class="sidebar-logo">
        GASTOS<span style="color: var(--accent)">.</span>
      </div>

      <nav class="sidebar-nav">
        <RouterLink to="/" class="nav-item" active-class="nav-item--active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><rect x="3" y="3" width="7" height="7"/><rect x="14" y="3" width="7" height="7"/><rect x="3" y="14" width="7" height="7"/><rect x="14" y="14" width="7" height="7"/></svg>
          Dashboard
        </RouterLink>
        <RouterLink to="/gastos" class="nav-item" active-class="nav-item--active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M12 2v20M17 5H9.5a3.5 3.5 0 0 0 0 7h5a3.5 3.5 0 0 1 0 7H6"/></svg>
          Gastos
        </RouterLink>
        <RouterLink to="/ingresos" class="nav-item" active-class="nav-item--active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><polyline points="23 6 13.5 15.5 8.5 10.5 1 18"/><polyline points="17 6 23 6 23 12"/></svg>
          Ingresos
        </RouterLink>

        <div class="nav-divider" />

        <RouterLink to="/categorias" class="nav-item" active-class="nav-item--active">
          <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5"><path d="M4 6h16M4 10h16M4 14h8M4 18h8"/></svg>
          Categorías
        </RouterLink>
      </nav>

      <div class="sidebar-footer">
        <div class="sidebar-year">{{ anio }}</div>
      </div>
    </aside>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'

const anio = new Date().getFullYear()
</script>

<style scoped>
/* ── Sidebar ── */
.sidebar {
  background: var(--surface);
  border-right: 1px solid var(--border);
  display: flex;
  flex-direction: column;
  padding: 1.5rem 1rem;
  position: sticky;
  top: 0;
  height: 100vh;
}

.sidebar-logo {
  font-family: var(--ff-display);
  font-weight: 800;
  font-size: 1.1rem;
  letter-spacing: 0.04em;
  color: var(--text-primary);
  margin-bottom: 2.5rem;
  padding: 0 0.5rem;
}

.sidebar-nav {
  display: flex;
  flex-direction: column;
  gap: 0.25rem;
  flex: 1;
}

.nav-item {
  display: flex;
  align-items: center;
  gap: 0.65rem;
  padding: 0.6rem 0.75rem;
  border-radius: 6px;
  font-size: 0.78rem;
  color: var(--text-secondary);
  text-decoration: none;
  transition: all 0.2s;
  letter-spacing: 0.03em;
}
.nav-item:hover { background: var(--surface2); color: var(--text-primary); }
.nav-item--active { background: rgba(99,179,255,0.1); color: var(--accent); }

.nav-divider {
  height: 1px;
  background: var(--border);
  margin: 0.5rem 0;
}

.sidebar-footer {
  padding: 0 0.5rem;
}
.sidebar-year {
  font-size: 0.62rem;
  color: var(--text-muted);
  letter-spacing: 0.1em;
}

</style>
