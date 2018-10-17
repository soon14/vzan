window.onload = function () {
    document.body.style.display = "block"

    //播放视频
    $("#btnVideo").click(function () {
        $(".video-box>video").attr("src", "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/video/20180205/ef92ff22b9fd4c899c072b1c2d3731df.mp4")
        $("#videoMasking").hide();
        $("video").trigger('play');
    })
    //提交咨询
    $(".btn-contact").on("click", function () {
        var name = $("#name").val();
        var phone = $("#phone").val();
        var content = $("#content").val();
        if (name == "" || phone == "" || content == "") {
            layer.msg("请填写完整信息");
        } else {
            setAgentWebSiteQuetion(name, phone, content);
        }
    })

    swiper();
    selectCase();
    getCaseList(4);
    solutionCase();
    function swiper() {
        var cArr = ["p1", "p2", "p3", "p4", "p5", "p6", "p7", "p8", "p9", "p10", "p11", "p12"];
        dotsSwitch();
        function nextimg() {
            cArr.unshift(cArr[11]);
            cArr.pop();
            $("#solutionCase>li").each(function (i, e) {
                $(e).removeClass().addClass(cArr[i]);
            })
            var activePosition = cArr.indexOf("p1") + 1;
            dotsSwitch(activePosition);
        }
        function previmg() {
            cArr.push(cArr[0]);
            cArr.shift();
            $("#solutionCase>li").each(function (i, e) {
                $(e).removeClass().addClass(cArr[i]);
            })
            var activePosition = cArr.indexOf("p1") + 1;
            dotsSwitch(activePosition);
        }
        function dotsSwitch(position) {
            var dotsWidth = $(".dots-bg").width() / cArr.length;
            if (position != "" && position != undefined) {
                var activeDots = dotsWidth * position;
                $(".dots").width(activeDots);
            } else {
                $(".dots").width(dotsWidth);
            }

        }
        //var timer = setInterval(nextimg, 3000);
        var startX, startY, moveX, moveY, endX, endY;
        $(".slide-show").on("touchstart", function (e) {
            e.preventDefault();
            startX = e.originalEvent.changedTouches[0].pageX;
            startY = e.originalEvent.changedTouches[0].pageY;

        });

        $(".slide-show").on("touchend", function (e) {
            e.preventDefault();
            endX = e.originalEvent.changedTouches[0].pageX;
            endY = e.originalEvent.changedTouches[0].pageY;
            var x = endX - startX;
            var y = endY - startY;
            var imgUrl = e.target.getAttribute("src");
            //console.log(imgUrl)
            //console.log(Math.abs(x))
            //console.log(Math.abs(y))
            if (Math.abs(x) < 5) {
                var imgUrl = e.target.getAttribute("src");
                if (imgUrl != null) {
                    $("#largeUrl").attr("src", imgUrl);
                    $(".large-box").css("display", "flex");
                    $("html,body").css("overflow", "hidden")
                }
            }
            if (Math.abs(x) > Math.abs(y) && x > 0) {
                //console.log("左往右");
                previmg();
            }
            else if (Math.abs(x) > Math.abs(y) && x < 0) {
                //console.log("右往左");
                nextimg();
            }
        })
    }
    var bannerSwiper = new Swiper('#bannerSwiper', {
        autoplay: 3000,//可选选项，自动滑动
        autoplayDisableOnInteraction: false,
        pagination: '.swiper-pagination',
        paginationType: 'custom',//自定义分页器
        paginationCustomRender: function (swiper, current, total) {
            var customPaginationHtml = "";
            for (var i = 0; i < total; i++) {
                //判断哪个分页器此刻应该被激活
                if (i == (current - 1)) {
                    customPaginationHtml += '<span class="swiper-pagination-customs swiper-pagination-customs-active"></span>';
                } else {
                    customPaginationHtml += '<span class="swiper-pagination-customs"></span>';
                }
            }
            return customPaginationHtml;
        }
    })

    var mySwiper = new Swiper('#aboutSwiper', {
        autoplay: 3000,//可选选项，自动滑动
        autoplayDisableOnInteraction: false,
        pagination: '.swiper-pagination',
        paginationType: 'custom',//自定义分页器
        paginationCustomRender: function (swiper, current, total) {
            var customPaginationHtml = "";
            for (var i = 0; i < total; i++) {
                //判断哪个分页器此刻应该被激活
                if (i == (current - 1)) {
                    customPaginationHtml += '<span class="swiper-pagination-customs swiper-pagination-customs-active"></span>';
                } else {
                    customPaginationHtml += '<span class="swiper-pagination-customs"></span>';
                }
            }
            return customPaginationHtml;
        }
    })

    function selectCase() {
        $("#caseLable>li").on("click", function () {
            $("#caseLable>li").removeClass("active");
            $(this).addClass("active");
            var type = $(this).attr("data-type");
            getCaseList(type);
        })
    }
    function solutionCase() {
        var loading = layer.load(2);
        $.post("/dzhome/GetCaseList",
                {
                    id: 0,
                    pageIndex: 1,
                    pageSize: 48,
                },
                 function (data) {
                     console.log(data);
                     layer.close(loading);
                     if (data.isok) {
                         for (var i = 0; i < data.cases.length; i++) {
                             var html = '<img class="imgUrl" src="' + data.cases[i].QrcodePath + '" />'
                             $("#solutionCase .case-item").eq(i).append(html);
                         }
                         

                     } else {
                         layer.msg(data.msg);
                     }

                 });
    }
    function getCaseList(type) {
        //单页版:4  企业版:2  电商版:6  餐饮版:18  行业版:27  专业版:37
        var loading = layer.load(2);
        $.post("/dzhome/GetCaseList",
            {
                id: type,
                pageIndex: 1,
                pageSize: 9,
            },
            function (data) {
                console.log(data);
                layer.close(loading);
                if (data.isok) {
                    $("#caseList").empty();
                    for (var i = 0; i < data.cases.length; i++) {
                        var html = '<div  class="case-item">' +
                                        '<div class="img">' +
                                            '<img class="imgUrl" src="' + data.cases[i].QrcodePath + '" />' +
                                        '</div>' +
                                        '<p class="name">' + data.cases[i].casename + '</p>' +
                                 '</div>'
                        $("#caseList").append(html);
                    }
                    clickLarge("#caseList .imgUrl");
                } else {
                    layer.msg(data.msg);
                }
            });
    }
    function setAgentWebSiteQuetion(name, phone, content) {
        var loading = layer.load(2);
        $.post("/agentmanager/SetAgentWebSiteQuetion",
            {
                userName: name,
                telephone: phone,
                question: content,
            },
            function (data) {
                layer.close(loading)
                console.log(data);
                if (data.isok) {
                    layer.msg(data.msg);
                    $("#name").val("");
                    $("#phone").val("");
                    $("#content").val("");
                } else {
                    layer.msg(data.msg);
                }
            })
    }
    //查看大图
    var clickLarge = function clickLarge(ele) {
        $(ele).on("click", function (event) {
            event.stopPropagation();
            var imgUrl = $(this).attr("src");
            $("#largeUrl").attr("src", imgUrl);
            $(".large-box").css("display","flex");
            $("html,body").css("overflow", "hidden")
          //  console.log(imgUrl)
        })
    }

  
    //关闭大图
    $("#closeLarge").on("click", function () {
        $(".large-box").css("display", "none");
        $("html,body").css("overflow", "")

    })
}