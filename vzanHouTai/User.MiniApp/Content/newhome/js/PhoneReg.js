;(function(){
    $(function () {
        $("#phoneReg-box").on("mouseenter", function () {
            $(this).addClass("animated pulse")

        })
        $("#phoneReg-box").on("mouseleave", function () {
            $(this).removeClass("animated pulse")
        })


        //������֤��
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
                $("#warning").html("�ֻ��Ų���Ϊ��");
                $("#warning").css("display", "block");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                $("#warning").html("��������ȷ���ֻ���");
                $("#warning").css("display", "block");
                return;
            }

            $.post("/dzhome/GetVaildCode", { phonenum: phone, type: 1 }, function (data) {
                if (data.code == 1) {
                    layer.msg("���ͳɹ�");
                    $("#" + id).css("background-color", "#DCDCDC")
                    //������ʱ��
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
            $("#" + id).html(timerseconds + "��");
            timer = setInterval(function () {
                timerseconds -= seconde;
                $("#" + id).html(timerseconds + "��");
                if (timerseconds < 0) {
                    $("#" + id).html("���»�ȡ");
                    $("#" + id).css("background-color", "#3296FA")
                    timerseconds = initcount;
                    clearInterval(timer)
                    timer = 0;
                }
            }, seconde * 1000)
        }

        //ȷ��
        $("#saveBtn").on("click", function () {
            save()
        })
        function save() {
            $("#warning").css("display", "none");
            var phone = $("#phone").val().trim();
            if (phone == '') {
                $("#warning").html("�ֻ��Ų���Ϊ��");
                $("#warning").css("display", "block");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                $("#warning").html("��������ȷ���ֻ���");
                $("#warning").css("display", "block");
                return;
            }
            var code = $("#code").val().trim();
            if (code == '') {
                $("#warning").html("��������֤��");
                $("#warning").css("display", "block");
                return;
            }

            var password = $("#psw").val().trim();
            if (password == '') {
                $("#warning").html("����������");
                $("#warning").css("display", "block");
                return;
            }
            if (password.length < 6) {
                $("#warning").html("���볤�Ȳ���");
                $("#warning").css("display", "block");
                return;
            }

            var password2 = $("#repsw").val().trim();
            if (password2 == "") {
                $("#warning").html("�ظ����벻��Ϊ��");
                $("#warning").css("display", "block");
                return;
            }
            if (password != password2) {
                $("#warning").html("�������벻һ�£�����������");
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
