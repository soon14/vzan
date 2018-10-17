; (function () {
    /**
 * Created by Administrator on 2017/10/31 0031.
 */
    $(window).load(function () {
        var mySwiper = new Swiper('.swiper-container', {
            direction: 'horizontal',
            //loop: true,
            autoplay: 3000,
            autoplayDisableOnInteraction: false,
            speed: 800,
            grabCursor: true,
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
    })
    $("#btnBanner").click(function () {
        $("html, body").animate({
            scrollTop: $("#submit").offset().top
        }, { duration: 500, easing: "swing" });
        return false;
    });



})()
