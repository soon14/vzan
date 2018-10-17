; (function () {
    $(function () {

        $("#findPsw-box").addClass("animated zoomIn")

        //第一步
        $("#phonenext").bind("click", function () {
            checknext();
        })
        function checknext() {
            $("#warning").css("display", "none");
            var phone = $("#phone").val();
            if (phone == '') {
                $("#warning").html("手机号不能为空");
                $("#warning").css("display", "block");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                $("#warning").html("请输入正确的手机号");
                $("#warning").css("display", "block");
                return;
            }

            var code = $("#code").val().trim();
            if (code == "") {
                $("#warning").html("请输入验证码");
                $("#warning").css("display", "block");
                return;
            }

            $.post("/dzhome/CheckVaildCode", { phonenum: phone, code: code }, function (data) {
                if (data.code == 1) {
                    $("#sline").removeClass("line-color01");
                    $("#sline").addClass("line-color02");

                    $("#sbkimg").removeClass("fpsw-step01");
                    $("#sbkimg").addClass("fpsw-step02");

                    $("#sfontcolor").addClass("fp-font-blue");

                    $("#fli").css("display", "none");
                    $("#sli").css("display", "");
                }
                else {
                    $("#warning").html(data.Msg);
                    $("#warning").css("display", "block");
                }
            })
        }

        //发送验证码
        $("#btnMsg").bind("click", function () {
            var id = this.id;
            sendValidCode(id);
        })
        function sendValidCode(id) {
            if (timer > 0) {
                return;
            }

            $("#warning").css("display", "none");
            var phone = $("#phone").val().trim();
            if (phone == '') {
                $("#warning").html("手机号不能为空");
                $("#warning").css("display", "block");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                $("#warning").html("请输入正确的手机号");
                $("#warning").css("display", "block");
                return;
            }

            $.post("/dzhome/GetVaildCode", { phonenum: phone, type: 0 }, function (data) {
                if (data.code == 1) {
                    layer.msg("发送成功");
                    $("#" + id).css("background-color", "#DCDCDC")
                    //启动计时器
                    setTimer(id, 1, 60);
                }
                else {
                    $("#warning").html(data.Msg);
                    $("#warning").css("display", "block");
                }
            })
        }

        var timerseconds;
        var timer = 0;
        function setTimer(id, seconde, initcount) {
            timerseconds = initcount;
            $("#" + id).html(timerseconds + "秒");
            timer = setInterval(function () {
                timerseconds -= seconde;
                $("#" + id).html(timerseconds + "秒");
                if (timerseconds < 0) {
                    $("#" + id).html("重新获取");
                    $("#" + id).css("background-color", "#3296FA")
                    timerseconds = initcount;
                    clearInterval(timer)
                    timer = 0;
                }
            }, seconde * 1000)
        }

        //第二步
        $("#resetpwdnext").bind("click", function () {
            resetpwdnext();
        })
        function resetpwdnext() {
            $("#swarning").css("display", "none");
            var password = $("#password").val();
            if (password == '') {
                $("#swarning").html("请输入密码");
                $("#swarning").css("display", "block");
                return;
            }

            var password2 = $("#password2").val().trim();
            if (password2 == "") {
                $("#swarning").html("重复密码不能为空");
                $("#swarning").css("display", "block");
                return;
            }
            if (password != password2) {
                $("#swarning").html("两次密码不一致，请重新输入");
                $("#swarning").css("display", "block");
                //alertmsg("两次密码不一致，请重新输入");
                return;
            }
            var code = $("#code").val().trim();
            var phone = $("#phone").val().trim();

            $.post("/dzhome/SaveUserInfo", { phone: phone, password: password, code: code, type: 0 }, function (data) {
                if (data.isok) {
                    $("#tbkimg").removeClass("fpsw-step01");
                    $("#tbkimg").addClass("fpsw-step02");

                    $("#tfontcolor").addClass("fp-font-blue");

                    $("#sli").css("display", "none");
                    $("#tli").css("display", "");

                    timerseconds = 5;
                    var id = "miao";
                    timer = setInterval(function () {
                        timerseconds -= 1;
                        $("#" + id)[0].innerHTML = timerseconds;
                        if (timerseconds <= 0) {
                            clearInterval(timer)
                            timer = 0;
                            window.location.href = "/dzhome/login";
                        }
                    }, 1000)
                }
                else {
                    $("#swarning").html(data.Msg);
                    $("#swarning").css("display", "block");
                }
            })
        }
    })

})()