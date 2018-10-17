import wepy from 'wepy'


export default class Enterprise extends wepy.mixin {
  data = {
    currentSkin: ''
  }
  async onShow() {
    await tools.setPageSkin(this);
  }
}
