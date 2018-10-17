// pages/sellCenter/record.js
const app = getApp();
const util = require("../../utils/util.js");
const tools = require("../../utils/tools.js");
const page = require("../../utils/pageRequest.js");
Page({

  /**
   * 页面的初始数据
   */
  data: {
    vmCount: {
      list: [],
      ispost: false,
      loadall: false,
      pageIndex: 1,
      pageSize: 10,
    },
  },

  /**
   * 生命周期函数--监听页面加载
   */
  onLoad: function (options) {
    let that = this
    app.getUserInfo(res => {
      that.recordList()
      util.setPageSkin(that)
    })

  },
  // 记录申请
  recordList: function () {
    let that = this
    let vmCount = that.data.vmCount
    if (vmCount.ispost || vmCount.loadall)
      return;
    if (!vmCount.ispost)
      that.setData({ "vmCount.ispost": true });
    page.cashList(vmCount).then(data => {
      vmCount.list = []
      vmCount.ispost = false
      if (data.isok) {
        for (let i = 0; i < data.obj.list.length; i++) {
          if (data.obj.list[i].state == -1) {
            data.obj.list[i].recordState = "审核不通过"
          }
          else if (data.obj.list[i].state == 0) {
            data.obj.list[i].recordState = "待审核"
          }
          else {
            if (data.obj.list[i].drawState == -1) {
              data.obj.list[i].recordState = "提现失败"
            }
            else if (data.obj.list[i].drawState == 0) {
              data.obj.list[i].recordState = "等待提现中"
            }
            else if (data.obj.list[i].drawState == 1) {
              data.obj.list[i].recordState = "提现中"
            }
            else {
              data.obj.list[i].recordState = "提现成功"
            }
          }
        }
        data.obj.totalCount >= vmCount.pageIndex ? vmCount.pageIndex += 1 : vmCount.loadall = true;
        data.obj.totalCount > 0 ? vmCount.list = vmCount.list.concat(data.obj.list) : "";

        that.setData({ vmCount })
      } else {
        tools.ShowMsg(data.msg)
      }

    })
  },

  /**
   * 页面上拉触底事件的处理函数
   */
  onReachBottom: function () {
    this.recordList()
  },

})