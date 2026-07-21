<template>
  <div class="landing">
    <LandingNavbar />
    <LandingHero />
    <LandingStatistics />
    <LandingFeatures />
    <LandingHowItWorks />
    <LandingScreenshots />
    <LandingTestimonials />
    <LandingFAQ />
    <LandingCTA />
    <LandingFooter />
  </div>
</template>

<script setup lang="ts">
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
</script>

<style scoped>
.landing { min-height: 100vh; }
</style>
