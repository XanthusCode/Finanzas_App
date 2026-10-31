<template>
  <section
    class="chat"
    :class="{ open: abierto }"
    role="dialog"
    aria-label="Copiloto financiero"
    :aria-hidden="!abierto"
    @keydown.esc="cerrar"
  >
    <header class="chat__header">
      <div class="chat__title">
        <div class="chat__avatar">✨</div>
        <div>
          <h3>Copiloto</h3>
          <p><span class="chat__dot" /> Con tus datos de este mes</p>
        </div>
      </div>
      <div class="chat__actions">
        <button v-if="mensajes.length" class="chat__icon-btn" title="Nueva conversación" @click="limpiar">↺</button>
        <button class="chat__icon-btn" aria-label="Cerrar" @click="cerrar">✕</button>
      </div>
    </header>

    <div ref="lista" class="chat__messages">
      <template v-if="!mensajes.length">
        <div class="msg ai">
          <div class="bubble">¡Hola! 👋 Pregúntame lo que quieras sobre tus finanzas.</div>
        </div>
        <div class="chips">
          <button v-for="s in sugerencias" :key="s" class="chip" @click="onEnviar(s)">{{ s }}</button>
        </div>
      </template>

      <div v-for="(m, i) in mensajes" :key="i" class="msg" :class="m.rol === 'user' ? 'user' : 'ai'">
        <div class="bubble">{{ m.contenido }}</div>
      </div>

      <div v-if="cargando" class="msg ai">
        <div class="bubble typing"><span /><span /><span /></div>
      </div>
    </div>

    <form class="chat__input-area" @submit.prevent="onEnviar()">
      <div class="chat__input-wrapper">
        <input
          ref="input"
          v-model="texto"
          type="text"
          placeholder="Escribe tu pregunta..."
          maxlength="500"
          :tabindex="abierto ? 0 : -1"
        />
        <button class="chat__send" type="submit" :disabled="cargando || !texto.trim()" aria-label="Enviar">↑</button>
      </div>
    </form>
  </section>
</template>

<script setup lang="ts">
import { ref, watch, nextTick } from 'vue'
import { useCopiloto } from '@/composables/useCopiloto'

const { abierto, mensajes, cargando, enviar, cerrar, limpiar } = useCopiloto()

const texto = ref('')
const lista = ref<HTMLElement | null>(null)
const input = ref<HTMLInputElement | null>(null)

const sugerencias = [
  '¿Cuánto llevo gastado este mes?',
  '¿En qué categoría gasto más?',
  '¿Puedo permitirme algo de $500.000?',
]

async function onEnviar(t = texto.value) {
  texto.value = ''
  await enviar(t)
}

// Auto-scroll cuando llega un mensaje o aparece "escribiendo…"
watch(() => [mensajes.value.length, cargando.value], async () => {
  await nextTick()
  lista.value?.scrollTo({ top: lista.value.scrollHeight, behavior: 'smooth' })
})

// Al abrir: foco en el input y scroll al final
watch(abierto, async (v) => {
  if (!v) return
  await nextTick()
  input.value?.focus()
  lista.value?.scrollTo({ top: lista.value.scrollHeight })
})
</script>


<style scoped>
.chat {
  position: fixed;
  bottom: 96px;
  right: 24px;
  width: 380px;
  height: min(560px, calc(100vh - 140px));
  background: var(--surface);
  border: 1px solid var(--border);
  border-radius: var(--radius-modal);
  box-shadow: var(--shadow-pop);
  display: flex;
  flex-direction: column;
  overflow: hidden;
  z-index: 200;

  opacity: 0;
  pointer-events: none;
  transform: translateY(16px) scale(0.97);
  transform-origin: bottom right;
  transition: opacity 0.2s var(--ease-out), transform 0.25s var(--ease-out);
}
.chat.open { opacity: 1; pointer-events: auto; transform: none; }

.chat__header {
  padding: 14px 16px;
  border-bottom: 1px solid var(--border);
  display: flex;
  align-items: center;
  justify-content: space-between;
}
.chat__title { display: flex; align-items: center; gap: 12px; }
.chat__title h3 { margin: 0; font-size: 14px; font-weight: 600; color: var(--text-primary); }
.chat__title p { margin: 0; font-size: 11px; color: var(--text-secondary); display: flex; align-items: center; gap: 6px; }
.chat__avatar {
  width: 34px; height: 34px; border-radius: var(--radius);
  background: var(--avatar-gradient);
  display: grid; place-items: center; font-size: 15px;
}
.chat__dot { width: 6px; height: 6px; border-radius: 50%; background: var(--green); }
.chat__actions { display: flex; gap: 4px; }
.chat__icon-btn {
  background: transparent; border: none; color: var(--text-secondary);
  cursor: pointer; font-size: 15px; padding: 6px; border-radius: var(--radius-sm);
}
.chat__icon-btn:hover { color: var(--text-primary); background: var(--surface2); }

.chat__messages {
  flex: 1;
  overflow-y: auto;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.msg { display: flex; max-width: 85%; }
.msg.user { align-self: flex-end; }
.msg.ai { align-self: flex-start; }

.bubble {
  padding: 10px 14px;
  font-size: 13.5px;
  line-height: 1.5;
  white-space: pre-wrap;      /* respeta los saltos de línea de la IA */
  overflow-wrap: anywhere;
}
.msg.user .bubble {
  background: var(--accent);
  color: #fff;
  border-radius: 14px 14px 4px 14px;
}
.msg.ai .bubble {
  background: var(--surface2);
  border: 1px solid var(--border);
  color: var(--text-primary);
  border-radius: 14px 14px 14px 4px;
}

.chips { display: flex; flex-wrap: wrap; gap: 8px; }
.chip {
  background: transparent;
  border: 1px solid var(--border);
  color: var(--text-secondary);
  border-radius: 999px;
  padding: 6px 12px;
  font-size: 12px;
  cursor: pointer;
  font-family: inherit;
}
.chip:hover { border-color: var(--border-hover); color: var(--text-primary); }

/* Indicador "escribiendo…" */
.typing { display: flex; gap: 4px; padding: 14px; }
.typing span {
  width: 6px; height: 6px; border-radius: 50%;
  background: var(--text-secondary);
  animation: blink 1.2s infinite ease-in-out;
}
.typing span:nth-child(2) { animation-delay: 0.15s; }
.typing span:nth-child(3) { animation-delay: 0.3s; }
@keyframes blink { 0%, 80%, 100% { opacity: 0.25; } 40% { opacity: 1; } }

.chat__input-area { padding: 12px; border-top: 1px solid var(--border); }
.chat__input-wrapper {
  display: flex;
  align-items: center;
  gap: 8px;
  background: var(--surface2);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 6px 6px 6px 14px;
}
.chat__input-wrapper:focus-within { border-color: var(--accent); box-shadow: var(--focus-ring); }
.chat__input-wrapper input {
  flex: 1; min-width: 0;
  background: transparent; border: none; outline: none;
  color: var(--text-primary); font-size: 14px; font-family: inherit;
}
.chat__input-wrapper input::placeholder { color: var(--text-muted); }
.chat__send {
  width: 32px; height: 32px; border: none; border-radius: var(--radius-sm);
  background: var(--accent); color: #fff; cursor: pointer; font-size: 16px;
}
.chat__send:disabled { opacity: 0.4; cursor: not-allowed; }

@media (max-width: 480px) {
  .chat { left: 16px; right: 16px; width: auto; bottom: 88px; }
}

@media (prefers-reduced-motion: reduce) {
  .chat { transition: opacity 0.15s; transform: none; }
  .typing span { animation: none; }
}
</style>
