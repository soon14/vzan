$(function () {
    var audioPlayer = "";
    var videoPlayer = "";
    var fls = flashChecker();
    if (fls.f) {
        console.log("您安装了flash,当前flash版本为: " + fls.v + ".x");
    } else {
        $("#audioTip").fadeIn();
        $("#hasFlashErr").show();
    }


    $("#audioPlay").on("click", function () {
        $(this).fadeOut(function () {
            $("#audioPause").fadeIn();
        });
        if (fls.f) {
            if (videoPlayer != "") {
                videoPlayer.stopPlay();
                $(".video-cover").fadeIn();
                $(".player-btn").css("z-index", "");
                $("#videomainbody").css("overflow", "hidden");
            }
            audioPlayer.startPlay();
        } else {
            $(this).fadeIn();
            
        }
    })
    $("#audioPause").on("click", function () {
        $(this).fadeOut(function () {
            $("#audioResumePlay").fadeIn();
        });
        audioPlayer.pausePlay();
    })
    $("#audioResumePlay").on("click", function () {
        $(this).fadeOut(function () {
            $("#audioPause").fadeIn();
        });
        audioPlayer.resumePlay();
    })


    $(".player-btn").on("click", function () {
        $(this).fadeOut();
        $(".video-cover").fadeOut();
        if (fls.f) {
            if(audioPlayer != ""){
                audioPlayer.stopPlay();
                $("#audioPause").fadeOut(function () {
                    $("#audioPlay").fadeIn();
                })
            }
            $("#videomainbody").css("overflow", "");
            videoPlayer.startPlay();
            $(".player-btn").css("z-index", "-1");
        }
       
    })


    videoPlayer = new aodianPlayer({
        playerwrap: 'videomainbody',
        container: 'videoPlayer',
        hlsUrl: 'http://oss-vod.vzan.cc/live/131725081055804046.1528080234.m3u8?time=636659522600959370',//控制台开通的APP hls地址
        /* 以下为可选参数*/
        width: "100%",
        height: "100%",
        autostart: false,
        bufferlength: 1,
        listencallback: function (res) {
            $("#videoLoading").fadeOut(function () {
                $(".player-btn").fadeIn();
            });
        }
    });
    audioPlayer = new aodianPlayer({
        playerwrap: 'audioMainBody',
        container: 'audioPlayer',
        hlsUrl: 'http://oss-vod.vzan.cc/live/131725692185746300.1528099441.m3u8',//控制台开通的APP hls地址
        /* 以下为可选参数*/
        width: "100%",
        height: "100%",
        autostart: false,
        bufferlength: 1,
        listencallback: function () {
            $("#loading").fadeOut();
            console.log(videoPlayer)

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

})

