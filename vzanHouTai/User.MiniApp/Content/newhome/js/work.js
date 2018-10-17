
function hide(id, type) {
    $("#" + id).hide();
    if (type == 0) {
        $("#templatemodel").hide();
        $("#templatemodel").attr("data-children", "");
    }
    $(document.body).css("overflow-y", "")

}


function show(id, type) {
    $("#" + id).show();
    $("#templatemodel").show();
    if (type == 0) {
        $("#templatemodel").attr("data-children", id);
    }
    $(document.body).css("overflow-y", "hidden")
}


function html(id, msg) {
    $("#" + id).html(msg);
    show(id);
}

    //#region
    var syswindow_hide_timer = 0;
    $("#sysmsg").hover(
                 function () {
                     $(".sysmsg-setting").show();
                     if (syswindow_hide_timer > 0) {
                         clearInterval(syswindow_hide_timer);
                     }
                 },
                 function () {
                     syswindow_hide_timer = setTimeout(function () {
                         $(".sysmsg-setting").hide();
                     }, 500)
                 }
             )
    $(".sysmsg-setting").mouseover(function () {
        if(syswindow_hide_timer>0)
        {
            clearInterval(syswindow_hide_timer);
        }
    })
    $.post("/dzhome/GetNoReadSystempMessageList", function (data) {
        if (data.isok) {
            var array = new Array();
            if(data.data!=null && data.data.length>0)
            {
                for (var i = 0; i < data.data.length; i++) {
                    var model = data.data[i];
                    array[array.length] = '<a href="/dzhome/SystemMessageList?sysid=' + model.Id + '" class="sysmsg-item" >';
                    array[array.length] = '<div class="sysmsg-item-title c-666">' + model.Title + '</div>';
                    array[array.length] = '<div class="line line4">' + model.Content + '</div>';
                    array[array.length] = '<div class="sysmsg-item-update">' + model.UpdateTimeStr + '</div>';
                    array[array.length] = '</a>';
                }
                //$(".sysmsg-setting").show();
                $(".sysmsg").show();
            }
            else {
                //array[array.length] = '<div>';
                array[array.length] = '<div class="no-sysmsg tc" >暂无消息</div>';
                //array[array.length] = ' </div>';
                $(".allreadclick").hide();
                $(".sysmsg-setting").hide();
            }

            $("#sysmsg_content").empty().html(array.join(""));
        }
    })
    $(".allreadclick").on("click",function () {
        $.post("/dzhome/AllRead", function (data) {
            if (data.isok) {
                $(".sysmsg-setting").hide();
                $(".sysmsg").hide();
                window.location.reload();
            }
        })
    })
    
    //提交信息
    function submitInformation(type, method, enter) {
        if (enter) {
            var name = $(".contacts").val();
            var phone = $(".contacts-phone").val().trim();
        } else {
            var name = $("#contacts").val();
            var phone = $("#contactsPhone").val().trim();
        }
        if (method == 2) {
            name = $("#applyContacts").val();//立即报名
            phone = $("#applyContactsPhone").val().trim();//立即报名
        }
        var typeValue = type;
        if (name == "" || phone == "") {
            layer.msg("请填写完整信息");
            return;
        }
        //if (!(/^1[34578]\d{9}$/.test(phone))) {
        //    layer.msg("请输入正确的手机号");
        //    return;
        //}

        $.post("/DLPT/SendUserAdvisory",
            { Phone: phone, username: name, source: typeValue },
            function (data) {
                if (data.isok) {
                    layer.msg(data.Msg);
                    var name = $("#contacts").val("");
                    var phone = $("#contactsPhone").val("");
                    setTimeout(function () {
                        window.location.reload();
                    }, 1000);
                }
            })
    }

    //获取当前屏幕宽高
    function popUp() {
        var height = $(window).height()
        var width = $(window).width()
        console.log(height)
        console.log(width)
    }


    //start公共方法
    //保存手机号手机号
    function resetpwdnext(phone, code, type, backgroundurl) {
        if (phone == '') {
            layer.msg("手机号不能为空");
            return;
        }
        //if (!(/^1[34578]\d{9}$/.test(phone))) {
        //    layer.msg("请输入正确的手机号");
        //    return;
        //}

        if (code == "") {
            layer.msg("请输入验证码");
            return;
        }

        $.post("/dzhome/SaveUserInfo", { phone: phone, code: code, type: type }, function (data) {
            if (data.isok) {
                layer.msg(data.Msg);
                setTimeout(function () {
                    if (backgroundurl != undefined && backgroundurl.length > 0) {
                        window.location.href = backgroundurl;
                    }
                    else {
                        window.location.reload();
                    }
                }, 1000);
            }
            else {
                layer.msg(data.Msg)
            }
        })
    }

    //发送验证码
    function sendValidCode(id, phone, type) {
        if (timer > 0) {
            return;
        }
        if (phone == '') {
            layer.msg("手机号不能为空")
            return;
        }
        //if (!(/^1[34578]\d{9}$/.test(phone))) {
        //    layer.msg("请输入正确的手机号")
        //    return;
        //}

        $.post("/dzhome/GetVaildCode", { phonenum: phone, type: type }, function (data) {
            if (data.code == 1) {
                layer.msg("发送成功");
                //$("#" + id).css("background-color", "#DCDCDC")
                //启动计时器
                setTimer(id, 1, 60);
            }
            else {
                layer.msg(data.Msg)
                show(warnid);
            }
        })
    }
    //发送验证码后计时
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
                //$("#" + id).css("background-color", "#3296FA")
                timerseconds = initcount;
                clearInterval(timer)
                timer = 0;
            }
        }, seconde * 1000)
    }



    //免费开通模板
    function OpenFreeTemplate(tid) {
        var formdata = { tid: tid };
        $.post("/dzhome/TestTemplate", formdata, function (data) {
            layer.msg(data.Msg + "，正在为您跳转到登录界面");
            if (data.isok) {
                setTimeout(function () {
                    window.location.reload();
                }, 1000)
            }
            if (data.code == "-10") {
                setTimeout(function () {
                    window.location.href = "/dzhome/login";
                }, 2000)
            }
        }, "json")
    }
    //end公共方法

    $(function () {
        $("#exit").bind("click", function () {
            if (confirm("确定退出当前用户")) {
                //var url = window.location.host;
                //var domain = ".vzan.com";
                //if (url == "www.xiaochengxu.com.cn") {
                //    domain = '.xiaochengxu.com.cn';
                //}
                var url = window.location.host;
                var domain = "";
                if (url.indexOf("www.") != -1) {
                    domain = url.replace("www.", ".");
                }
                else {
                    domain = "." + url;
                }
                var useraccountid = $.cookie("agent_UserCookieNew");
                $.cookie("dz_UserCookieNew", "00000000-0000-0000-0000-000000000000", {
                    expires: 7,
                    path: '/',
                    domain: domain
                });
                $.cookie("agent_UserCookieNew", "00000000-0000-0000-0000-000000000000", {
                    expires: 7,
                    path: '/',
                    domain: domain
                });
                window.location.href = "/dzhome/logout?userAccountId=" + useraccountid;
            }
        });


        $(".user").bind("click", function () {

            var type = $(this).attr("data-type");
            hide("usertemplate", 0);
            if (type < 3) {
                if (type == 1) {
                    $("#phonetitle").html("绑定手机号");
                }
                else {
                    $("#phonetitle").html("修改手机号");
                }
                //修改手机号
                show("updatephonediv", 0);
            }
            else if (type == "3") {
                //修改秘密啊
                show("updatepwd", 0);
            }
            else {
                //打开用户设置
                show("usertemplate", 0);
            }

        });
        //$(".user").bind("click", function () {

        //    var type = $(this).attr("data-type");

        //    hide("usertemplate", 0);
        //    if (type == "1") {
        //        //绑定手机号
        //        show("bindphone", 0);
        //    }
        //    else if (type == "2") {
        //        //修改手机号
        //        show("showupdatephone", 0);
        //    }
        //    else if (type == "3") {
        //        //修改秘密啊
        //        show("updatepwd", 0);
        //    }
        //    else {
        //        //打开用户设置
        //        show("usertemplate", 0);
        //    }
        //});



        //发送验证码
        $(".btnsendcode").on("click", function () {
            //1:绑定新手机号，0修改手机号
            var type = $("#updatephone").attr("data-type");
            var id = this.id;
            var phone = $("#phone").val().trim();
            sendValidCode(id, phone, type);
        })


        //保存手机号
        $(".btnsave").on("click", function () {
            //1:绑定新手机号，0修改手机号
            var ptype = $(this).attr("data-type");
            var phone = $("#phone").val().trim();
            var code = $("#code").val().trim();
            var type = ptype == "1" ? 2 : 3;
            //跳转链接
            var backgroundurl = $(this).attr("data-backgroundurl");
            resetpwdnext(phone, code, type, backgroundurl);
        })


        //修改密码
        $(".btnupdatepwd").on("click", function () {
            var warmid = "warningpwd";
            var pwd1 = $("#pwd1").val();
            var pwd2 = $("#pwd2").val();
            hide(warmid);
            if (pwd1 == '') {
                //html(warmid, "请输入密码");
                layer.msg("请输入密码")
                return;
            }

            if (pwd2 == "") {
                //html(warmid, "重复密码不能为空");
                layer.msg("重复密码不能为空")
                return;
            }

            if (pwd2 != pwd1) {
                //html(warmid, "两次密码不一致，请重新输入");
                layer.msg("两次密码不一致，请重新输入")
                return;
            }
            if (!confirm("确定更改密码？")) {
                return;
            }
            $.post("/dzhome/SaveUserInfo", { password: pwd1, checkphone: 1, type: 1 }, function (data) {
                if (data.isok) {
                    layer.msg(data.Msg);
                    setTimeout(function () {
                        window.location.reload();
                    }, 1000);
                }
                else {
                    //html(warmid, data.Msg);
                    layer.msg(data.Msg)
                }
            })
        })


        //取消
        $(".btn-cancel").bind("click", function () {
            //判断普通用户是否绑定手机号，没有绑定则不能取消弹出框
            var bindphone = $("#casetemplate_bindphone");
            if (bindphone != undefined && bindphone.val() == 1) {
                return;
            }
            var cid = $("#templatemodel").attr("data-children");
            hide(cid, 0);
        });


        $("#templatemodel").bind("click", function () {
            //判断普通用户是否绑定手机号，没有绑定则不能取消弹出框
            var bindphone = $("#casetemplate_bindphone");
            if (bindphone != undefined && bindphone.val() == 1) {
                return;
            }
            var cid = $(this).attr("data-children");
            //hide(cid, 0);
        });


        $(".stopevent").bind("click", function (event) {
            event.stopPropagation();
        });

    })
