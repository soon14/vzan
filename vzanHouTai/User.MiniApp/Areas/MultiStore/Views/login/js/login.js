$(function () {
    $("#login-box").addClass("animated zoomIn")

    var method = $("#method").val();
    if (method == 0)
    {

    }
    else if (method == 1)
    {
        //$("#login-qrcode").addClass("login-font-blue title-line");
        //$("#login-num").removeClass("login-font-blue title-line");
        $("#num").hide();
        $("#qrcode").show();
    }

    //$("#login-qrcode").on("click", function () {
        //$(this).addClass("login-font-blue title-line");
        //$("#login-num").removeClass("login-font-blue title-line");
    //    $("#num").hide();
    //    $("#qrcode").show();
    //});
    //$("#login-num").on("click", function () {
        //$(this).addClass("login-font-blue title-line");
        //$("#login-qrcode").removeClass("login-font-blue title-line");
    //    $("#qrcode").hide();
    //    $("#num").show();
    //})
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
                    if (location.host == "www.vzan.com") {
                    } else {
                        //������̨
                        window.location.href = returnData;
                    }
                }
            }
        });
    }
    setInterval(wxLogin, 1000);

    function ValidateLogins() {
        var userName = $("#user_name").val();
        var passWord = $("#user_pwd").val();
        $("#warning").css("display", "none");
        if (userName == "") {
            $("#warning").html("�˺Ų���Ϊ��");
            $("#warning").css("display", "block");
            return;
        }
        if (passWord == "") {
            $("#warning").html("���벻��Ϊ��");
            $("#warning").css("display", "block");
            return;
        }

        $.ajax({
            url: "/dzhome/DZLoginAjax",
            type: "post",
            data: { userName: userName, passWord: passWord, isKeep: true, backurl: "" },
            dataType: "json",
            success: function (data) {
                $("#warning").css("display", "none");
                if (data.success) {
                    window.location.href = "/dzhome/caseTemplate";
                }
                else {
                    if (data.code == 1) {
                        $("#warning").html("��������˺Ų�����");
                        $("#warning").css("display", "block");
                    }
                    else if (data.code == 2) {
                        $("#warning").html("��������������˺Ų�ƥ��");
                        $("#warning").css("display", "block");
                    }
                }
            }
        });
    }
})