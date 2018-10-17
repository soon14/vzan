const express = require('express');
const app = express();
const bodyParser = require('body-parser');
const urlencodedParser = bodyParser.urlencoded({
    extended: true
});
app.post("/openid",urlencodedParser, async (req, res) => {
    const Ut = require("./common");
    try {
        console.log(req.body);
        let appId = "wx4b776e0585b5de94";
        let secret = "f44e3629c5456a311402d84f00642ee0";
        let js_code= req.body.code;
        let opts = {
            url: `https://api.weixin.qq.com/sns/jscode2session?appid=${appId}&secret=${secret}&js_code=${js_code}&grant_type=authorization_code`
        }
        let r1 = await Ut.promiseReq(opts);
        r1 = JSON.parse(r1);
        console.log(r1);
        res.json(r1);
    } catch (e) {
        console.log(e);
        res.json('');
    }
})

var server = app.listen(8096, function () {
    var host = server.address().address;
    var port = server.address().port;
    console.log("success", host, port)
})