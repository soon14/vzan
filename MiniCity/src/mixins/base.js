import wepy from "wepy";
export default class baseMixin extends wepy.mixin {
  data = {
    $toast: {
      show: false,
      msg: ""
    }
  };
  methods = {
    
  };
  ShowToast(msg) {
    let that = this;
    that.$toast.show = true;
    that.$toast.msg = msg;
    that.$apply();
    setTimeout(() => {
      that.$toast.show = false;
      that.$apply();
    }, 2000);
  }
}
