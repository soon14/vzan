; (function () {
    $(function () {
        $("#login-box").addClass("animated zoomIn")

        var method = $("#method").val();
        if (method == 0) {

        }
        else if (method == 1) {
            $("#login-qrcode").addClass("login-font-blue title-line");
            $("#login-num").removeClass("login-font-blue title-line");
            $("#num").hide();
            $("#qrcode").show();
        }

        $("#login-qrcode").on("click", function () {
            $(this).addClass("login-font-blue title-line");
            $("#login-num").removeClass("login-font-blue title-line");
            $("#num").hide();
            $("#qrcode").show();
        });
        $("#login-num").on("click", function () {
            $(this).addClass("login-font-blue title-line");
            $("#login-qrcode").removeClass("login-font-blue title-line");
            $("#qrcode").hide();
            $("#num").show();
        })
        //��½
        $("#login").on("click", function () {
            ValidateLogins()
        })
        //enter����½
        $("#bodykey").on("keyup", function () {
            keyLogin()
        })

        function keyLogin() {
            if (event.keyCode == 13)  //�س����ļ�ֵΪ13
                ValidateLogins(); //���õ�¼��ť�ĵ�¼�¼�
        }
        //ɨ���¼
        function wxLogin() {
            var key = $('#sessid').val();
            $.ajax({
                type: "POST",
                url: "/dzhome/wxlogin",
                xhrFields: {
                    withCredentials: true
                },
                data: { wxkey: key },
                success: function (returnData) {
                    if (returnData != "-1") {
                        if (returnData == "-2") {
                            window.location.href = "/dzhome/wxreg";
                        }
                        else {
                            if (location.host == "www.vzan.com") {
                            } else {
                                //������̨
                                window.location.href = returnData;
                            }
                        }
                    }
                }
            });
        }
        setInterval(wxLogin, 1000);

        function ValidateLogins() {
            var userName = $("#user_name").val();
            var passWord = $("#user_pwd").val();
            $("#warning").css("opacity", "0");
            if (userName == "") {
                $("#warning").html("�˺Ų���Ϊ��");
                $("#warning").css("opacity", "1");
                return;
            }
            if (passWord == "") {
                $("#warning").html("���벻��Ϊ��");
                $("#warning").css("opacity", "1");
                return;
            }

            $.ajax({
                url: "/dzhome/DZLoginAjax",
                type: "post",
                data: { userName: userName, passWord: passWord, isKeep: true, backurl: "" },
                dataType: "json",
                success: function (data) {
                    if (data.success) {
                        //$.cookie("dz_UserCookieNew", data.msg, {
                        //    expires: 7,
                        //    path: '/',
                        //    domain: '.vzan.com'
                        //});
                        var url = window.location.host;
                        var domain = ".vzan.com";
                        if (url != "dz.vzan.com") {
                            domain = '.xiaochengxu.com.cn';
                        }
                        $.cookie("dz_UserCookieNew", data.msg, {
                            expires: 7,
                            path: '/',
                            domain: domain
                        });

                        window.location.href = "/dzhome/casetemplate?hosttype=" + domain;
                    }
                    else {
                        if (data.code == 1) {
                            $("#warning").html("��������˺Ų�����");
                            $("#warning").css("opacity", "1");
                        }
                        else if (data.code == 2) {
                            $("#warning").html("��������������˺Ų�ƥ��");
                            $("#warning").css("opacity", "1");
                        }
                    }
                }
            });
        }
    })
})()
