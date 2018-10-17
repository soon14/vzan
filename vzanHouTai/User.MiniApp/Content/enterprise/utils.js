var utils = {
    resizeimg: function (imgurl, w, h, mode, quality) {
        if (imgurl == null || imgurl == undefined || imgurl == "")
            return "";
        if (imgurl.indexOf("?") != -1)
            return imgurl;
        if (imgurl.indexOf("http://i.vzan.cc/") > -1||
            imgurl.indexOf("http://vzan-img.oss-cn-hangzhou.aliyuncs.com") > -1) {

            if (!mode) {
                mode = "fill";
            }
            imgurl = imgurl.replace("http://vzan-img.oss-cn-hangzhou.aliyuncs.com", "http://i.vzan.cc");
            imgurl += "?x-oss-process=image/resize,limit_0,m_" + mode + ",w_" + w + ",h_" + h + "/format,";
            imgurl += "gif";
            if (!quality) {
                quality = 80;
            }
            imgurl += "/quality,q_" + quality;
            return imgurl;
        }
        else {
            return imgurl;
        }
    },
    //查询微信订单号,传入vzan 对外订单号,返回微信相关资料集合
    getWxOrderMsgs: function (appId, orderNum)
    {
        var orderMsgs = [];
        if (orderNum.trim() == '') {
            layer.msg("请输入要查询的订单编号");
            return orderMsgs;
        }

        var loadindex = layer.load(1);
        $.ajax({
            url: "/common/FindWxOrderNum",
            type: "GET",
            async: false,
            data: {
                appId: appId,
                orderNum: orderNum
            },
            dataType: "JSON",
            success: function (data) {
                layer.close(loadindex);
                if (!data.isok) {
                    layer.msg(data.Msg);
                    return;
                } else {
                    layer.msg(data.Msg);
                    orderMsgs = data.dataObj;
                }
            },
            error: function()
            {
                layer.close(loadindex);
                layer.msg("网络异常或遇上未知错误！");
            }
        });
        return orderMsgs;
    },
    getSystemOrderMsgs: function (appId, orderNum) {
        var orderMsgs = [];
        if (orderNum.trim() == '') {
            layer.msg("请输入要查询的订单编号");
            return orderMsgs;
        }

        var loadindex = layer.load(1);
        $.ajax({
            url: "/common/FindSystemOrderNum",
            type: "GET",
            async: false,
            data: {
                appId: appId,
                wxPayNum: orderNum
            },
            dataType: "JSON",
            success: function (data) {
                layer.close(loadindex);
                if (!data.isok) {
                    layer.msg(data.Msg);
                    return;
                } else {
                    layer.msg(data.Msg);
                    orderMsgs = data.dataObj;
                }
            },
            error: function () {
                layer.close(loadindex);
                layer.msg("网络异常或遇上未知错误！");
            }
        });
        return orderMsgs;
    },
    //弹出查询框
    showFindWxOrderForm: function ()
    {
        $('#findWXOrder').modal('show');
    }
}