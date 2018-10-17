var redishelper = require("../helper/redishelper");
var constData = require("../model/const");
var sd = require('silly-datetime');
var uuid = require('node-uuid');

exports.chat = function (msgdata, connections, conn) {
    if(msgdata.tuserId>0) {
        msgdata.ids = uuid.v4();
        msgdata.fnickName=conn.nickName;
        msgdata.fheadImg=conn.headImg;
        msgdata.sendDate=sd.format(new Date(), 'YYYY-MM-DD HH:mm:ss');
        conn.sendText(JSON.stringify(msgdata));
        connections.forEach(function (v, i) {
            if (v.userId == msgdata.tuserId && v.fuserType == msgdata.tuserType) {
                v.sendText(JSON.stringify(msgdata));
            }
        })
        conn.message.tuserType = msgdata.tuserType;
        conn.message.fuserId=msgdata.fuserId
        conn.message.ids = msgdata.ids;
        conn.message.msg = msgdata.msg;
        conn.message.tuserId = msgdata.tuserId;
        conn.message.msgType = msgdata.msgType;
        conn.message.sendDate= msgdata.sendDate;
        var keyname = `${constData.message_keyname}_${msgdata.fuserId}_${msgdata.tuserId}_${msgdata.tuserType}`;
        redishelper.rpushRedis(keyname, conn.message)
            .then(function (data) {
                if(data.isok){
                    redishelper.lrange('messagekeys').then(function(returnData){
                        if(returnData.isok){
                            var haskey=false;
                            returnData.msg.forEach(function(v,i){
                                if(v===keyname){
                                    haskey=true;
                                }
                            })
                            if(!haskey){
                                redishelper.rpushRedis('messagekeys',keyname)
                            }
                        }
                    })
                }
            })
            .otherwise(function (res) {
                console.log(res);
            })
    }
}

var chatclose = function (connections, conn) {
    var userNickname = '';
    connections.forEach(function (v, i) {
        if (v == conn) {
            connections.splice(i, 1);
        }
    });
}

exports.chatclose = chatclose;