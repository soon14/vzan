// The Vue build version to load with the `import` command
// (runtime-only or standalone) has been set in webpack.base.conf with an alias.
import Vue from 'vue';
import axios from 'axios';

import router from './router';
import VueAxios from 'vue-axios'
import ElementUI from 'element-ui';
import scrollReveal from 'scrollreveal';
import io from 'vue-socket.io';
import "./assets/base.css";
import "./assets/dzicon.css";
import 'element-ui/lib/theme-chalk/index.css';
import App from './App';

Vue.config.productionTip = false
Vue.use(ElementUI)
Vue.use(io, 'http://kaaden.orrzt.com:8989')
Vue.use(VueAxios, axios)

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  components: {
    App
  },
  template: '<App/>',
  watch: {
    $route() {
      document.title = this.$route.meta.title
    }
  },
  data() {
    return {
      scrollReveal: scrollReveal(),
    }
  },
  mounted() {
    this.scrollReveal.reveal('.rv', {
      //动画的时长
      duration: 500,
      //延迟时间
      delay: 0,
      //动画开始的位置，'bottom', 'left', 'top', 'right'
      origin: 'bottom',
      //回滚的时候是否再次触发动画
      reset: true,
      //在移动端是否使用动画
      mobile: false,
      //滚动的距离，单位可以用%，rem等
      distance: '0',
      //其他可用的动画效果
      opacity: 0.8,
      easing: 'linear',
      scale: 0.5,
    });
  }
}).$mount('#app')
