var redishelper = require("../helper/redishelper");
var constData = require("../model/const");
var sd = require('silly-datetime');
var uuid = require('node-uuid');

exports.chat = function (msgdata, connections, conn) {
    msgdata.ids = uuid.v4();

    if ("NickName" in conn.userInfo) {
        msgdata.fnickName = conn.userInfo.NickName;
    } else {
        msgdata.fnickName = '';
    }

    msgdata.fheadImg = conn.userInfo.HeadImgUrl;
    msgdata.sendDate = sd.format(new Date(), 'YYYY-MM-DD HH:mm:ss');
    conn.sendText(JSON.stringify(msgdata));
    var isonline = false;
    connections.forEach(function (v, i) {
       
        if (v.userId == msgdata.tuserId) {
            isonline = true;
            v.sendText(JSON.stringify(msgdata));
            if (v.tuserId != msgdata.fuserId) {
                SetUnReadCount(msgdata.fuserId, v.userId);//设置未读消息数    
            }
        }
        
    })
    if (!isonline) {
       
         SetUnReadCount(msgdata.fuserId, msgdata.tuserId);//设置未读消息数
     }
    var keyname = `${constData.message_keyname}_${msgdata.fuserId}_${msgdata.tuserId}_0`;
    redishelper.rpushRedis(keyname, msgdata)
        .then(function (data) {
            if (data.isok) {
                redishelper.lrange('messagekeys').then(function (returnData) {
                    if (returnData.isok) {
                        var haskey = false;
                        returnData.msg.forEach(function (v, i) {
                            if (v === keyname) {
                                haskey = true;
                            }
                        })
                        if (!haskey) {
                            redishelper.rpushRedis('messagekeys', keyname)
                        }
                    }
                })
            }
        })
        .otherwise(function (res) {
            console.log(res);
        })
}

var chatclose = function (connections, conn) {
    var userNickname = '';
    connections.forEach(function (v, i) {
        if (v.userId == conn.userId) {
            connections.splice(i, 1);
        }
    });
}

exports.chatclose = chatclose;

function SetUnReadCount(fuserId, tuserId) {
    var keyname = `messageCount_${tuserId}`;
    redishelper.incrby(keyname, 1);
    keyname = `messageCount_${tuserId}_${fuserId}`;
    redishelper.incrby(keyname, 1);
}



var RemoveReadCount = function (fuserId, tuserId) {
    var keyname = `messageCount_${fuserId}_${tuserId}`;
    redishelper.getRedis(keyname).then(function (returnData) {//删除该用户针对某个用户的未读消息数
        if (returnData.isok && returnData.msg != null) {
            var msgCount = parseInt(returnData.msg);
            redishelper.delRedis(keyname).then(function (data) {
                if (data.isok) {
                    keyname = `messageCount_${fuserId}`;
                    redishelper.decrby(keyname, msgCount);
                }
            })
        }
    })
}
exports.RemoveReadCount = RemoveReadCount;