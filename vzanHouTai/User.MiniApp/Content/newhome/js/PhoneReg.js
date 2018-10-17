;(function(){
    $(function () {
        $("#phoneReg-box").on("mouseenter", function () {
            $(this).addClass("animated pulse")

        })
        $("#phoneReg-box").on("mouseleave", function () {
            $(this).removeClass("animated pulse")
        })


        //发送验证码
        $("#btnMsg").on("click", function () {
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

            $.post("/dzhome/GetVaildCode", { phonenum: phone, type: 1 }, function (data) {
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

        //确定
        $("#saveBtn").on("click", function () {
            save()
        })
        function save() {
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
            var code = $("#code").val().trim();
            if (code == '') {
                $("#warning").html("请输入验证码");
                $("#warning").css("display", "block");
                return;
            }

            var password = $("#psw").val().trim();
            if (password == '') {
                $("#warning").html("请输入密码");
                $("#warning").css("display", "block");
                return;
            }
            if (password.length < 6) {
                $("#warning").html("密码长度不对");
                $("#warning").css("display", "block");
                return;
            }

            var password2 = $("#repsw").val().trim();
            if (password2 == "") {
                $("#warning").html("重复密码不能为空");
                $("#warning").css("display", "block");
                return;
            }
            if (password != password2) {
                $("#warning").html("两次密码不一致，请重新输入");
                $("#warning").css("display", "block");
                return;
            }


            $.post("/dzhome/SaveUserInfo", { phone: phone, password: password, code: code, type: 1 }, function (data) {
                if (data.isok) {
                    window.location.href = "/dzhome/login";
                }
                else {
                    $("#warning").html(data.Msg);
                    $("#warning").css("display", "block");
                }
            })
        }
    })
})()
