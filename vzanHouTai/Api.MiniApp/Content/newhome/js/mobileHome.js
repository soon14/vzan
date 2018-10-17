; (function () {
    $(function () {

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
        //选择类型
        var regType = 0;
        $("#regType>li").on("click", function () {
            $("#regType>li").removeClass("active");
            $(this).addClass("active");
            regType = $(this).attr("data-id");
        })
        //提交注册 
        $("#applyBtn").click(function () {
            event.stopPropagation();
            var source="代理分销推广";
            submitReg(source)
        })
        //返回首页
        $(".backtohome").click(function (event) {
            event.stopPropagation();
            $(window).attr('location', '/webview/mobileHome');
            $(".masking-box").hide();
            $(".regfinish-box").hide();
        })
        //提交注册
        function submitReg(from) {
            var username = $("#username").val().trim();
            var phone = $("#phone").val().trim();
            var password = $("#password").val();
            var code = $("#code").val();
            var address = $("#address").val();
            var opentype = regType;
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

            //if (address == "" && agentqrcodeid == 0) {
            //    layer.msg("请选择您所在的地区");
            //    return;
            //}
            $("#applyBtn").css("background-color", "#ddd");
            $(this).attr("disabled", true);
            $.post("/webview/SaveUserInfo",
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

            $.post("/webview/GetVaildCode", { phonenum: phone, type: 1, agentqrcodeid: agentqrcodeid }, function (data) {
                layer.msg(data.Msg)
            })
        }

    })
})()
