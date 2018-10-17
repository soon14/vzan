var querystring = require('querystring'); //序列化
var express = require('express');
var app = express();
var fs = require("fs");
var bodyParser = require('body-parser');
var multer = require('multer'); //接受文件上传配置
var sql = require('./sql')

app.use('/uploads', express.static('uploads')); //使用static托管静态文件（访问项目中的图片或者文件）
// 创建 application/x-www-form-urlencoded 编码解析
var urlencodedParser = bodyParser.urlencoded({
    extended: false
})

app.all("*", function (req, res, next) {
    res.header("Access-Control-Allow-Origin", "*");
    res.header("Access-Control-Allow-Headers", "X-Requested-With");
    res.header("Access-Control-Allow-Methods", "PUT,POST,GET,DELETE,OPTIONS");
    res.header("X-Powered-By", ' 3.2.1')
    res.header("Content-Type", "application/json;charset=utf-8");
    next();
})
app.get('/', function (req, res) {
    res.send("hello world 老乡我是.net运行的");
})

var upload = multer({
    dest: './uploads'
}).any()

//登陆
app.post('/login', upload, function (req, res) {
    console.log(req.body)
    var sqlite = 'SELECT * FROM user';

    sql.query(sqlite).then(data => {
        let _g = {}
        let _findname = data.find(f => f.name == req.body.name)
        let _findpassword = data.find(f => f.password == req.body.password)
        if (_findname && _findpassword) {
            _g.isok = true
            _g.msg = '查找到数据'

        } else {
            _g.isok = false
            _g.msg = '您输入的帐号或者密码不正确，请重新输入。'
        }
        
        res.end(JSON.stringify(_g));
    })



})

app.post('/touxiang', upload, function (req, res, next) {
    // console.log(req.files)
    console.log(req.body)

    var des_file = './uploads' + "/" + req.files[0].originalname;
    fs.readFile(req.files[0].path, function (err, data) {
        fs.writeFile(des_file, data, function (err) {
            if (err) {
                console.log(err);
            } else {
                let img = 'http:localhost:8081/uploads/' + req.files[0].originalname
                response = {
                    message: 'File uploaded successfully',
                    filename: req.files[0].originalname,
                    img: img,

                };
            }
            fs.unlink(req.files[0].path, function (err, data) {
            });
            console.log(response);
            res.end(JSON.stringify(response));
        });
    });
    var addSql = 'INSERT INTO news(title,content,img) VALUES(?,?,?)';
    // var addSql = 'INSERT INTO user(name,password) VALUES(?,?)';
    // var addSqlParams = [req.body.title, req.body.content, req.files[0].originalname];
    var addSqlParams = [req.body.name, req.body.password];
    sql.insert(addSql, addSqlParams);
})

app.post('/kaaden', urlencodedParser, function (req, res) {
    let _array = {}
    let appid = req.body.appid
    if (appid == '3') {
        _array.isok = true
        _array.msg = [{
            name: 'Technology ',
            num: 25
        }, {
            name: 'Development',
            num: 11
        }, {
            name: 'News',
            num: 11
        }, {
            name: 'HTML5&CSS3',
            num: 11
        }, {
            name: 'Photography',
            num: 11
        }, {
            name: 'JavaScript',
            num: 11
        }, {
            name: 'Design',
            num: 11
        }, {
            name: 'Tutorials',
            num: 11
        }, {
            name: 'Web Design',
            num: 11
        }, {
            name: 'Other',
            num: 11
        }];
        _array.data = [{
                img: './img/c1.png',
                icon: "./img/l1.png",
                u: 'A good old Standard Post for Computer Clusters',
                list: [{
                        img: './img/l2.png',
                        name: 'June 3 , 2013'
                    },
                    {
                        img: './img/l3.png',
                        name: '12 Comments'
                    },
                    {
                        img: './img/l4.png',
                        name: 'Public ,News'
                    },
                    {
                        img: './img/l5.png',
                        name: '124 Views'
                    },
                    {
                        img: './img/l6.png',
                        name: '18'
                    },
                ],
                content: "The components of a cluster are usually connected to each other through fast local area networks (“LAN”), with each node (computer used as a server) running its own instance of an operating system.Computer clusters emerged as a result of convergence of a number of computing trends including the availability of low cost microprocessors, high speed networks, and software for high performance distributed computing.",

            },
            {
                img: './img/c2.png',
                icon: "./img/l7.png",
                u: 'A good old Standard Post for Computer Clusters',
                list: [{
                        img: './img/l2.png',
                        name: 'June 3 , 2013'
                    },
                    {
                        img: './img/l3.png',
                        name: '12 Comments'
                    },
                    {
                        img: './img/l4.png',
                        name: 'Public ,News'
                    },
                    {
                        img: './img/l5.png',
                        name: '124 Views'
                    },
                    {
                        img: './img/l6.png',
                        name: '18'
                    },
                ],
                content: "The components of a cluster are usually connected to each other through fast local area networks (“LAN”), with each node (computer used as a server) running its own instance of an operating system.Computer clusters emerged as a result of convergence of a number of computing trends including the availability of low cost microprocessors, high speed networks, and software for high performance distributed computing.",

            }
        ]
    } else {
        _array.isok = false
        _array.msg = '无法查询'
    }
    res.json(_array)
})
var server = app.listen(8081, function () {
    var host = server.address().address;
    var port = server.address().port;
    console.log("应用实例，访问地址为 http://%s:%s", host, port)
})