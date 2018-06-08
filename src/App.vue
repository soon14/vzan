<style>
  html {
    font-size: 0.8vw !important;
  }
  .bgapp {
    background-image: url("./assets/bg.jpg");
    background-size: cover;
    background-repeat: no-repeat;
    width: 100vw;
    height: 100vh;
  }
</style>

<template>
  <div id="app" :class="{'bgapp':routName=='login'}">
    <router-view></router-view>
  </div>
</template>

<script>
  export default {
    name: 'app',
    data() {
      return {
        routName: ''
      }
    },

    mounted() {
      this.routName=this.$route.name
    },
    updated () {
       this.routName=this.$route.name
    }
  }
</script>


