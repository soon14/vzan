var http = require('http');
var url = require('url');
var util = require('util');
var qureystring = require('querystring');

// get
// http.createServer(function(req,res){
//     res.writeHead(200,{'Content-Type':'text/plain;charset=utf-8'});

//     var params=url.parse(req.url,true).query;
//     res.write('网站名:'+params.name);
//     res.write("\n");
//     res.write('网站URL:'+params.url);
//     // res.end(util.inspect(url.parse(req.url,true)));
//     res.end()
// }).listen(3000);

// POST 请求的内容全部的都在请求体中，http.ServerRequest 并没有一个属性内容为请求体，原因是等待请求体传输可能是一件耗时的工作。
var postHTML =
    '<html><head><meta charset="utf-8"><title>菜鸟教程 Node.js 实例</title></head>' +
    '<body>' +
    '<form method="post">' +
    '网站名： <input name="name"><br>' +
    '网站 URL： <input name="url"><br>' +
    '<input type="submit">' +
    '</form>' +
    '</body></html>';

http.createServer(function (req, res) {
    var body = '';
    req.on('data', function (chunk) {
        body += chunk;
    })
    req.on('end', function () {
        // 解析参数
        body = qureystring.parse(body);
        // 设置响应头部信息及编码
        res.writeHead(200, {
            'Content-Type': 'text/html;charset=utf8'
        });

        if (body.name && body.url) {
            res.write('网站名：' + body.name);
            res.write('<br>');
            res.write('网站Url:' + body.url);

        }else{
            res.write(postHTML);
        }
        res.end();
    })
}).listen(3000);