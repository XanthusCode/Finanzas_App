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
import {ref, watch} from 'vue';

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
 
function onInput(e: Event) {
  const raw = (e.target as HTMLInputElement).value
  const numeric = clean(raw)
 
  emit('update:modelValue', numeric)
 
  // reformatea mientras escribe
  const formatted = numeric ? '$ ' + numeric.toLocaleString('es-CO') : ''
  displayed.value = formatted
 
  // mantiene el cursor al final
  setTimeout(() => {
    if (inputRef.value) {
      inputRef.value.setSelectionRange(formatted.length, formatted.length)
    }
  }, 0)
}
 
function onFocus() {
  focused.value = true
}
 
function onBlur() {
  focused.value = false
  displayed.value = props.modelValue ? format(props.modelValue) : ''
}
</script>