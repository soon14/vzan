$(function () {
    function fontSize() {
        var deviceWidth = $(document).width();
        if (deviceWidth > 1920) {
            deviceWidth = 1920;
            //                console.log("deviceWidth",deviceWidth);
        }
        var fontSize = deviceWidth / 19.2;
        //console.log("fontSize", fontSize);
        $("html").css("fontSize", fontSize);
    }
    fontSize();
    $(window).resize(function () {
        fontSize();
    });
    window.sr = ScrollReveal({
        reset: true,
    });
    sr.reveal('.rv');
    //导航条定位内容
    var arrOffsetTop = [
        $('.banner').offset().top,
       // $('.video-box').offset().top,
        $('.flow-path').offset().top,
        $('.sever-box').offset().top,
    ];
    $(".nav-list>li").click(function () {
        $('body, html').animate({ scrollTop: arrOffsetTop[$(this).attr("data-index")] - $(".top-list").height() }, 500);
    });

    $(".player-btn").on("click", function () {
        $(".cover-box").fadeOut();
        $(".player-box>video").attr("src", "http://j.vzan.cc/dz/content/images/pinxianghui/video/pxh.mp4").trigger('play');
        //$("#dsVideo").trigger('play');
    })

    //var objectPlayer = new aodianPlayer({
    //    playerwrap: 'videomainbody',
    //    container: 'videoPlayer',//播放器容器ID，必要参数
    //    hlsUrl:"",//控制台开通的APP hls地址，必要参数
    //    /* 以下为可选参数*/
    //    width: 100%,//播放器宽度，可用数字、百分比等
    //    height: 100%,//播放器高度，可用数字、百分比等
    //    autostart: true,//是否自动播放，默认为false
    //    bufferlength: 1,//设置flv的缓存时间。默认为3秒
    //});
});