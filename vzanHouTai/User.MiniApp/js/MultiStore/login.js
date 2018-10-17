
$(function () {

    //��½
    $("#login").on("click", function () {
        //showStore()
        ValidateLogins()
    })

    //enter����½
    $("#bodykey").on("keyup", function () {
        keyLogin()
    });
    //�س���¼
    function keyLogin() {
        if (event.keyCode == 13)  //�س����ļ�ֵΪ13
            ValidateLogins(); //���õ�¼��ť�ĵ�¼�¼�
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

    //��ʾѡ�����
    function showStore() {
        $("#loginTitle").hide();
        $(".login-list").hide();
        $("#storeTitle").show();
        $(".store-manage").show();
        $("#accountTitle").hide();
        $(".account-manage").hide();

        $(".store-icon").on("click", function () {
            $(".store-icon").attr("isCheck", 0);
            $(".store-icon").children().attr("src", "http://j.vzan.cc/dz/content/images/MultiStore/unselected.png")

            $(this).attr("isCheck", 1);
            $(this).children().attr("src", "http://j.vzan.cc/dz/content/images/MultiStore/selected.png")
        });

        $("#btnStoreCancel").on("click", function () {
            showLogin();
        });
        $("#btnStoreConfirm").on("click", function () {
            if ($(".store-icon[isCheck='1']").length <= 0) {
                layer.msg("��ѡ����Ҫ������ŵ�");
                return;
            }
            var checkedStoreUrl = $(".store-icon[isCheck='1']").data("url");

            window.location.href = checkedStoreUrl;
        });
    }
    //��ʾ�����¼
    function showLogin() {
        $("#loginTitle").show();
        $(".login-list").show();
        $("#storeTitle").hide();
        $(".store-manage").hide();
        $("#accountTitle").hide();
        $(".account-manage").hide();
    }


    //��ʱȷ���Ƿ��Ѿ�ɨ���¼
    var wxLoginInterval = setInterval(wxLogin, 1000);
    //ɨ���¼
    function wxLogin() {
        var key = $('#sessid').val();
        $.ajax({
            type: "POST",
            url: "/MultiStore/UserLogin/WXLogin",
            xhrFields: {
                withCredentials: true
            },
            data: { wxkey: key },
            success: function (returnData) {
                if (returnData.success) {
                    clearInterval(wxLoginInterval);
                    var url = window.location.host;
                    var domain = ".vzan.com";
                    if (url == "www.xiaochengxu.com.cn") {
                        domain = '.xiaochengxu.com.cn';
                    }

                    var cookiekey = "dz_UserCookieNew";
                    $.cookie(cookiekey, returnData.msg, {
                        expires: 7,
                        path: '/',
                        domain: domain
                    });
                    getManagerList();
                }
                else
                {
                    //layer.msg(returnData.msg);
                    return;
                }
            }
        });
    }

    //�����¼
    function ValidateLogins() {
        var merNo = $("#multiStore_merchant_name").val();
        var userName = $("#multiStore_user_name").val();
        var password = $("#multiStore_user_pwd").val();
        if (merNo == "")
        {
            layer.msg("�̻��Ų���Ϊ��")
            return;
        }
        if (userName == "") {
            layer.msg("�˺Ų���Ϊ��")
            return;
        }
        if (password == "") {
            layer.msg("���벻��Ϊ��")
            return;
        }

        //�����¼
        $.ajax({
            url: "/MultiStore/UserLogin/LoginRequest",
            type: "post",
            data: { merNo: merNo, loginName: userName, password: password },
            dataType: "json",
            success: function (data) {
                $("#warning").css("display", "none");
                if (data.success) {
                    getManagerList();
                }
                else {
                    layer.msg(data.msg);
                }
            }
        });
    }

    //��ȡ�����б�
    function getManagerList()
    {
        //��ȡ�����б�
        $.ajax({
            url: "/MultiStore/UserLogin/GetManagerList",
            type: "get",
            data: {},
            dataType: "json",
            success: function (data) {
                $("#warning").css("display", "none");
                if (data.success) {
                    var checkStoreHtml = '<div style="padding:4% 2%;">';
                    $(data.postData).each(function (i, x) {
                        checkStoreHtml += '     <div class="store-item f fc">';
                        checkStoreHtml += '         <div class="store-icon" isCheck="0" data-url="' + x.url + '">';
                        checkStoreHtml += '             <img src="http://j.vzan.cc/dz/content/images/MultiStore/unselected.png" />';
                        checkStoreHtml += '         </div>';
                        checkStoreHtml += '         <div class="store-content">';
                        checkStoreHtml += '             <h4 class="store-name">' + x.storeName + '</h4>';
                        checkStoreHtml += '             <p class="store-details">' + x.storeAddress + '</p>';
                        checkStoreHtml += '         </div>';
                        checkStoreHtml += '     </div>';
                    });
                    checkStoreHtml += '</div>';

                    $("#checkStore_check").html(checkStoreHtml);
                    showStore();
                }
                else {
                    layer.msg(data.msg);
                    showStore();
                }
            }
        });
    }
})