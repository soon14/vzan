var express = require('express'); //引入模块
var cheerio = require('cheerio');
var superagent = require('superagent');
var fs = require("fs");
var app = express();
let Ut = require("./downLoad")
var OSS = require('ali-oss');
var http = require('http')
var request = require("request");
var url = "https://bbs.hupu.com"
app.get('/', async function (req, res, next) {
    superagent.get('https://bbs.hupu.com/selfie-1') //请求页面地址
        .end(function (err, sres) { //页面获取到的数据
            if (err) return next(err);

            var $ = cheerio.load(sres.text); //用cheerio解析页面数据
            var arr = [];

            $(".for-list").each(function (index, element) { //下面类似于jquery的操作，前端的小伙伴们肯定很熟悉啦
                var $eleItem = $(element).find('.truetit');
                for (let i = 0, len = $eleItem.length; i < len; i++) {
                    arr.push(url + $eleItem[i].attribs.href)
                    grabImg.img(url + $eleItem[i].attribs.href)
                }
            });
            res.send(arr);
        })
});
var grabImg = {
    img(url) {
        var imgUrl = []
        superagent.get(url)
            .end(function (err, ires) {

                if (err)
                    return next(err);
                var $in = cheerio.load(ires.text);

                $in('.quote-content').each(function (Index, ele) {
                    var $inele = $in(ele).find("img");
                    for (let j = 0, jen = $inele.length; j < jen; j++) {
                        // var imgPath = "/" + j + "." + $inele[j].attribs.src.split(".").pop();
                        // fs.writeFile(__dirname + "/public" + imgPath, imgData, "binary", function (err) {
                        //     console.log(err);
                        // })
                        grabImg.downLoad($inele[j].attribs.src)


                        // imgUrl.push($inele[j].attribs.src)
                        // grabImg.upLoadBaseImg($inele[j].attribs.src)
                        // let opts = {
                        //     url: $inele[j].attribs.src,
                        // };
                        // let path = "./" + j + ".jpg";
                        // Ut.downImg(opts, path)
                    }
                    // console.log(imgUrl)
                })
            })
    },


    saveImage(url, path) {
        http.get(url, function (req, res) {
            var imgData = '';
            req.on('data', function (chunk) {
                imgData += chunk;
            })
            req.setEncoding('binary');
            req.on('end', function () {
                fs.writeFile(path, imgData, 'binary', function (err) {
                    console.log('保存图片成功' + path)
                })
            })
        })
    },
    upLoadBaseImg(urlT) {
        const client = new OSS({
            region: 'oss-cn-hangzhou',
            accessKeyId: 'LTAIBP8SaLnUNWnE',
            accessKeySecret: '2oLe5CYTuyGmqxZZw1CqTxaGg0ORNT',
            bucket: 'kaaden-upload'
        });


        var url = client.signatureUrl(urlT);
        console.log(url)
    },

    downLoad(url) {
        var startTime = new Date().getTime();
        url = encodeURI(url);
        var fileName = url.split('/').pop();
        request(url).on('response', function () {
                var endTime = new Date().getTime();
                console.log('downloading..%s..%ss', url, (endTime - startTime) / 1000);
            })
            //rs.pipe(destination, [options]);
            /**
             * destination 必须一个可写入流数据对象
             * [opations] end 默认为true，表示读取完成立即关闭文件；
             */
            .pipe(fs.createWriteStream(__dirname + fileName))
            .on('error', function () {
                console.log("failed to download");
            });
    }
}
app.listen(8888, function () {
    console.log('抓取成功~~~');
});