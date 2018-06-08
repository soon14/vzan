<style>
  .login {
    width: 41.6vw;
    height: 41.6vw;
    position: absolute;
    top: 50%;
    left: 50%;
    margin-left: -20.8vw;
    margin-top: -20.8vw;
  }
  .title {
    font-size: 70px;
    font-weight: bold;
    margin-bottom: 40px;
    color: #fff;
    font-family: 'beyno';
  }
  form {
    width: 32.5rem;
    height: 32.5rem;
    background: #fff;
    border-radius: 8px;
    padding: 40px 0;
  }
  .form-title {
    font-family: 'beyno';
  }
  .forminput {
    position: relative;
    width: 100%;
    padding: 0 50px;
  }
  .f-label {
    position: absolute;
    top: 5px;
    left: 50px;
    font-size: 16px;
    color: #9e9e9e;
    transition: top 0.2s ease;
    -webkit-transition: top 0.2s ease;
  }
  .f-name {
    width: 100%;
    height: 33px;
    border: none;
    border-bottom: 1px solid #9e9e9e;
  }
  .f-name:focus+.f-label {
    top: -25px;
    font-size: 14px;
    color: #26a69a;
  }
  .f-name:focus {
    border-bottom: 2px solid #26a69a;
  }
  .forminput2 {
    position: relative;
    width: 100%;
    padding: 0 50px;
  }
  .f-password {
    width: 100%;
    height: 33px;
    border: none;
    border-bottom: 1px solid #9e9e9e;
  }
  .f-label2 {
    position: absolute;
    top: 5px;
    left: 50px;
    font-size: 16px;
    color: #9e9e9e;
    transition: top 0.2s ease;
    -webkit-transition: top 0.2s ease;
  }
  .f-password:focus+.f-label2 {
    top: -25px;
    font-size: 14px;
    color: #26a69a;
  }
  .f-password:focus {
    border-bottom: 2px solid #26a69a;
  }
  .show {
    top: -25px !important;
    font-size: 14px;
    color: #26a69a;
  }
  .showbottom {
    border-bottom: 2px solid #26a69a;
  }
  .formbtn {
    width: 80%;
    height: 44px;
    line-height: 44px;
    text-align: center;
    background-color: rgb(56, 153, 256);
    color: #fff;
    font-size: 22px;
    border: none;
    font-family: 'beyno';
  }
  .f-error {
    font-size: 14px;
    color: #f20033;
  }
  .f-account {
    position: absolute;
    font-size: 12px;
    top: 7px;
    right: 55px;
    color: #f20033;
  }
</style>

<template>
  <div class="Login">
    <div class="login f fv fc">
      <div class="title f fc">
        <span>Kaa</span>
        <span class="c1a">d</span>
        <span>en</span>
      </div>
      <form class="form-horizontal f fv fc fj" role="form" @submit.prevent="submit">
        <h1 class="form-title">Log in</h1>
        <div class="forminput">
          <input class="f-name" id='username' v-model='formdata.name' :class="{'showbottom':formdata.name!=''}" @input="useName">
          <label class="f-label" for='username' :class="{'show':formdata.name!=''}">userName</label>
          <label class="f-account" v-if='checkname'>请输入账号</label>
        </div>
        <div class="forminput2">
          <input class="f-password" id='password' v-model='formdata.password' type='password' :class="{'showbottom':formdata.password!=''}" @input="usePassword">
          <label class="f-label2" for='password' :class="{'show':formdata.password!=''}">password</label>
          <label class="f-account" v-if='checkpass'>请输入密码</label>
        </div>
        <p class="f-error" v-if="check">{{tip}}</p>
        <input class="formbtn" type="submit" value="login">
      </form>
    </div>
  </div>
</template>

<script>
  export default {
    name: 'login',
    data() {
      return {
        formdata: {
          name: '',
          password: '',
        },
        checkname: false,
        checkpass: false,
        tip: '',
        check: false,
      }
    },
    methods: {
      useName(e) {
        this.checkname = false
        this.check = false
      },
      usePassword(e) {
        this.checkpass = false
        this.check = false
      },
      submit() {
        let that = this
        let _g = that.formdata
        console.log(that.formdata)
        if (_g.name) {
          that.checkname = false
        } else {
          that.checkname = true
          return;
        }
        if (_g.password) {
          that.checkpass = false
        } else {
          that.checkpass = true
          return;
        }
        let param = new FormData(); //创建form对象  
        param.append('name', _g.name); //通过append向form对象添加数据  
        param.append('password', _g.password);
        let config = {
          headers: {
            'Content-Type': 'multipart/form-data'
          }
        }; //添加请求头  
        that.axios.post('http://127.0.0.1:8081/login', param, config)
          .then(res => {
            if (res.data.isok) {
              that.check = false
              that.$router.push('/index')
            } else {
              that.tip = res.data.msg
              that.check = true
            }
          })
      }
    }
  }
</script>


