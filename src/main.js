// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue'
import App from './App'
import $ from 'jquery'
import axios from 'axios'
import router from './router'
import VueAxios from 'vue-axios'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.min'
import "./assets/base.css"

Vue.config.productionTip = false

Vue.use(VueAxios,axios)

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components: { App },
  template: '<App/>',
  watch:{
    $route(){
      document.title=this.$route.meta.title
    }
  }
}).$mount('#app')

