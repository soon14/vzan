const app = getApp();
const requestUtil = require('../../../utils/requestUtil');
const _DuoguanData = require('../../../utils/data');
Page({
    data: {
        rz_config: [],
        submitIsLoading: false,
        buttonIsDisabled: false,
        img_count_limit: 5,
        this_img_i: 0,
        this_img_max: 0,
        postimg: [],
    },
    onLoad: function () {

    },
    onShow: function () {
        var that = this;
        requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/getRuzhuConfig', {}, (info) => {
            console.log(info.config)
            that.setData({ rz_config: info.config });
        }, this, {});
    },
    formSubmit: function (e) {
        var that = this;
        that.setData({ submitIsLoading: true, buttonIsDisabled: true });
        var rdata = e.detail.value;
        requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/dishRuzhu', rdata, (info) => {
            that.submit_upload_pic(info);
        }, this, { isShowLoading: true, completeAfter: function () { that.setData({ submitIsLoading: false, buttonIsDisabled: false }); } });
    },
    //删除
    del_pic_bind: function (e) {
        var that = this
        var index = e.currentTarget.id;
        var datas = that.data.postimg;
        datas.splice(index, 1)
        that.setData({
            postimg: datas
        })
    },
    //上传图片
    chooseimg_bind: function () {
        var that = this
        var img_lenth = that.data.postimg.length
        var sheng_count = that.data.img_count_limit - img_lenth
        if (sheng_count <= 0) {
            wx.showModal({
                title: '提示',
                content: '对不起，最多可上传五张图片',
                showCancel: false
            })
            return false
        }
        wx.chooseImage({
            count: sheng_count,
            sizeType: ['compressed'], // 可以指定是原图还是压缩图，默认二者都有
            sourceType: ['album', 'camera'], // 可以指定来源是相册还是相机，默认二者都有
            success: function (res) {
                // 返回选定照片的本地文件路径列表，tempFilePath可以作为img标签的src属性显示图片
                that.setData({
                    postimg: that.data.postimg.concat(res.tempFilePaths)
                })
            }
        })
    },
    //上传图片
    submit_upload_pic: function (ruzhu_id) {
        var that = this;
        var g_data = that.data.postimg;
        if (g_data.length > 0) {
            that.setData({ this_img_max: g_data.length });
            wx.showToast({
                title: '图片上传中',
                icon: 'loading',
                duration: 10000
            })
            that.imgUploadTime(ruzhu_id);
        } else {
            that.go_pay_bind(ruzhu_id);
        }
    },
    imgUploadTime: function (ruzhu_id) {
        var that = this;
        var this_img_len = that.data.this_img_i;
        var this_img_max_len = that.data.this_img_max;
        if (this_img_len < this_img_max_len) {
            var requsetData = {};
            requsetData.ruzhu_id = ruzhu_id;
            requestUtil.upload(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/ruzhuImgUpload', that.data.postimg[this_img_len], 'file', requsetData, (info) => {
                that.setData({ this_img_i: that.data.this_img_i + 1 });
                that.imgUploadTime(ruzhu_id);
            }, this, {});
        } else {
            that.setData({ postimg: [], this_img_i: 0, this_img_max: 0 });
            wx.hideToast();
            //进行支付
            that.go_pay_bind(ruzhu_id);
        }
    },
    //支付
    go_pay_bind: function (ruzhu_id) {
        var that = this;
        if (that.data.rz_config.dish_rz_jiner <= 0) {
            wx.showModal({
                title: '提示',
                content: "入驻提交成功，请等待客服与您联系",
                showCancel: false,
                success: function (res) {
                    wx.switchTab({
                        url: '/pages/restaurant/restaurant-home/index'
                    })
                }
            });
        } else {
            requestUtil.get(_DuoguanData.duoguan_host_api_url + '/index.php/addon/DuoguanDish/Api/makeRuzhuPay', { ruzhu_id: ruzhu_id}, (info) => {
                wx.requestPayment({
                    'timeStamp': info.timeStamp,
                    'nonceStr': info.nonceStr,
                    'package': info.package,
                    'signType': 'MD5',
                    'paySign': info.paySign,
                    'success': function (res) {
                        wx.showModal({
                            title: '提示',
                            content: "入驻提交成功，请等待客服与您联系",
                            showCancel: false,
                            success: function (res) {
                                wx.switchTab({
                                    url: '/pages/restaurant/restaurant-home/index'
                                })
                            }
                        });
                    },
                    'fail': function (res) {
                        wx.showModal({
                            title: '提示',
                            content: "支付失败",
                            showCancel: false,
                            success: function (res) {
                                
                            }
                        });
                    }
                });
            }, this, {});
        }
    },
})