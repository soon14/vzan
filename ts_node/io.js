//服务器及页面响应部分
var express = require('express'),
    app = express(),
    sql = require('./sql'),
    msgArray = [],
    moment = require('moment'),
    server = require('http').createServer(app),
    io = require('socket.io').listen(server); //引入socket.io模块并绑定到服务器

var _ = require('underscore');
app.use('/', express.static(__dirname + '/www'));
server.listen(8989);
var arraySort = function (a, b) {
    if (a.time > b.time) {
        return -1
    } else {
        return 1;
    }
}
//socket部分
io.on('connect', function (socket) {
    socket.rIds = []
    //插入聊天记录
    socket.on('addChat', function (data) {
        var vm = {},
            addSql = 'INSERT INTO gossip_chatlst(userId,friendId,friendImg,friendName,lastMsg,msgType,time) VALUES(?,?,?,?,?,?,?)',
            addSqlParams = [data.userId, data.friendId, data.friendImg, data.friendName, data.lastMsg, data.msgType, moment(new Date()).format('YYYY-MM-DD HH:mm:ss')],
            sqlite = 'SELECT * FROM gossip_chatlst Where userId=' + '"' + data.userId + '"' + ' and friendId=' + '"' + data.friendId + '"';
        sql.query(sqlite).then(lres => {
            if (lres.length) {
                vm.isok = true
                vm.obj = lres
                vm.msg = "该好友已存在"
                socket.emit('addLst', vm);
            } else {
                sql.insert(addSql, addSqlParams).then(ldata => {
                    if (ldata) {
                        vm.isok = true
                        vm.msg = '添加成功'

                    } else {
                        vm.isok = false
                        vm.msg = '添加失败'
                    }
                    socket.emit('addLst', vm);
                })
            }
        })
    })
    //查找当前用户信息
    socket.on("userInfo", function (data) {
        let vm = {}
        var sqlite = 'SELECT * FROM gossip_login Where userId=' + '"' + data + '"';
        sql.query(sqlite).then(res => {
            if (res.length) {
                vm.isok = true

                vm.userInfo = {
                    account: res[0].account,
                    name: res[0].name,
                    logo: res[0].logo,
                    userId: res[0].userId
                }
                socket.emit('userInfo', vm);
            } else {
                vm.isok = false

                socket.emit('userInfo', vm);
            }

        })
    })
    //查找聊天列表
    socket.on("chatLst", function (data) {
        socket.userId = data
        var vm = {},
            sqlite = 'SELECT * FROM gossip_chatlst Where userId=' + '"' + data + '"';
        sql.query(sqlite).then(lres => {
            if (lres.length) {
                vm.isok = true
                for (let i = 0, len = lres.length; i < len; i++) {
                    lres[i].time = moment(lres[i].time).format('YYYY-MM-DD HH:mm:ss')
                    socket.rIds.push(lres[i].friendId)
                }
                lres = lres.sort(arraySort)
                vm.lst = lres
                socket.emit('chatLst', vm);
            } else {
                vm.isok = false
                vm.lst = []
                socket.emit('chatLst', vm);
            }
        })
    })
    socket.on("addChatMsg", function (data) {
        socket.friendId = data.friendId
        if (data.changeLine) {
            if (msgArray.length) {
                var addSql = 'INSERT INTO gossip_chatHistory(sId,rId,msgType,msg,time,slogo) VALUES(?,?,?,?,?,?)';
                for (let i = 0, len = msgArray.length; i < len; i++) {
                    var addSqlParams = [msgArray[i].sId, msgArray[i].rId, msgArray[i].msgType, msgArray[i].msg, moment(new Date()).format('YYYY-MM-DD HH:mm:ss'), msgArray[i].slogo];
                    sql.insert(addSql, addSqlParams).then(res => {
                        if (res) {
                            msgArray = []
                            io.sockets.emit('Msg', msgArray);
                        }
                    })
                    let newMsg = 0
                    var updateSql = 'UPDATE gossip_chatlst SET lastMsg=?,msgType=?,time=?,newMsg=? WHERE  (userId=? AND friendId=?) OR (userId=? AND friendId=?)';
                    var updateP = [msgArray[i].msg, msgArray[i].msgType, moment(new Date()).format('YYYY-MM-DD HH:mm:ss'), newMsg, msgArray[i].sId, msgArray[i].rId, msgArray[i].rId, msgArray[i].sId]
                    var sqlite = 'SELECT * FROM gossip_chatlst Where userId=' + '"' + socket.userId + '"';
                    sql.insert(updateSql, updateP).then(fres => {
                        if (fres) {
                            var vm = {}
                            sql.query(sqlite).then(lres => {
                                if (lres.length) {
                                    vm.isok = true

                                    for (let i = 0, len = lres.length; i < len; i++) {
                                        lres[i].time = moment(lres[i].time).format('YYYY-MM-DD HH:mm:ss')
                                    }
                                    lres = lres.sort(arraySort)
                                    vm.lst = lres
                                    socket.emit('chatLst', vm);
                                } else {
                                    vm.isok = false
                                    vm.lst = []
                                    socket.emit('chatLst', vm);
                                }
                            })

                        }

                    })

                }
            }
        }
        setTimeout(() => {
            data.changeLine = false
        }, 3000);
    })
    //发送消息
    socket.on("Msg", function (data) {
        let array = []
        let userId = Number(socket.userId)
        let friendId = Number(socket.friendId)
        let newMsg = 1
        data.sendDate = moment(new Date()).format('YYYY-MM-DD HH:mm:ss')

        var updateSql = 'UPDATE gossip_chatlst SET lastMsg=?,msgType=?,time=?,newMsg=? WHERE  (userId=? AND friendId=?) OR (userId=? AND friendId=?)';
        var updateP = [data.msg, data.msgType, data.sendDate, newMsg, data.sId, data.rId, data.rId, data.sId]
        if (data.sId == userId && data.rId == friendId) {
            array.push(data)
            socket.broadcast.emit('Msg', array);
            sql.insert(updateSql, updateP)
        }
        msgArray.push(data)
        if (msgArray.length > 10) {
            var addSql = 'INSERT INTO gossip_chatHistory(sId,rId,msgType,msg,time,slogo) VALUES(?,?,?,?,?,?)';
            for (let i = 0, len = msgArray.length; i < len; i++) {
                var addSqlParams = [msgArray[i].sId, msgArray[i].rId, msgArray[i].msgType, msgArray[i].msg, moment(new Date()).format('YYYY-MM-DD HH:mm:ss'), msgArray[i].slogo];
                sql.insert(addSql, addSqlParams).then(res => {
                    if (res) {
                        msgArray = []
                    }
                })
            }
        }
    })
    //查找聊天记录
    socket.on("msghistory", function (data) {
        let vm = {}
        var sqlite = 'SELECT * FROM gossip_chatHistory Where sId=' + '"' + data.userId + '"' + ' and rId=' + '"' + data.friendId + '"' + ' or sId=' + '"' + data.friendId + '"' + ' and rId=' + '"' + data.userId + '"';
        sql.query(sqlite).then(res => {
            if (res.length) {
                vm.isok = true
                vm.lst = res
                for (let i = 0, len = res.length; i < len; i++) {
                    res[i].time = moment(res[i].time).format('YYYY-MM-DD HH:mm:ss')
                }
                socket.emit('msghistory', vm);
            } else {
                vm.isok = false
                vm.lst = []
                socket.emit('msghistory', vm);
            }
        })
    })
    //查找朋友或添加好友 state-1表示无该账号 1还未成为好友 2已是好友
    socket.on("search", function (data) {
        var vm = {}
        var lst = []
        var sqlite = 'SELECT * FROM gossip_login Where account LIKE ' + '"%' + data.search + '%"';
        sql.query(sqlite).then(res => {
            if (res.length) {
                var sqlite2 = 'SELECT * FROM gossip_relation Where userId=' + '"' + data.userId + '"';
                sql.query(sqlite2).then(lres => {
                    console.log(lres)
                    for (let i = 0, len = res.length; i < len; i++) {
                        let temp = lres.find(f => f.friendId == res[i].userId)
                        if (temp) {
                            res[i].state == 2
                            res[i].msg = '已是好友'
                        } else {
                            res[i].state = 1
                            res[i].msg = '未成好友'
                        }

                        if (res[i].userId != null && res[i].userId != data.userId) {


                            let itemSearch = {
                                userId: res[i].userId,
                                name: res[i].name,
                                logo: res[i].logo,
                                state: res[i].state,
                                msg: res[i].msg
                            }

                            lst.push(itemSearch)

                        }
                    }
                    vm.isok = true
                    vm.lst = lst
                    socket.emit('search', vm);
                })
            } else {
                vm.isok = false;
                vm.state = -1;
                vm.msg = "暂无该账号";
                socket.emit('search', vm);
            }
        })
    })
    //添加好友通知
    socket.on("addFriend", function (data) {
        var addSql = 'INSERT INTO gossip_friend(userId,userName,userLogo,friendId,friendName,friendLogo,isType,isFriend,detail) VALUES(?,?,?,?,?,?,?,?,?)',
            addPara = [data.userInfo.userId, data.userInfo.name, data.userInfo.logo, data.friendInfo.userId, data.friendInfo.name, data.friendInfo.logo, 1, 0, data.detail],
            addPara2 = [data.friendInfo.userId, data.friendInfo.name, data.friendInfo.logo, data.userInfo.userId, data.userInfo.name, data.userInfo.logo, 0, 0, data.detail]
        sql.insert(addSql, addPara).then(res => {
            if (res) {
                let vm = {
                    Id: data.friendInfo.userId,
                    userInfo: data.userInfo,
                    detail: data.detail
                }
                socket.broadcast.emit('addFriend', vm);
            }
        })
        sql.insert(addSql, addPara2)

    })


});