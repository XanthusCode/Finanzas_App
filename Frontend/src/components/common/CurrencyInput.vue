<template>
    <input
        ref="inputRef"
        type="text"
        inputmode="numeric"
        :value="displayed"
        :placeholder="placeholder"
        :class="inputClass"
        @input="onInput"
        @blur="onBlur"
        @focus="onFocus"    
    />
</template>

<script setup lang="ts">
import {ref, watch, nextTick} from 'vue';

const props = withDefaults(defineProps<{
  modelValue: number
  placeholder?: string
  inputClass?: string
}>(), {
  placeholder: '$ 0',
  inputClass: 'input',
})
const emit = defineEmits<{'update:modelValue' : [value: number]}>()

const inputRef = ref<HTMLInputElement>()
const displayed = ref('')
const focused = ref(false)

// formatea numero a string con puntos: 1500000 → "1.500.000"
function format(n: number): string {
    if(!n) return ''
    return '$ ' + n.toLocaleString('es-CO')
}

// quita todo lo que no sea digito
function clean(s: string): number {
  const digits = s.replace(/\D/g, '')
  return digits ? parseInt(digits, 10) : 0
}
 
// inicializa con el valor que viene del padre
watch(() => props.modelValue, (val) => {
  if (!focused.value) {
    displayed.value = val ? format(val) : ''
  }
}, { immediate: true })
 
// cuenta cuántos dígitos hay antes de una posición dada del string
function digitsBefore(s: string, pos: number): number {
  return s.slice(0, pos).replace(/\D/g, '').length
}

// ubica la posición en `s` que deja exactamente `count` dígitos a la izquierda
function positionForDigitCount(s: string, count: number): number {
  if (count <= 0) return s.length > 0 ? 2 : 0 // justo después del prefijo "$ "
  let seen = 0
  for (let i = 0; i < s.length; i++) {
    if (/\d/.test(s.charAt(i))) {
      seen++
      if (seen === count) return i + 1
    }
  }
  return s.length
}

function onInput(e: Event) {
  const inputEl = e.target as HTMLInputElement
  const raw = inputEl.value
  const cursorPos = inputEl.selectionStart ?? raw.length
  const digitsBeforeCursor = digitsBefore(raw, cursorPos)

  const numeric = clean(raw)
  emit('update:modelValue', numeric)

  // reformatea mientras escribe
  const formatted = numeric ? format(numeric) : ''
  displayed.value = formatted

  // conserva la posición relativa del cursor en vez de moverlo siempre al final
  nextTick(() => {
    if (inputRef.value) {
      const pos = positionForDigitCount(formatted, digitsBeforeCursor)
      inputRef.value.setSelectionRange(pos, pos)
    }
  })
}
 
function onFocus() {
  focused.value = true
}
 
function onBlur() {
  focused.value = false
  displayed.value = props.modelValue ? format(props.modelValue) : ''
}
</script>