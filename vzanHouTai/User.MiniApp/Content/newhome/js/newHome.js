; (function () {
    $(function () {

        resizeVnavListHeight()
        $(window).resize(function () {
            resizeVnavListHeight()
        })
        function resizeVnavListHeight() {
            var navHeight = $("#top").height();
            var navSwiperHeight = navHeight + $("#bannerSwiper").height()  //导航+轮播高度
            var vNavscollTop = navSwiperHeight - $(window).scrollTop(); //计算文档卷出距离对比
            var navListHeight = $(window).height() - 100;
            var navListLength = $(".v-nav-list>li").length;
            var navItemHeight = Math.floor(navListHeight / navListLength * 100) / 100;
            $(".v-nav-list>li").css({ "margin-top": navItemHeight / 2, "margin-bottom": navItemHeight / 2 })
            if ($(window).scrollTop() < navSwiperHeight) { //$(window).scrollTop() <= 0 ||
                $(".v-nav").css("top", vNavscollTop)
            }
            $(window).scroll(function () {
                vNavscollTop = navSwiperHeight - $(window).scrollTop();
                if ($(window).scrollTop() <= 0) {
                    $(".v-nav").css("top", vNavscollTop)
                } else if ($(window).scrollTop() > 0) {
                    if ($(window).scrollTop() <= navSwiperHeight) {
                        $(".v-nav").css("top", vNavscollTop + navHeight)
                    } else if ($(window).scrollTop() >= navSwiperHeight) {
                        $(".v-nav").css("top", navHeight + 10)
                    }
                }

            })
        }

        $(window).load(function () {
            $(".v-nav").fadeIn();
            var arrOffsetTop = [
                $('#first').offset().top,
                $('#second').offset().top,
                $('#fourth').offset().top,
                $('#fifth').offset().top,
                $('#sixth').offset().top,
                $('#seventh').offset().top,
                $('#eighth').offset().top,
                $('#ninth').offset().top,
                $('#solution-box').offset().top,
                $('#market').offset().top,
                $('#app').offset().top,
                $('#eleventh').offset().top,
                $('#miniAppNews').offset().top,
            ];
            getCurrentPosition()
            $(window).scroll(function () {
                getCurrentPosition()
            });
            function getCurrentPosition() {
                for (var i = 0; i < $('.home-section').length; i++) {
                    if ($(this).scrollTop() > arrOffsetTop[i] - 200) {  // 减去一个固定值，定位准确点
                        $('.v-nav-list>li').eq(i).addClass('vnav-green-active').siblings().removeClass('vnav-green-active');
                    }
                }
            }
            /* 点击事件 */
            $('.v-nav-list>li').click(function () {
                //$('.v-nav-list>li').removeClass('vnav-green-active');
                console.log($(this));
                $('body, html').animate({ scrollTop: arrOffsetTop[$(this).index()] - 80 }, 500);
                setTimeout(function () {
                    $(this).addClass('vnav-green-active').siblings().removeClass('vnav-green-active');
                }, 300)
            });
        })

        //banner轮播图
        var bannerSwiper = new Swiper('#bannerSwiper', {
            autoplay: {
                //delay:1000,
                disableOnInteraction: false
            },
            loop: true,
            speed:1000,
            navigation: {
                prevEl: '.swiper-button-prev',
                nextEl: '.swiper-button-next',
            },
            grabCursor: true,
            pagination: {   //分页器
                el: '.swiper-pagination',
                type: 'custom',
                renderCustom: function (swiper, current, total) {
                    var customPaginationHtml = "";
                    for (var i = 0; i < total; i++) {
                        if (i == (current - 1)) {
                            customPaginationHtml += '<span class="swiper-pagination-customs swiper-pagination-customs-active"></span>';
                        } else {
                            customPaginationHtml += '<span class="swiper-pagination-customs"></span>';
                        }
                    }
                    return customPaginationHtml;
                }
            },
        })


        //扫描登录
        function wxLogin() {
            var key = $('#sessid').val();
            $.ajax({
                type: "POST",
                url: "/dzhome/wxlogin",
                xhrFields: {
                    withCredentials: true
                },
                data: { wxkey: key, usertype: 1 },
                success: function (returnData) {
                    if (returnData.success) {
                        var gourl = "";
                        var cookiekey = "";
                        if (returnData.code == "-2") {
                            gourl = "/dzhome/wxreg";
                            cookiekey = "regphoneuserid";
                        }
                        else {
                            gourl = "/dzhome/casetemplate";
                            cookiekey = "dz_UserCookieNew";
                        }

                        var url = window.location.host;
                        var domain = "";
                        if (url.indexOf("www.") != -1) {
                            domain = url.replace("www.", ".");
                        }
                        else {
                            domain = "." + url;
                        }
                        $.cookie(cookiekey, returnData.msg, {
                            expires: 7,
                            path: '/',
                            domain: domain
                        });

                        window.location.href = gourl;
                    }
                }
            });
        }
        var timer = setInterval(wxLogin, 1000);

        //播放视频
        var video = document.getElementById("dsVideo")

        //点击默认播放视频
        $("#btnPlay").on("click", function () {
            $("#videoMasking").addClass("video-masking-gif")
            $("#btnPlay").hide();
            $("#dsVideo").attr("src", "http://vzan-img.oss-cn-hangzhou.aliyuncs.com/upload/video/20180205/ef92ff22b9fd4c899c072b1c2d3731df.mp4").trigger('play');
            isPlayed = true;
            //$("#videoMasking").hide();
            $("#dsVideo").trigger('play');
            //$(".video-swiper-play[data-videoindex='0']").css("color", "#63b504").siblings().css("color", "#666")
            //playVideo(videoArr[3])
        })


        //监听视频播放状态
        videoPlayListener();
        function videoPlayListener() {
            video.addEventListener("loadedmetadata", function () {
                //console.log("加载中。。。")
            })
            video.addEventListener("canplay", function () {
                $(".next-video-warn").hide();
                $("#videoMasking").hide();
                $("#dsVideo")[0].play();
                //console.log("加载完成")
            })
        }

        //关于我们视频
        $(".video-play").on("click", function () {
            $(".video-aboutus-masking").show();
            $(".video-aboutus-masking>video").trigger("play");
        })
        $(".video-aboutus-masking").on("click", function () {
            $(".video-aboutus-masking>video").trigger("pause");
            $(this).hide();
        })
        //小程序动态
        var vm = new Vue({
            el: "#news-box",
            data: {
                pageIndex: 1,
                pageSize: 5,
                pageIndex_deep: 1,
                pageSize_deep: 5,
                list: [],
                time: "",
                list_deep: [],
                pageChange: 0,
                deep_time: "",
                list_wiki: "",
                img: "",
                img_deep: "",
                title: "",
                title_deep: "",
                title_wiki: "",
                img_wiki: "",
                listOne: "",
                listTwo: "",
                id_wiki: ""
            },
            //初始化
            mounted: function () {
                //var that = this;
                this.getNewsList(function () {
                    //console.log(that.list)
                });
                this.getDeepList(function () {
                    //console.log(that.list_deep)
                })
                this.getWiki(function () {
                    //console.log(that.list_wiki) 
                })

            },
            methods: {
                getNewsList: function (fun) {
                    var that = this;
                    $.ajax({
                        url: "http://testwtApi.vzan.com/apiMiniAppGw/NewsList",
                        type: "get",
                        data: "&pageIndex=" + that.pageIndex + "&pageSize=" + that.pageSize + "&type=" + 0,
                        dataType: "json",
                        success: function (res) {
                            if (res.isok) {
                                if (res.obj != undefined && res.obj != null && res.obj.length > 0) {
                                    that.time = data_string(res.obj[0].addtime) + "-" + time_string(res.obj[0].addtime)
                                    that.img = res.obj[0].ImgPath
                                    that.title = res.obj[0].Title
                                    that.listOne = res.obj[0]
                                    that.list = res.obj
                                    that.list.splice(0, 1)
                                }
                                else {
                                    console.log("newhome.js:NewsList:obj=" + res.obj)
                                }
                                if (fun) {
                                    fun();
                                }

                            }
                        }
                    });
                },
                getDeepList: function (fun) {
                    var that = this;
                    $.ajax({
                        url: "http://testwtapi.vzan.com/apiMiniAppGw/NewsList",
                        type: "get",
                        data: "&pageIndex=" + that.pageIndex_deep + "&pageSize=" + that.pageSize_deep + "&type=" + 1,
                        dataType: "json",
                        success: function (res) {
                            if (res.isok) {
                                if (res.obj != undefined && res.obj != null && res.obj.length > 0) {
                                    that.deep_time = data_string(res.obj[0].addtime) + "-" + time_string(res.obj[0].addtime)
                                    that.img_deep = res.obj[0].ImgPath
                                    that.title_deep = res.obj[0].Title
                                    that.listTwo = res.obj[0]
                                    that.list_deep = res.obj
                                    that.list_deep.splice(0, 1)
                                } else {
                                    console.log("newhome.js:NewsList:obj=" + res.obj)
                                }
                                if (fun) {
                                    fun();
                                }
                            }
                        }
                    });
                },
                getWiki: function (fun) {
                    var that = this
                    $.ajax({
                        url: "/dzhome/GetBkNewsList",
                        type: "post",
                        data: "&pageIndex=" + that.pageIndex + "&pagesize=" + that.pageSize,
                        dataType: "json",
                        success: function (res) {
                            if (res.isok == true && res.newsList != null) {
                                if (res.newsList != undefined && res.newsList != null && res.newsList.length > 0) {
                                    that.title_wiki = res.newsList[0].title
                                    that.img_wiki = res.newsList[0].imgUrl
                                    that.id_wiki = res.newsList[0].id
                                    that.list_wiki = res.newsList
                                    that.list_wiki.splice(0, 1)
                                }
                            } else {
                                console.log("newhome.js:GetBkNewsList:newsList=" + res.newsList)
                            }
                            if (fun) {
                                fun();
                            }
                        }

                    })
                },

                //判断是否有地址，无则跳转内置网页
                getnewsurl: function (newsitem) {
                    if (newsitem != undefined) {

                        return newsitem.NewsURL == "" ? "/dzhome/more_detail/" + newsitem.Id : newsitem.NewsURL;
                    }
                },
            }
        })

        function data_string(str) {
            var d = eval('new ' + str.substr(1, str.length - 2));
            var ar_date = [d.getFullYear(), d.getMonth() + 1];
            for (var i = 0; i < ar_date.length; i++) ar_date[i] = dFormat(ar_date[i]);
            return ar_date.join('-');

            function dFormat(i) { return i < 10 ? "0" + i.toString() : i; }
        }
        function time_string(str) {
            var d = eval('new ' + str.substr(1, str.length - 2));
            var ar_date = [d.getDate()];
            for (var i = 0; i < ar_date.length; i++) ar_date[i] = dFormat(ar_date[i]);
            return ar_date.join('-');

            function dFormat(i) { return i < 10 ? "0" + i.toString() : i; }
        }
        //行业解决方案轮播
        var solutionSwiper = new Swiper('#solutionSwiper', {
            autoplay: {
                //delay: 1000,
                disableOnInteraction: false
            },
            speed: 1000,
            loop: true,
            centeredSlides: true,
            slidesPerView: 3,
            navigation: {
                prevEl: '.swiper-button-prev',
                nextEl: '.swiper-button-next',
            },
            on: {
                transitionStart: function () {
                    var curIndex = this.realIndex;
                    toggleLabel(curIndex);
                }
            }
        })
        $("#solutionList>li").on("click", function () {
            var labelIndex = $(this).index();
            toggleLabel(labelIndex);
            solutionSwiper.slideToLoop(labelIndex, 1000, false);
        })

        function toggleLabel(index) {
            $("#solutionList>li").each(function (i, item) {
                $(item).children("span").removeClass("green-current-btn");
            })
            $("#solutionList>li").eq(index).children("span").addClass("green-current-btn")
        }

        //单页版：4，企业版：2，电商版：6，餐饮版：18，行业版：27，专业版：37
        getCaseImgData("4,2,6,18,27,37", 3);
        function getCaseImgData(id, pgSize) {
            var isCaseImgLoad = false;
            $.ajax({
                type: "POST",
                url: "/dzhome/GetCaseListByTemplate",
                data: { tids: id, pageSize: pgSize },
                success: function (data) {
                    //console.log(data)
                    var itemIndex = 0;
                    var casesItem;
                    if (data.isok && data.list != null && data.list != undefined) {
                        for (var i = 0; i < data.list.length; i++) {
                            casesItem = data.list[i]
                            itemIndex = i * 3;
                            for (var j = 0; j < casesItem.clist.length; j++) {
                                $("#itemCaseImgBox>img").eq(itemIndex).attr({ "class": "lazy", "data-original": casesItem.clist[j].coverPath })
                                $("#itemCaseQrcode>img").eq(itemIndex).attr({ "class": "lazy", "data-original": casesItem.clist[j].QrcodePath })
                                itemIndex++;
                            }
                            isCaseImgLoad = true;
                        }
                        if (isCaseImgLoad) {
                            var imgs = $(".template-item img")
                            for (var i = 0; i < imgs.length; i++) {
                                var src = imgs[i].data - src;
                                var href1 = window.location.href.substring(0, location.href.lastIndexOf('/') + 1);//IE
                                var href2 = window.location.href;//非IE
                                if (src == "" || src == href1 || src == href2) {
                                    imgs[i].parentNode.removeChild(imgs[i]);
                                };
                            }
                        }
                        $("img.lazy").lazyload();
                    }
                },
                error: function () {
                    console.log("error");
                }
            })

        }


        //提交信息
        $("#floaterBtn").click(function () {
            var urlenter;
            if ($.cookie("source") == "" || $.cookie("source") == null) {
                urlenter = "自然搜索";
            } else {
                urlenter = $.cookie("source");
            }
            submitInformation(0, urlenter);
        })
        function submitInformation(type, urlenter) {
            var name = "首页获取秘诀-" + urlenter;
            var phone = $("#floaterPhoneNum").val().trim();
            var typeValue = type;

            if (phone == "") {
                layer.msg("请填写完整信息");
                return;
            }
            if (!(/^1[34578]\d{9}$/.test(phone))) {
                layer.msg("请输入正确的手机号");
                return;
            }

            $.post("/DLPT/SendUserAdvisory",
                { Phone: phone, username: name, source: typeValue },
                function (data) {
                    if (data.isok) {
                        layer.msg("提交成功");
                        $("#floaterPhoneNum").val("");
                    } else {
                        console.log("newhome.js:SendUserAdvisory:error")
                    }
                })
        }
    })
})()
