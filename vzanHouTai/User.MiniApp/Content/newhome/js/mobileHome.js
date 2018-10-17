; (function () {
    $(function () {
        var sourceUrl = window.location.search.split("&")[0];
        var value = sourceUrl.substring(12);
        var param = sourceUrl.slice(0, 12);
        var urlenter = param === "?utm_source=" && value?value:"";
        if ($.cookie("mobileSource")) {
            if (urlenter != "" && urlenter != $.cookie("mobileSource")) {
                $.cookie("mobileSource", urlenter, { path: '/' });
            }
        } else {
            if (urlenter != "") {
                $.cookie("mobileSource", urlenter, { path: '/' });
            }
        }

        //播放视频
        $("#btnPlay").click(function () {
            $(".video-box>video").attr("src", "http://mp4-vod.oss-cn-hangzhou.aliyuncs.com/dzan.mp4")
            $("#videoMasking").hide();
            $("video").trigger('play');
        })

        $("#addwxBtn").click(function (event) {
            $(".masking-box").show();
            $(".dswx-box").fadeIn();
            $("html,body").css("overflow", "hidden")
            document.addEventListener('touchmove', function (e) { e.preventDefault(); }, false);
            event.stopPropagation();
        })
        $("#callBtn").click(function (event) {
            event.stopPropagation();
        })
        $("#freeReg").click(function (event) {
            $(window).attr('location', '/mobile/mobileReg');
            event.stopPropagation();
        })
        $(".code-close").click(function (event) {
            $(".masking-box").fadeOut();
            $(".dswx-box").fadeOut();
            $("html,body").css("overflow", "");
            document.addEventListener('touchmove', function (e) { e.returnValue = true; }, false);
            event.stopPropagation();
        })


        $("#protocolEnter").click(function (event) {
            $(".protocol-box").fadeIn();
            $("html,body").css("overflow", "hidden")
            event.stopPropagation();
        })
        $(".protocol-close>img").click(function (event) {
            $(".protocol-box").fadeOut();
            $("html,body").css("overflow", "");
            event.stopPropagation();
        })


        //获取验证码
        $("#getCodeBtn").click(function () {
            sendValidCode();
        })

        //提交注册
        $("#applyBtn").click(function () {
            event.stopPropagation();
            var source;
            if ($.cookie("mobileSource") == "" || $.cookie("mobileSource") == null) {
                source = "自然搜索";
            } else {
                source = $.cookie("mobileSource");
            }

            submitReg(source)
        })
        //返回首页
        $(".backtohome").click(function (event) {
            event.stopPropagation();
            $(window).attr('location', '/mobile/mobileHome');
            $(".masking-box").hide();
            $(".regfinish-box").hide();
        })
        $("#btnSubmit").click(function () {
            var inputNum = 1;
            submitInformation(1, inputNum);
        })
        $("#btnSubmit2").click(function () {
            var inputNum = 2;
            submitInformation(1, inputNum);
        })
        //提交咨询
        function submitInformation(type, inputNum) {

            if (inputNum == 1) {
                var name = $("#name").val();
                var phone = $("#phoneNum").val().trim();
            } else if (inputNum == 2) {
                var name = $("#name2").val();
                var phone = $("#phoneNum2").val().trim();
            }
            var source;
            if ($.cookie("mobileSource") == "" || $.cookie("mobileSource") == null) {
                source = "自然搜索";
            } else {
                source = $.cookie("mobileSource");
            }
            var userName = name + '-' + source;
            var typeValue = type;

            if (name == "" || phone == "") {
                layer.msg("请填写完整信息！");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                layer.msg("请输入正确的手机号");
                return;
            }

            $.post("/DLPT/SendUserAdvisory",
               { Phone: phone, username: userName, source: typeValue },
               function (data) {
                   if (data.isok) {
                       layer.msg(data.Msg);
                       $("#name,#name2").val("");
                       $("#phoneNum,#phoneNum2").val("");
                   }
               })
        }
        //提交注册
        function submitReg(from) {
            var username = $("#username").val().trim();
            var phone = $("#phone").val().trim();
            var password = $("#password").val();
            var code = $("#code").val();
            var address = $("#address").val();
            var opentype = $("#opentype").attr("data-id");
            var sourcefrom = from

            //代理分销
            var matcharray = window.location.search.substr(1).match(new RegExp("(^|&)agentqrcodeid=([^&]*)(&|$)", "i"));
            var agentqrcodeid = 0;
            if (matcharray != null && matcharray != undefined) {
                agentqrcodeid = matcharray[2];
            }

            //if (agentqrcodeid>0 && username == "") {
            //    layer.msg("请输入姓名");
            //    return;
            //}
            if (phone == "") {
                layer.msg("请输入手机号码");
                return;
            }
            if (code == "") {
                layer.msg("请输入验证码");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                layer.msg("请输入正确的手机号码");
                return;
            }

            if (agentqrcodeid > 0 && opentype < 0) {
                layer.msg("请选择类型");
                return;
            }

            if (address == "" && agentqrcodeid == 0) {
                layer.msg("请选择您所在的地区");
                return;
            }
            $("#applyBtn").css("background-color", "#ddd");
            $(this).attr("disabled", true);
            $.post("/mobile/SaveUserInfo",
                { phone: phone, password: password, code: code, address: address, sourcefrom: sourcefrom, agentqrcodeid: agentqrcodeid, opentype: opentype, username: username },
                function (data) {
                    if (data.isok) {
                        $("#phone").val("");
                        $("#password").val("");
                        $("#code").val("");
                        $("#username").val("");
                        $("#address").val("");
                        if (agentqrcodeid <= 0) {
                            $(".masking-box").fadeIn();
                            $(".regfinish-box").fadeIn();
                        } else {
                            layer.msg("注册成功");
                        }
                        //$(window).attr('location', 'http://dz.vzan.com/mobile/mobileHome');
                    } else {
                        layer.msg(data.Msg);
                        $("#applyBtn").css("background-color", "");
                        $("#applyBtn").attr("disabled", false);
                    }
                })

        }
        //发送验证码
        function sendValidCode() {

            var phone = $("#phone").val().trim();
            if (phone == '') {
                layer.msg("手机号不能为空");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                layer.msg("请输入正确的手机号");
                return;
            }

            //代理分销
            var matcharray = window.location.search.substr(1).match(new RegExp("(^|&)agentqrcodeid=([^&]*)(&|$)", "i"));
            var agentqrcodeid = 0;
            if (matcharray != null && matcharray != undefined) {
                agentqrcodeid = matcharray[2];
            }

            $.post("/dzhome/GetVaildCode", { phonenum: phone, type: 1, agentqrcodeid: agentqrcodeid }, function (data) {
                layer.msg(data.Msg)
            })
        }

        //$(window).load(function () {
        //    var str = "<script>" +
        //    "var _hmt = _hmt || [];" +
        //    "(function() {" +
        //        "var hm = document.createElement('script');" +
        //        "hm.src = 'https://hm.baidu.com/hm.js?6d893fca3f83f3d64ee95af3a1aaa832';" +
        //        "var s = document.getElementsByTagName('script')[0]; " +
        //        "s.parentNode.insertBefore(hm, s);" +
        //    "})();" +
        //    "</script>"
        //    $("script:last").after(str)
        //})

    })
})()
