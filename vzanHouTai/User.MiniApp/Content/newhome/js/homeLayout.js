; (function () {
    $(function () {


        //懒加载
        $("img.lazy").lazyload({ threshold: 100 });
        //滚动效果
        window.sr = ScrollReveal({
            reset: true,
        });
        sr.reveal('.rv');

        var currentPath = window.location.pathname.split("/").slice(2).toString();
        //console.log(currentPath)
        switch (currentPath) {
            case "newHome":
                $("#home>a").addClass("nav-active");
                break;
            //case "newRetail":
            //    $("#nav>li>a").removeClass("nav-active");
            //    $("#retail>a").addClass("nav-active");
            //    break;
            case "pinxianghui":
                $("#nav>li>a").removeClass("nav-active");
                $("#newRetail>a").addClass("nav-active");
                $("#pinxianghui").css("background-color", "rgba(99,181,4,1)");
                break;
            case "retail":
                $("#nav>li>a").removeClass("nav-active");
                $("#newRetail>a").addClass("nav-active");
                $("#retail").css("background-color", "rgba(99,181,4,1)");
                break;
            case "videoList":
                $("#nav>li>a").removeClass("nav-active");
                $("#videos>a").addClass("nav-active");
                break;
            case "videoDetails":
                $("#nav>li>a").removeClass("nav-active");
                $("#videos>a").addClass("nav-active")
                break;
            case "agent":
                $("#nav>li>a").removeClass("nav-active");
                $("#agent>a").addClass("nav-active");
                break;
            case "productCenter":
                $("#nav>li>a").removeClass("nav-active");
                $("#center>a").addClass("nav-active");
                break;
            case "templateMarket":
                $("#nav>li>a").removeClass("nav-active");
                $("#center>a").addClass("nav-active");
                $("#templateMarket").css("background-color", "rgba(99,181,4,1)");
                break;
            case "solution":
                $("#nav>li>a").removeClass("nav-active");
                $("#center>a").addClass("nav-active");
                $("#solution").css("background-color", "rgba(99,181,4,1)");
                break;
            case "productCase":
                $("#nav>li>a").removeClass("nav-active");
                $("#case>a").addClass("nav-active");
                break;
            case "miniAppShop":
                $("#nav>li>a").removeClass("nav-active");
                $("#shop>a").addClass("nav-active");
                break;
            case "about":
                $("#nav>li>a").removeClass("nav-active");
                $("#about>a").addClass("nav-active");
                break;
        }

        $("#touxiang").hover(
            function () {
                $(".user-setting").show();
            },
            function () {
                $(".user-setting").hide();
            }
        )
        $(".copyright-details-list>li>a").on("click", function () {
            window.location.reload();
            window.scrollTo(0, 0)
        })
        $(window).resize(function () {
            //console.log($("#top").height())
            positonRightList()
        });
        positonRightList();
        function positonRightList() {
            var vNavscollTop, swiperHeight, navHeight;
            navHeight = $("#top").height();
            swiperHeight = $("#bannerSwiper").height() || 0;
            var navSwiperHeight = navHeight + $("#bannerSwiper").height();
            //console.log("导航：" + navHeight, "轮播图:" + swiperHeight);
            if ($(window).scrollTop() < navSwiperHeight) {
                $(".right-list").css("top", vNavscollTop);
            }
            $(window).scroll(function () {
                vNavscollTop = navSwiperHeight - $(window).scrollTop();
                if ($(window).scrollTop() <= 0) {
                    //if (swiperHeight == undefined || swiperHeight == 0 || swiperHeight == null) {
                    //     $(".right-list").css("top", navHeight + 10)
                    // } else {
                    $(".right-list").css("top", vNavscollTop + 10)
                    //}
                } else if ($(window).scrollTop() > 0) {
                    if ($(window).scrollTop() <= navSwiperHeight) {
                        $(".right-list").css("top", vNavscollTop + navHeight)
                    } else if ($(window).scrollTop() >= navSwiperHeight) {
                        //if (swiperHeight == undefined || swiperHeight == 0 || swiperHeight == null) {
                        //    $(".right-list").css("top", navHeight + 40)
                        //} else {
                        $(".right-list").css("top", navHeight + 10)
                        //}
                    }
                }
            })
        }


        $.post("/dzhome/GetUserCookie", function (data) {
            if (data.isok) {
                if (data.dataObj._Account != null) {
                    $("#logined").show();
                    $("#userloginid").html(data.dataObj._Account.LoginId);

                    if (data.dataObj.Agentinfo != null) {
                        $("#agentauthod").html(data.dataObj.Agentinfo.AuthCode);
                        $("#showagent").show();
                    }
                    else {
                        $("#agentauthodbox").hide();
                    }
                    if (data.dataObj._Account.ConsigneePhone == null || data.dataObj._Account.ConsigneePhone == undefined || data.dataObj._Account.ConsigneePhone.length <= 0) {
                        $("#updatephone").attr("data-type", "1");
                        $("#updatephone").html("去绑定");
                    }
                    else {
                        $("#updatephone").attr("data-type", "2");
                        $("#updatephone").html("去修改");
                        $("#userphone").html(data.dataObj._Account.ConsigneePhone);
                        $("#editphone").html(data.dataObj._Account.ConsigneePhone);
                    }
                }
                else {
                    $("#nologin").show();
                }
            }
        })

        //方法从work.js调用
        //打款说明
        $(".btn_payment").click(function () {
            var paymentHeight = $(window).height() * 0.9;
            $(".payment-box").css("height", paymentHeight)
            show("payment", 0)
        })
        // 立即报名  
        $("#btnApply").click(function () {
            show("apply", 0)
        })
        $("#btnOk").click(function () {
            var method = $(this).attr("data-type");
            submitInformation(3, method)
        })
        //提交信息
        $(".btn_gotobuy").click(function () {
            show("gotobuy", 0)
        })
        //立即订购
        $("#admintmsg").click(function () {
            submitInformation(2)
        })
        //立即咨询
        $("#btnSubmit").click(function () {
            var isAgent = $("#btnSubmit").attr("data-page");
            submitInformation(0, 5, isAgent);
        })
        //3分钟免费创建
        $("#freeBuild").on("click", function () {
            OpenFreeTemplate(4)
        })


        $(".btn-label1").click(function () {
            $(".btn-label1").removeClass("current-btn");
            $(this).addClass("current-btn");
        })

        $("#btn_top").click(function () {
            $("html, body").animate({
                scrollTop: $(".content").offset().top
                //scrollTop: $(document).offset()
            }, { duration: 500, easing: "swing" });
            return false;
        });



        $("#templatemodel").on("click", function () {
            $("#payment").hide();
            $("#gotobuy").hide();
            $("#templatemodel").hide();
            $(document.body).css("overflow-y", "")
            var cid = $(this).attr("data-children");
            hide(cid, 0);
        })


        //$(window).load(function () {
        //    var str = "<script async>" +
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
