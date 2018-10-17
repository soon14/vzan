const express = require('express');
const app = express();
const fs = require("fs");
const path = require('path');
const bodyParser = require('body-parser');
const session = require('express-session');
const multer = require('multer');
const cookieParase = require('cookie-parser');
const manger = require("./manger");
const gossip = require("./gossip");
const urlencodedParser = bodyParser.urlencoded({
    extended: true
});
const upload = multer({
    dest: './public/image'
}).any();

//设置允许跨域 以及样式加载ContentType属性指定响应的 HTTP内容类型
app.all("*", function (req, res, next) {
    const strs = req.originalUrl.split("/")
    switch (strs[1]) {
        case 'static':
            res.header("Content-Type", "text/css;charset=utf-8");
            break;
        case 'uploads':
            res.header("Content-Type", "image/jpeg");
            break;
    }
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "X-Requested-With");
    res.header("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
    next();
})
app.use(cookieParase())
//设置可访问静态资源
app.use('/static', express.static(path.join(__dirname, 'public')));
//只要页面由刷新，session值就会被保存，如果为false则只要半小时以后不管有没有操作，session都会消失
app.use(session({
    secret: 'keyboard cat', //值可以随便取
    resave: false,
    saveUninitialized: true,
    cookie: {
        maxAge: 1000 * 60 * 30
    },
    rolling: true
})); //配置session

app.get('/', function (req, res) {
    req.session.user = undefined
    res.end(fs.readFileSync(path.join(__dirname, 'login.html')));
})
app.get('/home', function (req, res) {
    if (req.session.user) {
        res.end(fs.readFileSync(path.join(__dirname, 'home.html')));
    } else {
        res.end(fs.readFileSync(path.join(__dirname, 'login.html')));
    }
})
//登陆验证
app.post("/login", urlencodedParser, manger.login)
//用户信息
app.post("/userInfo", manger.userInfo)
//查询内容
app.post("/newsList", urlencodedParser, manger.queryContent)

//添加内容
app.post("/addnews", urlencodedParser, manger.addContent)
//更新内容
app.post("/updateNews", urlencodedParser, manger.updateContent)
//删除内容
app.post("/delete", urlencodedParser, manger.deleteContent)
//查询分类
app.post("/classify", urlencodedParser, manger.queryClassify)
//添加分类
app.post("/addType", urlencodedParser, manger.addClassify)
//更新分类
app.post("/updateType", urlencodedParser, manger.updateClassify)
//删除分类
app.post("/deleType", urlencodedParser, manger.deleteClassify)
//查询所有用户
app.post('/userlist', urlencodedParser, manger.queryUser)
//增加用户
app.post('/adduser', urlencodedParser, manger.addUser)
//更新用户
app.post("/updateUser", urlencodedParser, manger.updateUser)
//删除用户
app.post('/deleUser', urlencodedParser, manger.deleteUser)

/**
 * @param {blogIden生成验证码}
 */
app.get("/Gossip", function (req, res) {
    res.end(fs.readFileSync(path.join(__dirname, 'index.html')))
})
app.get("/gossipIden", gossip.idenSvg)
app.post("/gossipLogin", urlencodedParser, gossip.login)
app.post("/gossipRegister", urlencodedParser, gossip.register)
app.post("/gossipCheck", urlencodedParser, gossip.checkAccount)
app.post("/gossipSendMail", urlencodedParser, gossip.sendMail)
app.post("/gossipFind", urlencodedParser, gossip.findAccount)
app.post("/gossipUpdate", urlencodedParser, gossip.updateInfo)
app.post("/gossipConcactLst", urlencodedParser, gossip.friendLst)
app.post("/gossipUpdateUserInfo", urlencodedParser, gossip.updateUserInfo)
app.post("/gossipFriend", urlencodedParser, gossip.addSuccess)
// //错误页
app.get('*', function (req, res) {
    res.end(fs.readFileSync(path.join(__dirname, 'error.html')));
});
//图片上传
app.post('/uploadFile', upload, function (req, res) {
    let response = {}
    let des_file = './public/image' + "/" + req.files[0].originalname;
    fs.readFile(req.files[0].path, function (err, data) {
        fs.writeFile(des_file, data, function (err) {
            if (err) {
                console.log(err);
            } else {
                let img = 'http://kaaden.orrzt.com/static/image/' + req.files[0].originalname
                response = {
                    isok: true,
                    filename: req.files[0].orisginalname,
                    img: img,
                };
            }
            fs.unlink(req.files[0].path);
            res.writeHead(200, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify(response));
        });
    });
})
var server = app.listen(80, function () {
    var host = server.address().address;
    var port = server.address().port;
    console.log("success", host, port)
})