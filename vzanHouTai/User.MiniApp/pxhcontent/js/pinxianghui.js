$(function () {

    window.sr = ScrollReveal({
        reset: true,
    });
    sr.reveal('.rv');


    //导航条定位内容
    var arrOffsetTop = [
        $('.banner').offset().top,
        $('.video-box').offset().top,
        $('.flow-path').offset().top,
        $('.sever-box').offset().top,
    ];
    $(".nav-list>li").click(function () {
        if ($(this).attr("data-index")!=undefined) {
            $('body, html').animate({ scrollTop: arrOffsetTop[$(this).attr("data-index")] - $(".top-list").height() }, 500);
        }
    });


    //检测是否安装了flash
    function flashChecker() {
        var hasFlash = 0; //是否安装了flash
        var flashVersion = 0; //flash版本
        if (document.all) {
            var swf = new ActiveXObject('ShockwaveFlash.ShockwaveFlash');
            if (swf) {
                hasFlash = 1;
                VSwf = swf.GetVariable("$version");
                flashVersion = parseInt(VSwf.split(" ")[1].split(",")[0]);
            }
        } else {
            if (navigator.plugins && navigator.plugins.length > 0) {
                var swf = navigator.plugins["Shockwave Flash"];
                if (swf) {
                    hasFlash = 1;
                    var words = swf.description.split(" ");
                    for (var i = 0; i < words.length; ++i) {
                        if (isNaN(parseInt(words[i]))) continue;
                        flashVersion = parseInt(words[i]);
                    }
                }
            }
        }
        return { f: hasFlash, v: flashVersion };
    }
    var fls = flashChecker();
    if (fls.f) {
        console.log("您安装了flash,当前flash版本为: " + fls.v + ".x");
    } else {
        $(".video-tip").show();
    }


    $(".code-btn").on("click", function () {
        $(".pxh-phone-box").fadeOut();
        $(".pxh-code-box").fadeIn();
    });
    $(".pxh-code-box>.close-btn").on("click", function () {
        $(".pxh-code-box").fadeOut();
    });
    $("#phoneBtn").on("click", function () {
        $(".pxh-code-box").fadeOut();
        $(".pxh-phone-box").fadeIn();
    });
    $(".pxh-phone-box .close-btn").on("click", function () {
        $(".pxh-phone-box").fadeOut();
    });
    $(".submit-btn").on("click", function () {
        submitInformation();
    })

    //提交咨询
    function submitInformation() {
        var name = "来自拼享惠官网"
        var phone = $(".pxh-phone-box input").val().trim();

        if (phone == "") {
            layer.msg("请填写完整信息");
            return;
        }
        if (!(/^1[34578]\d{9}$/.test(phone))) {
            layer.msg("请输入正确的手机号");
            return;
        }

        $.post("/DLPT/SendUserAdvisory",
            { Phone: phone, username: name, source: 0 },//source: 0 pc端、1 移动端
            function (data) {
                if (data.isok) {
                    layer.msg("提交成功");
                    $(".pxh-phone-box input").val("");
                    $(".pxh-phone-box").fadeOut();
                } else {
                    layer.msg(data.Msg);
                }
            })
    }

    $("#videoPlayer1 .player-btn").on("click", function () {
        $("#videoPlayer1>.cover-box").fadeOut();
        $("#videomainbody>.cover-box").fadeIn();
        objectPlayer.pausePlay();
        $("#videoPlayer1>video").trigger('play');
    })
    $("#videomainbody .player-btn").on("click", function () {
        $("#videomainbody>.cover-box").fadeOut();
        $("#videoPlayer1>.cover-box").fadeIn();
        $("#videoPlayer1>video").trigger('pause');
        objectPlayer.startPlay();
    })
    var objectPlayer = new aodianPlayer({
        playerwrap: 'videomainbody',
        container: 'videoPlayer2',//播放器容器ID，必要参数
        hlsUrl: "http://qvod2.vzan.cc/54261/131765325382005806/replay.1532441166.34943546.m3u8?time=636681997056719750",
        /* 以下为可选参数*/
        width: "100%",
        height: "100%",
        autostart: false,
        bufferlength: 1,//设置flv的缓存时间。默认为3秒
    });
    var pageIndex = 1, pageSize =10, type=2;
    $.ajax({
        type: "GET",
        url: "/dzhome/NewsList",
        data: "pageIndex=" + pageIndex + "&pageSize=" + pageSize + "&type=" + type,
        success: function (res) {
            if (res.isok) {
                //console.log(res); 
                var dynamicList = res.dataObj.list;
                $.each(dynamicList, function (i, item) {
                    if (i <6) {
                        var url = item.NewsURL == "" ? "/dzhome/more_detail/" + item.Id : item.NewsURL;
                        var html = '<li class="f fv rv">' +
                                       '<a href="' + url + '" target="_blank">' +
                                            '<img src="' + item.ImgPath + '" />' +
                                            '<p class="f22 line line2">' + item.Title + '</p>' +
                                       '</a>'
                        '</li>'
                        $(".dynamic-list").append(html);
                    } else {
                        $(".btn-more").show();
                    }
                })

            } else {
                console.log(res.Msg);
            }
        }
    })
});