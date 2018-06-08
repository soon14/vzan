<style scoped>
  .m-left {
    width: 15vw;
    height: 100vh;
  }
  .m-title {
    width: 100%;
    height: 88px;
    line-height: 88px;
    background-color: #3b77e3;
  }
  .m-title>img{
    width:20px;
    height:20px;
  }
  .m-logo {
    font-size: 20px;
    font-weight: bold;
    margin-bottom: 40px;
    color: #fff;
    font-family: 'beyno';
  }
  .m-right {
    width: 85vw;
    height: 100vh;
  }
</style>

<template>
  <div class="main f">
    <section class="m-left">
      <div class="m-title">
        <img src='../../static/logo.jpg'>
      </div>
    </section>
    <section class="m-right">
      <div class="m-title" style="opacity:0.8">
      </div>
    </section>
  </div>
</template>

<script>
  export default {
    name: 'index',
    data() {
      return {}
    }
  }
</script>

<!-- Add "scoped" attribute to limit CSS to this component only -->

