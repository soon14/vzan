const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
    data: {
        this_quan_list:null
    },
    onLoad: function () {
        var that = this;
    },
    go_home_bind:function(){
        wx.switchTab({
            url: '/pages/restaurant/restaurant-home/index',
            fail:function(){
                wx.navigateTo({
                    url: '/pages/restaurant/restaurant-home/index',
                })
            }
        })
    },
    onShow: function () {
        var that = this;
        //获取平台优惠券
        requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/ApiQuan/getQuanList.html', {}, (info) => {
            that.setData({ this_quan_list: info.qlist });
        });
    },
    //领取
    lingqu_action_bind:function(e){
        var that = this;
        let requestData = {};
        requestData.quan_id = e.target.dataset.id;
        requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/ApiQuan/lingquQuanAction.html',requestData, (info) => {
            wx.showModal({
                title: '提示',
                content: info,
                showCancel:false,
                success: function (res) {
                    that.onShow();
                }
            })
        });
    },
    onShareAppMessage: function () {
        var that = this;
        var shareTitle = '抢超值好券，享全场优惠';
        var shareDesc = '';
        var sharePath = 'pages/restaurant/get-redbag/index';
        return {
            title: shareTitle,
            desc: shareDesc,
            path: sharePath
        }
    },
})