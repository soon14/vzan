import Vue from 'vue'
import Router from 'vue-router'
import Login from '@/components/login'
import Index from "@/components/index"
Vue.use(Router)

export default new Router({
  routes: [
    {
      path: '/',
      name: 'login',
      component: Login,
      meta: {
        title: 'Kaaden后台系统'
      }
    },
    {
      path: '/index',
      name: 'index',
      component: Index,
      meta: {
        title: '首页'
      }
    }
  ]
})
