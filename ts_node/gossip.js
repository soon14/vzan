const svgCaptcha = require('svg-captcha');
const sql = require('./sql');
const nodemailer = require('nodemailer');
const mailTransport = nodemailer.createTransport({
    host: 'smtp.qq.com',
    secureConnection: true, // 使用SSL方式（安全方式，防止被窃取信息）
    auth: {
        user: '794102675@qq.com',
        pass: 'thhthdbngshwbbba'
    },
});
const mine = {
    "css": "text/css",
    "gif": "image/gif",
    "html": "text/html",
    "ico": "image/x-icon",
    "jpeg": "image/jpeg",
    "jpg": "image/jpeg",
    "js": "text/javascript",
    "json": {
        'Content-Type': 'application/json'
    },
    "pdf": "application/pdf",
    "png": "image/png",
    "svg": "image/svg+xml",
    "swf": "application/x-shockwave-flash",
    "tiff": "image/tiff",
    "txt": "text/plain",
    "wav": "audio/x-wav",
    "wma": "audio/x-ms-wma",
    "wmv": "video/x-ms-wmv",
    "xml": "text/xml"
};


//验证码
exports.idenSvg = function (req, res) {
    var captcha = svgCaptcha.create({
        inverse: false, //翻转颜色
        fontSize: 36, //字体大小
        noise: 2, //噪声线条数
        width: 80,
        height: 30,
    })
    req.session.loginCheck = captcha.text.toLowerCase();

    res.cookie("captcha", req.session);
    res.setHeader("Content-Type", mine['svg']);
    res.write(String(captcha.data));
    res.end();
}
exports.login = function (req, res) {
    var vm = {},
        reb = req.body.vm,
        sqlite = 'SELECT * FROM gossip_login Where account=' + '"' + reb.account + '"' + ' and password=' + '"' + reb.password + '"';
    sql.query(sqlite).then(data => {
        if (data.length) {
            vm.isok = true;
            vm.msg = '查找到数据';
            vm.userId = data[0].userId;
        } else {
            vm.isok = false;
            vm.userId = null;
            vm.msg = '您输入的帐号或者密码不正确，请重新输入。';
        }
        res.writeHead(200, mine['json']);
        res.end(JSON.stringify(vm));
    })
}
exports.register = function (req, res) {
    var vm = {},
        reb = req.body.vm,
        addSql = 'INSERT INTO gossip_login(account,password,email) VALUES(?,?,?)',
        addSqlParams = [reb.reaccount, reb.repassword, reb.email];
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '注册成功'
            vm.account = reb.reaccount
        } else {
            vm.isok = false
            vm.msg = '注册失败'
            vm.account = null
        }
        res.writeHead(200, mine['json']);
        res.end(JSON.stringify(vm));
    })
}
exports.checkAccount = function (req, res) {
    var vm = {},
        account = req.body.account,
        sqlite = 'SELECT * FROM gossip_login Where account=' + '"' + account + '"';
    sql.query(sqlite).then(data => {
        if (data.length) {
            vm.isok = false
            vm.msg = '账号已存在'
            vm.userId = data[0].id
        } else {
            vm.isok = true;
            vm.msg = '账号可用';
        }
        res.writeHead(200, mine['json']);
        res.end(JSON.stringify(vm));
    })
}
exports.sendMail = function (req, res) {
    let mail = req.body.email
    var options = {
        from: '"Gossip" <794102675@qq.com>',
        to: mail,
        // cc         : ''  //抄送
        // bcc      : ''    //密送
        subject: '一封来自Gossip的邮件',
        text: '一封来自Gossip的邮件',
        html: '<h1 style="color:#1F4A89">Gossip用户,您好！</h1><br/><p>感谢您使用Gossip,请点击下面的链接进行密码找回：</p><br/><p>http://kaaden.orrzt.com/</p><br/><p>Gossip</p><br/><p>——————————————————————————————</p><br/><p>此邮件由Gossip系统发出，系统不接收回信，请勿直接回复。</p>',
    };

    mailTransport.sendMail(options, function (err, msg) {
        let vm = {}
        res.writeHead(200, mine['json']);
        if (err) {
            vm.isok = false
            vm.msg = "发送失败"
            res.end(JSON.stringify(vm))
        } else {
            vm.isok = false
            vm.msg = "已发送,请注意查收"
            res.end(JSON.stringify(vm))

        }
    });
}
//type=1查找该用户id，type=2查找该用户userid
exports.findAccount = function (req, res) {
    var vm = {},
        userId = req.body.userId,
        account = req.body.account,
        type = Number(req.body.type),
        sqlite = type === 1 ? 'SELECT * FROM gossip_login Where account=' + '"' + account + '"' : 'SELECT * FROM gossip_login Where userId=' + '"' + userId + '"';
    sql.query(sqlite).then(data => {
        if (data.length) {
            vm.isok = true
            vm.msg = '成功'
            type === 1 ? vm.id = data[0].id : vm.userId = data[0].userId;
            vm.bg = data[0].bg_logo
        } else {
            vm.isok = false;
            vm.msg = '失败';
        }
        res.writeHead(200, mine['json']);
        res.end(JSON.stringify(vm));
    })

}
exports.updateInfo = function (req, res) {
    var vm = {},
        reb = req.body.vm,
        userid = Number(reb.id + 1),
        sqlite = 'SELECT * FROM gossip_login Where Id=' + '"' + reb.id + '"',
        addSql = 'UPDATE gossip_login SET logo=?,name=?,sex=?,birthday=?,userId=? WHERE Id=?',
        addSqlParams = [reb.logo, reb.name, reb.sex, reb.birth, userid, reb.id],
        insertSql = 'INSERT INTO gossip_relation(userId,friendId,isType) VALUES(?,?,?)';
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            sql.query(sqlite).then(lres => {
                if (lres.length) {
                    var insertParams_first = ['311', lres[0].userId, '1'];
                    var insertParams_second = [lres[0].userId, '311', '0'];
                    sql.insert(insertSql, insertParams_first)
                    sql.insert(insertSql, insertParams_second)
                    vm.isok = true
                    vm.msg = '更新成功';
                    vm.userId = lres[0].userId;
                    res.writeHead(200, mine['json']);
                    res.end(JSON.stringify(vm));
                }
            })
        } else {
            vm.isok = false
            vm.msg = '更新失败';
            res.writeHead(200, mine['json']);
            res.end(JSON.stringify(vm));
        }

    });
}
exports.friendLst = function (req, res) {
    var vm = {},
        ids = [],
        userId = req.body.userId,
        sqlite = 'SELECT * FROM gossip_relation Where userId=' + '"' + userId + '"';
    sql.query(sqlite).then(data => {
        for (let i = 0, len = data.length; i < len; i++) {
            ids.push(data[i].friendId)
        }
        ids = ids.join(',')
        var sqlite_PE = 'SELECT * FROM gossip_login Where find_in_set(userId,' + '"' + ids + '")';
        sql.query(sqlite_PE).then(lres => {
            if (lres.length) {
                let array = []
                for (let j = 0, len = lres.length; j < len; j++) {
                    array.push({
                        imgUrl: lres[j].logo,
                        name: lres[j].name,
                        id: lres[j].userId
                    })
                }
                vm.isok = true
                vm.list = array
            } else {
                vm.isok = false
                vm.list = []
            }
            res.writeHead(200, mine['json']);
            res.end(JSON.stringify(vm));
        })
    })
}
//req.body.state 1更新背景图片  
exports.updateUserInfo = function (req, res) {
    let vm = {}
    let state = Number(req.body.vm.state)
    if (state === 1) {
        var addSql = 'UPDATE gossip_login SET bg_logo=? WHERE userId=?',
            addSqlParams = [req.body.vm.bg, req.body.vm.userId];
    }

    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true;
            vm.msg = '更新成功';
            vm.lst = req.body.vm;
        } else {
            vm.isok = false;
            vm.msg = '更新失败';
            vm.lst = []
        }
        res.writeHead(200, mine['json']);
        res.end(JSON.stringify(vm));
    })

}

