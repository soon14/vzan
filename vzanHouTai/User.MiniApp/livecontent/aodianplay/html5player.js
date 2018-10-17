var HTML5_ID_BASE = 0;

var tools = {
    iscommonleveltbs: function () {
        var _ua = navigator.userAgent;
        //if (/iphone|ipad|ipod/.test(navigator.userAgent.toLowerCase())) {
        //    return true;
        //}
        return /TBS\/(\d{6})/gi.test(_ua) ?
            parseInt(_ua.match(/TBS\/(\d{6})/)[1]) > 036849 :
        (/MQQBrowser\/(\d.\d)/gi.test(_ua) ?
            parseFloat(_ua.match(/MQQBrowser\/(\d.\d)/)[1]).toFixed(1) >= 7.1 : false);
    }
}

function IsAutoFullScreen() {
    var _agent = navigator.userAgent;
    if (_agent.indexOf("MI 5") > 0) {
        return true;
    }
    else {
        return false;
    }
}
function isios() {//iphone
    return /iphone|ipad|ipod/.test(navigator.userAgent.toLowerCase())
}
function html5playerRun(conf) {
    var mode = /^\d{0,6}(\%)?$/;
    var width = mode.test(conf.width) ? conf.width : '100%';
    var height = mode.test(conf.height) ? conf.height : '100%';
    HTML5_ID_BASE++;
    this.uuid  /*string*/ = 'html5Media' + HTML5_ID_BASE;
    this.hlsUrl = conf.hlsUrl;
    this.container = conf.mediaid;
    this.autostart = conf.autostart;
    this.volume = conf.volume ? conf.volume : 80;//音量	  
    this.adveDeAddr = conf.adveDeAddr ? conf.adveDeAddr : '';//播放前显示图片地址
    var _this = this;
    var html = "";
    var _ctrl = "";
    //if (isios()) {
    //    _ctrl = "controls";
    //}
    html += "<video x5-video-player-type='h5'    x5-video-player-fullscreen='true'  class='video'  " + _ctrl + " preload='auto' id='liveVideo' type='application/x-mpegURL' poster='" + this.adveDeAddr + "'  src='" + this.hlsUrl + "' style='width:100%;'";
    html += "playsinline  webkit-playsinline webkit-inline='true'  x-webkit-airplay='allow ' controls></video>";//<span id='videoopeator'></span> 
    //html += "<div class='videoPoster'>";
    //html += "<img id='playbtn' style='top: 50%;left: 50%;-webkit-transform: translate(-50%,-50%);transform: translate(-50%,-50%);position: absolute;cursor:pointer;' src='//j.vzan.cc/zhibo/img/play.png' width='80px'>";
    //html += "<img src='" + this.adveDeAddr + "' style='cursor:pointer;border:0;width:100%;' id='livePoster' />";
    //html += "</div>";

    document.getElementById(conf.container).innerHTML = html;
    //document.getElementById("liveVideo").play();
    if (typeof (conf.lssCallBackFunction) == 'function') {
        conf.lssCallBackFunction();

    }

}