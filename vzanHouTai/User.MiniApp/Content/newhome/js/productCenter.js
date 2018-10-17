$(function () {
    var solutionSwiper = new Swiper('#solutionSwiper', {
        autoplay: {
            delay: 1000,
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
    getUpdateLogList();
    function getUpdateLogList() {
        $.ajax({
            type: "POST",
            url: "/dzhome/GetUpdateLogList",
            data: { pageIndex: 1, pageSize: 7 },
            success: function (data) {
                if (data.isok) {
                    var dataList = data.dataObj.DataList;
                    for (var i = 0, len = dataList.length; i < len; i++) {
                        var html = '<li class="item">' +
                                        '<div class="f fc fj msg-title" >' +
                                            '<span class="fz-18">' + dataList[i].Title + '</span>' +
                                            '<span style="color:#999;" >' + dataList[i].UpdateTimeStr + '</span>' +
                                        '</div>' +
                                        '<div class="msg-content" >' +
                                             '<p>' + dataList[i].Content + '</p>' +
                                        '</div>' +
                                    '</li>'
                        $("#updataList").append(html);
                    }
                }
            }
        })
    }
})
