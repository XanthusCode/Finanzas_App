<template>
  <div ref="rootRef" class="landing">
    <LandingNavbar />
    <main>
      <LandingHero />
      <LandingStatistics />
      <LandingFeatures />
      <LandingHowItWorks />
      <LandingScreenshots />
      <LandingTestimonials />
      <LandingFAQ />
      <LandingCTA />
    </main>
    <LandingFooter />
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount } from 'vue'
import LandingNavbar from '@/components/landing/LandingNavbar.vue'
import LandingHero from '@/components/landing/LandingHero.vue'
import LandingStatistics from '@/components/landing/LandingStatistics.vue'
import LandingFeatures from '@/components/landing/LandingFeatures.vue'
import LandingHowItWorks from '@/components/landing/LandingHowItWorks.vue'
import LandingScreenshots from '@/components/landing/LandingScreenshots.vue'
import LandingTestimonials from '@/components/landing/LandingTestimonials.vue'
import LandingFAQ from '@/components/landing/LandingFAQ.vue'
import LandingCTA from '@/components/landing/LandingCTA.vue'
import LandingFooter from '@/components/landing/LandingFooter.vue'
import '@/assets/landing.css'

// Aparición suave de los bloques .reveal al entrar en pantalla
const rootRef = ref<HTMLElement>()
let observer: IntersectionObserver | null = null

onMounted(() => {
  const els = rootRef.value?.querySelectorAll<HTMLElement>('.reveal') ?? []
  if (!('IntersectionObserver' in window)) {
    els.forEach(el => el.classList.add('is-visible'))
    return
  }
  observer = new IntersectionObserver(entries => {
    for (const e of entries) {
      if (e.isIntersecting) {
        e.target.classList.add('is-visible')
        observer?.unobserve(e.target)
      }
    }
  }, { rootMargin: '0px 0px -10% 0px', threshold: 0.1 })
  els.forEach(el => observer!.observe(el))
})
onBeforeUnmount(() => observer?.disconnect())
</script>

