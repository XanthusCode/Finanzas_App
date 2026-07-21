import { ref, computed, onMounted, onUnmounted } from 'vue'

export type DeviceType = 'mobile' | 'tablet' | 'desktop'

const MOBILE_MAX = 767
const TABLET_MAX = 1024

export function useDeviceType() {
  const width = ref(typeof window !== 'undefined' ? window.innerWidth : 0)

  function onResize() {
    width.value = window.innerWidth
  }

  onMounted(() => window.addEventListener('resize', onResize))
  onUnmounted(() => window.removeEventListener('resize', onResize))

  const deviceType = computed<DeviceType>(() => {
    if (width.value <= MOBILE_MAX) return 'mobile'
    if (width.value <= TABLET_MAX) return 'tablet'
    return 'desktop'
  })

  const isMobile  = computed(() => deviceType.value === 'mobile')
  const isTablet  = computed(() => deviceType.value === 'tablet')
  const isDesktop = computed(() => deviceType.value === 'desktop')

  return { width, deviceType, isMobile, isTablet, isDesktop }
}
