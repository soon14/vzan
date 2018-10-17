; (function () {

    var vl = new Vue({
        el: "#videoList",
        data: {
            labelNum: 0,
            themeid: 0,
            sortNum: 0,
            //isClickSort: false,
            sortType: 0,
            inputValue: "",
            videoLabel: {
                title: "课程类别",
                arr: ["全部", "公开课", "内训", "小视频"]
            },
            videoTheme: {
                title: "主题",
                arr: [{ Name: "全部" }]
            },
            videoSort: ['默认', '按时间'],
            videoArr: [],
            videoListParam: {
                typeid: -1,  //课程类
                themeid: -1, //主题 -1全部
                orderbytype: 0,//0默认，1热门，2时间
                orderby: -1, //升序0、降序1，-1或""为后台排序
                pageIndex: 1,
                pageSize: 8
            },
            url: "/dzhome/videoDetails/?",
            moreVideoToggle: true,
            moreBtnText: "",
        },
        mounted() {
            this.getThemeList();
            this.getVideoList();
        },
        methods: {
            toggleLabel: function (i) {
                vl.videoArr = [];
                vl.moreVideoToggle = true;
                vl.labelNum = i;
                vl.videoListParam.typeid = i == 0 ? -1 : i - 1; //切换课程分类
                vl.videoListParam.pageIndex = 1;
                vl.getVideoList();

            },
            toggleTheme: function (i) {
                vl.themeid = i;
                vl.videoArr = [];
                vl.moreVideoToggle = true;
                //i == 0 ? vl.videoListParam.themeid = -1 : vl.videoListParam.themeid = i - 1;
                vl.videoListParam.themeid = i == 0 ? -1 : vl.videoTheme.arr[i].Id;
                vl.videoListParam.pageIndex = 1;
                vl.getVideoList();
            },
            toggleSort: function (i) {
                vl.sortNum = i;
                vl.videoArr = [];
                vl.moreVideoToggle = true;
                vl.videoListParam.pageIndex = 1;
                vl.moreVideoToggle = true;
                i == 0 ? vl.videoListParam.orderbytype = -1 : vl.videoListParam.orderbytype = i + 1;
                vl.getVideoList();
            },
            getThemeList: function () {
                $.ajax({
                    type: "POST",
                    url: "/dzhome/GetVideoPlayThemeList",
                    success: function (res) {
                        vl.videoTheme.arr = vl.videoTheme.arr.concat(res.dataObj);
                    }
                })
            },
            getVideoList: function () {
                var that = this;
                $.ajax(
                    {
                        type: "POST",
                        url: "/dzhome/GetVideoPlayList ",
                        data: {
                            typeid: that.videoListParam.typeid,
                            themeid: that.videoListParam.themeid,
                            orderbytype: that.videoListParam.orderbytype,
                            // orderby: that.videoListParam.orderby,
                            pageIndex: that.videoListParam.pageIndex,
                            pageSize: that.videoListParam.pageSize,
                        },
                        success: function (res) {
                            //console.log(res.data);
                            if (res != undefined && res != null) {
                                if (res.isok) {
                                    if (vl.videoListParam.pageIndex > 1 && res.data == "") {
                                        vl.moreVideoToggle = false;
                                        vl.moreBtnText = "视频已全部加载"
                                    } else if (vl.videoListParam.pageIndex == 1 && res.data == "") {
                                        vl.moreVideoToggle = false;
                                        vl.moreBtnText = "暂无内容"
                                    }
                                    vl.videoArr = vl.videoArr.concat(res.data);
                                    //console.log(vl.videoArr)
                                }
                            }
                        },
                        error: function (err) {
                            console.log("GetVideoPlayList:err");
                        }
                    })
            },
            addMoreVideo: function () {
                vl.videoListParam.pageIndex++;
                vl.getVideoList();

            },
            addUrlParam: function (index) {
                vl.url = "/dzhome/videoDetails?";
                var videoSrc = vl.videoArr[index]
                var name = encodeURI(videoSrc.Title) //对中文字符进行编码
                var date = videoSrc.PlayTimeStr;
                var isPassword = videoSrc.IsPassword;
                var id = videoSrc.Id;
                vl.url = vl.url + "id=" + id + "&isPassword=" + isPassword + "&name=" + name + "&date=" + date + "&v=" + videoSrc.PlayUrl;
                window.open(vl.url)
            },
            invitedLecture: function () {
                var name = "邀请讲课";
                var phone = this.inputValue;

                if (phone == "") {
                    layer.msg("请输入手机号");
                    return;
                }

                $.post("/DLPT/SendUserAdvisory",
                    { Phone: phone, username: name, },
                    function (data) {
                        if (data.isok) {
                            layer.msg("提交成功");
                            vl.inputValue = "";
                        } else {
                            layer.msg(data.Msg)
                        }
                    })
            }
        }
    })
})()
