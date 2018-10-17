import wepy from 'wepy';
import {
  core,
  http,
  tools
} from '@/lib/core';
import addr from '@/lib/addr';
import Validator from '@/lib/wxValidator'
const app = wepy.$instance;
export default class bingPhoneMixin extends wepy.mixin {
  data = {
    codeStatus: '获取验证码',
    isFocusList: [false, false],
    phone: '',
    code: ''
  }
  methods = {
    async getCode() {
      var vm = this,
        t = 60,   
        c;
        
      var reslutInfo = Validator.singleValid(vm.phone, 'phone', '请输入正确的手机号码格式')
        console.log(reslutInfo)
        if (!reslutInfo.result) {
          tools.showModalCancle(reslutInfo.msg)
          return;
        }
       
      //禁止多次点击
      if (vm.codeStatus != '获取验证码') return;
    
      vm.codeStatus = '正在发送中'
       
      //开始ajax
      var userInfo = await core.getUserInfo()

      http.post(addr.senduserauth, {
        tel: vm.phone,
        sendType: 8,
        appid: app.globalData.appid,
        openId: userInfo.OpenId
      }).then((res) => {
        tools.handleResult(res, async(res) => {
          await tools.freeToast('发送成功', 'success')
           //success
          c = setInterval(() => {
            vm.codeStatus = ('0' + --t).slice(-2) + 's'
            if (t == 0) {
              vm.codeStatus = '获取验证码'
              clearInterval(c)
            }
            vm.$apply()
          }, 1000);
        }, (err) => {
          //failure
          tools.showModalCancle(err.Msg)
          vm.codeStatus = '获取验证码'
          vm.$apply()
        })
    
      })

    },
    handleFocus(inputIndex) {
      this.isFocusList[inputIndex] = true
    },
    handleBlur(inputIndex) {
      this.isFocusList[inputIndex] = false
    },
    handlePhoneInput(e) {
      this.phone = e.detail.value
    },
    handleCodeInput(e) {
      this.code = e.detail.value
    }
  }
}
