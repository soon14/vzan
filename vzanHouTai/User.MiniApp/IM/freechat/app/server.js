/**专业版私信 */
var config = require("./config/config"),
    sqlhelper = require("./helper/sqlhelper"),
    miniappbll = require("./bll/miniappbll"),
    immessageModle = require("./model/immessage"),
    ws = require("nodejs-websocket"),
    url = require("url"),
    queryString = require("querystring"),
    extend = require("extend"),
    when = require("when"),
    prochatbll = require("./bll/prochatbll"),
    chatbll = require("./bll/chatbll"),
    sd = require('silly-datetime'),
    httphelper = require("./helper/httphelper"),
    schedule = require("node-schedule");

var connections = [];

try {
    var server = ws.createServer(function (conn) {
        var _url = url.parse(conn.path);
        var _querystring = queryString.parse(_url.query);
        var appId, userId, fuserType, aid, isFirst = false;

        if ("userId" in _querystring) {
            userId = _querystring.userId;
        }
        if ("appId" in _querystring) {
            appId = _querystring.appId;
        }
        if ("aid" in _querystring) {
            aid = _querystring.aid;
        }

        if ("fuserType" in _querystring) {//足浴的逻辑
            //console.log(appId);
            fuserType = _querystring.fuserType;
            setfbUserInfo(conn, userId, appId, fuserType);

            //消息接收
            conn.on("text", function (str) {
                try {
                    var data = JSON.parse(str);
                    if (data.command === 'online') {
                        GetOnline(conn);
                    } else {
                        chatbll.chat(data, connections, conn);
                    }
                } catch (ex) {
                    console.log(ex);
                }

            })
            //关闭连接
            conn.on("close", function (code, reason) {
                chatbll.chatclose(connections, conn);
            })
            //连接异常
            conn.on("error", function (err) {
                console.log(err);
            })
        } else {//专业版的
            if ("isFirst" in _querystring) {
                isFirst = _querystring.isFirst;
            }
            conn.onlineDate = new Date();
            conn.isFirst = isFirst;
            conn.userId = userId;
            conn.appId = appId;
            conn.aid = aid;
            // conn.fuserType = fuserType;
            setUserInfo(conn);

            //消息接收
            conn.on("text", function (str) {
                try {
                    var data = JSON.parse(str);
                    if (data.msgType == 2) {//系统消息(msgType:0 文字消息，1 图片消息 2系统消息)
                        switch (data.isChat) {
                            case 0://离开聊天界面
                                conn.tuserId = 0;
                                break;
                            case 1://进入聊天界面
                                conn.tuserId = data.tuserId;
                                prochatbll.RemoveReadCount(conn.userId, conn.tuserId);
                                break;
                            case 2://更新在线时间
                                conn.onlineDate = new Date();
				// console.log( "更新在线时间 "+conn.userId+":"+conn.onlineDate);
                                break;
                        }
                    } else {
                        prochatbll.chat(data, connections, conn);
                        httphelper.post(config.host + "/apiim/HaveSixin", { tuserId: data.tuserId });
                    }
                } catch (ex) {
                    console.log(ex);
                }

            })
            //关闭连接
            conn.on("close", function (code, reason) {
                prochatbll.chatclose(connections, conn);
            })
            //连接异常
            conn.on("error", function (err) {
                //                console.log(err);
                //console.log("连接异常");
            })
        }
    }).listen(9528);

} catch (ex) {
    console.log(ex.message);
}



function setUserInfo(conn) {
    //console.log(config.host+"/apiim/GetUserInfo");
    //console.log({ appId: conn.appId, userId: conn.userId });
    httphelper.post(config.host + "/apiim/GetUserInfo", { appId: conn.appId, userId: conn.userId }).then(function (data) {
        var obj = JSON.parse(data);
        if (obj.isok) {
            conn.userInfo = obj.dataObj
            var isOnline = false;
            connections.forEach(function (v, i) {
                if (v.userId == conn.userId) {
                    isOnline = true;
                }
            })
            if (!isOnline) {
                connections.push(conn);
            }
        } else {
            var msgdata = { msgType: 4, msg: obj.Msg };
            conn.sendText(JSON.stringify(msgdata));
            conn.close();
        }
    })
}
//足浴设置用户信息
function setfbUserInfo(conn, userId, appId, fuserType) {
    //技师
    if (fuserType == 1) {
        var reqdata = { tableName: 'xcxappaccountrelation', sqlwhere: "appid='" + appId + "'" }
        sqlhelper.getModel(reqdata).then(function (xcxRelation) {
            if (!xcxRelation) {
                var bugtime = sd.format(new Date(), 'YYYY-MM-DD HH:mm:ss');
                console.log(bugtime + "小程序不存在 appid:" + appId + " ");
                conn.close();
                return;
            }
            reqdata.tableName = 'technicianinfo';
            reqdata.sqlwhere = "id=" + userId;
            sqlhelper.getModel(reqdata).then(function (technicianinfo) {
                conn.message = immessageModle.model;
                conn.message.appId = xcxRelation.AppId;
                conn.message.aId = xcxRelation.Id;
                conn.fuserType = fuserType;
                conn.headImg = technicianinfo.headimg;
                conn.nickName = technicianinfo.jobNumber;
                conn.userId = userId;
                connections.push(conn);
            })
        })
    }
    else {//普通用户
        tableName = 'c_userInfo';
        sqlwhere = " id=" + userId + " and appid='" + appId + "'";
        sqlhelper.getModel({ tableName: tableName, sqlwhere: sqlwhere })
            .then(miniappbll.bll.GetXcxRelationByAppid)
            .then(function (data) {
                conn.message = data.message;
                conn.fuserType = fuserType;
                conn.userId = userId;
                conn.headImg = data.userInfo.HeadImgUrl;
                conn.nickName = data.userInfo.NickName;
                connections.push(conn);
            }).otherwise(function (data) {
                console.log(data);
                conn.close();
                return;
            });
    }
}
function GetOnline(conn) {
    var data = { connetions: connections.length }
    conn.sendText(JSON.stringify(data));
}
//每隔2分钟判断在线时间是否超过三分钟没有更新，是的话断开链接
function checkOnlineDate() {
    var rule1 = new schedule.RecurrenceRule();
    var times1 = [0,2,4,6,8,10,12,14,16,18,20,22,24,26,28,30,32,34,36,38,40,42,44,46,48,50,52,54,56,58];
    rule1.minute = times1;
   // rule1.second = times1;
    schedule.scheduleJob(rule1, function () {
        //console.log("before:"+connections.length);
        connections.forEach(function (v, i) {
            var date =new Date(v.onlineDate);
            date.setMinutes(date.getMinutes()+3);
            //console.log( v.userId+":"+v.onlineDate);
            if(date<new Date()){
                v.close();
            }
        });
       // console.log("after:"+connections.length);
    });
}


checkOnlineDate();


console.log("勿关！小程序私信服务器运行中 端口：9530");
module.exports = server;