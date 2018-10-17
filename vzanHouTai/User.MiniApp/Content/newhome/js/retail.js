$(function () {
    var detailArr = [
           "满足了几乎所有足浴、按摩、水疗行业经常光顾的顾客挑选技师的痒点，此点之痒，比痛点更甚。你将“按了吗”展示到任何此类顾客眼前，他们都会大喜过望，心动不已，恨不得所有的店铺都用上这个系统。从价值上来说，这个产品彻底的征服了消费者。",
           "技师有点钟和轮钟之说，轮钟通常一个小时的提成是30块，点钟一个小时的提成通常是轮钟的两倍，在线预约肯定是点钟，可以大幅提高技师的收益。另外看照片的收益技师是可以直接提现的，这又进一步的增加了技师的收入。",
           "从营销能力上来讲，传统的足浴店、按摩店、水疗店就跟一只乌龟一样，从开业的第一天到最终结业整个经营历史上营销手段少之又少，最多派点人粗暴的发发传单，其他都是等待顾客上门，但是顾客上门要考虑的问题很多呀，有多远，时间够不够，技师好不好，这些问题都会导致本来有可能来的顾客中间由于任何你不能够左右的问题，而最终来不了。“按了吗”把技师当做一个核心内容在运营，她的好处是：通过技师的内容或者互动，可以大幅提高新客户到店率和老顾客二次购买率，让顾客的决策在你的掌控之中。技师为了获得更多的点钟会通过各种网络渠道推广自己的业务，以前没有一个合适的下单渠道，现在整套系统都已经完备。这两个问题，在过去的足浴行业是完全无解的。“按了吗”带来的是足浴行业的互联网化的深刻力量。",
    ];
    var detailsClass = ["role-details-one", "role-details-two", "role-details-three"];
    $(".role-list>li").on("click", function () {
        var index = $(this).index();
        $("#roleDetails").text(detailArr[index]);
        $("#roleDetails").removeClass();
        $("#roleDetails").addClass("role-details " + detailsClass[index]);
        $(".role-list>li").each(function () {
            $(this).children("img").removeClass("active");
            $(this).children(".role-name").removeClass("active");
        })
        $(this).children("img").addClass("active");
        $(this).children(".role-name").addClass("active");
    })


    $(".cover-btn").on("click", function () {

        $(this).fadeOut();
        $(".cover").fadeOut();
        var fls = flashChecker();
        if (fls.f) {
            console.log("您安装了flash,当前flash版本为: " + fls.v + ".x");
            objectPlayer.startPlay();
        }
        else $("#hasFlashErr").show();
    })

    var objectPlayer = new aodianPlayer({
        playerwrap: 'videomainbody',
        container: 'videoPlayer',
        hlsUrl: 'http://oss-vod.vzan.cc/live/131714561577405517.1527221887.m3u8?time=636650865623870922',//控制台开通的APP hls地址
        /* 以下为可选参数*/
        width: "100%",
        height: "100%",
        autostart: false,
        bufferlength: 1,
        //controlbardisplay: 'disable',//是否显示控制栏，值为：disable、enable默认为disable。
        //adveDeAddr: 'https://i.vzan.cc/image/liveimg/jpeg/2018/2/1/155809fe4a1b71eb364b518e9667c37a6a3068.jpeg?x-oss-process=image/resize,limit_0,m_fill,w_640,h_360/format,jpeg/quality,q_70',//封面图片链接
        // adveWidth: w,//封面图宽度
        // adveHeight: h,//封面图高度
        // adveReAddr: 'https://i.vzan.cc/image/liveimg/jpeg/2018/2/1/155809fe4a1b71eb364b518e9667c37a6a3068.jpeg?x-oss-process=image/resize,limit_0,m_fill,w_640,h_360/format,jpeg/quality,q_70' //封面图点击链接
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

    var technicianSwiper = new Swiper('#technicianSwiper', {
        autoplay: {
            disableOnInteraction: false
        },
        speed: 800,
        loop: true,
        centeredSlides: true,
        slidesPerView: 3,
        navigation: {
            prevEl: '.swiper-button-prev',
            nextEl: '.swiper-button-next',
        },
    })

    var businessSwiper = new Swiper('#businessSwiper', {
        autoplay: {
            disableOnInteraction: false
        },
        speed: 800,
        loop: true,
        centeredSlides: true,
        slidesPerView: 3,
        navigation: {
            prevEl: '.swiper-button-prev',
            nextEl: '.swiper-button-next',
        },
    })
})

