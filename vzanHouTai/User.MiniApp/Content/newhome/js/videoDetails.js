; (function () {
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
    var s = "";
    if (fls.f) document.write("您安装了flash,当前flash版本为: " + fls.v + ".x");
    else $("#hasFlashErr").css("display","flex"); //alert("抱歉，您没有安装flash，为了您能更好的体验，请先安装flash或更换其他打开浏览器观看");


    var urlParam = window.location.search.split("&");
    //console.log(urlParam)
    var id = urlParam[0].split("=")[1];
    var isPsw = urlParam[1].split("=")[1];
    var page = decodeURI(urlParam[2].split("=")[1]) //对url传来的中文标题进行解码
    var date = urlParam[3].split("=")[1]
    var playSrc = urlParam[4].split("=")[1]
    //console.log(dateIndex)
    //console.log(pageIndex)
    //console.log(id)
    //console.log(isPsw)
    //console.log(date)
    //console.log(page)
    document.title = page;
    $(".video-title>h1").text(page);
    $("#date").text(date);


    isPswVideo(playSrc);
    function isPswVideo(playSrc) {
        if (isPsw == "true") {
            $("#videoMasking").hide();
            $(".encryption-masking").css("display","flex");
        } else if (isPsw == "false") {
            $(".encryption-masking").hide();
            playVideo(playSrc);
        }

    }
    function playVideo(playSrc) {
        var w = "100%";//视频宽度
        var h = "100%";//视频高度
        var url = playSrc// 'http://qvod2.vzan.cc/54261/131618790255281093/replay.1517494924.31169607.m3u8?time=636538752114194502';//视频地址
        var objectPlayer = new aodianPlayer({
            playerwrap: 'videomainbody',
            container: 'videoPlayer',//播放器容器ID，必要参数
            hlsUrl: url,//控制台开通的APP hls地址，必要参数
            /* 以下为可选参数*/
            width: w,//播放器宽度，可用数字、百分比等
            height: h,//播放器高度，可用数字、百分比等
            autostart: true,//是否自动播放，默认为false
            bufferlength: 1,//设置flv的缓存时间。默认为3秒
            //controlbardisplay: 'disable',//是否显示控制栏，值为：disable、enable默认为disable。
            //adveDeAddr: 'https://i.vzan.cc/image/liveimg/jpeg/2018/2/1/155809fe4a1b71eb364b518e9667c37a6a3068.jpeg?x-oss-process=image/resize,limit_0,m_fill,w_640,h_360/format,jpeg/quality,q_70',//封面图片链接
            // adveWidth: w,//封面图宽度
            // adveHeight: h,//封面图高度
            // adveReAddr: 'https://i.vzan.cc/image/liveimg/jpeg/2018/2/1/155809fe4a1b71eb364b518e9667c37a6a3068.jpeg?x-oss-process=image/resize,limit_0,m_fill,w_640,h_360/format,jpeg/quality,q_70' //封面图点击链接
        });
    }

    //提交加密视频密码
    $("#encryptionVideoBtn").on("click", function () {
        //Base64加密
        var b = new Base64();
        var videopsw = b.encode($("#encryptionInput").val());
        submitVideoPsw(videopsw);
    })
    //提交视频密码
    function submitVideoPsw(code) {
        $.post("/dzhome/CanSeeVideo",
                {
                    id: id,
                    password: code
                },
                 function (data) {
                     if (data.isok) {
                         isPsw = "false";
                         isPswVideo(playSrc);
                     } else {
                         layer.msg(data.msg);
                     }
                 })
    }







})()