exports.addSuccess = function (req, res) {
    var vm = {},
        reb = req.body.vm,
        updateSql = 'UPDATE gossip_friend SET isFriend=? WHERE userId=?',
        updatePara = [1, reb.userInfo.userId],
        updatePara2 = [1, reb.friendInfo.userInfo.userId],
        insertParams_first = [reb.userInfo.userId, reb.friendInfo.userInfo.userId, '0'],
        insertParams_second = [reb.friendInfo.userInfo.userId, reb.userInfo.userId, '1'],
        insertSql = 'INSERT INTO gossip_relation(userId,friendId,isType) VALUES(?,?,?)',
        addSql = 'INSERT INTO gossip_chatlst(userId,friendId,friendImg,friendName,lastMsg,msgType,time) VALUES(?,?,?,?,?,?,?)',
        addSqlParams = [reb.userInfo.userId, reb.friendInfo.userInfo.userId, reb.friendInfo.userInfo.logo, reb.friendInfo.userInfo.name, '你们已成为好友，快聊天吧', 0, moment(new Date()).format('YYYY-MM-DD HH:mm:ss')],
        addSqlParams2 = [reb.friendInfo.userInfo.userId, reb.userInfo.userId, reb.userInfo.logo, reb.userInfo.name, '你们已成为好友，快聊天吧', 0, moment(new Date()).format('YYYY-MM-DD HH:mm:ss')];
    sql.insert(updateSql, updatePara).then(data => {
        if (data) {
            sql.insert(updateSql, updatePara2).then(ldata => {
                if (ldata) {
                    sql.insert(insertSql, insertParams_first)
                    sql.insert(insertSql, insertParams_second)
                    sql.insert(addSql, addSqlParams)
                    sql.insert(addSql, addSqlParams2)
                    vm.isok = true;
                    vm.msg = "成功"
                } else {
                    vm.isok = false;
                    vm.msg = "网络错误，请稍后重试"
                }
                res.writeHead(200, mine['json']);
                res.end(JSON.stringify(vm));
            })
        } else {
            vm.isok = false;
            vm.msg = "网络错误，请稍后重试"
            res.writeHead(200, mine['json']);
            res.end(JSON.stringify(vm));
        }
    })


}