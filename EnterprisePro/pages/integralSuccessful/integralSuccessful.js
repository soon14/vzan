const tools = require("../../utils/tools.js")
Page({

  data: {

  },

  onLoad: function (options) {

  },
  goBackHome: function () {
    tools.goLaunch("../index/index")
  },
  openIntegralOrder: function () {
    tools.goNewPage("../integralOrder/integralOrder")
  },



})