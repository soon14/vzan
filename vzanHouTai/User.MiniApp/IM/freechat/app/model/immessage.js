var sd = require('silly-datetime');
var time = sd.format(new Date(), 'YYYY-MM-DD HH:mm:ss');
//console.log(time);
/**
 * @description 消息记录类
 */
class immessage {
    constructor(Id = 0, appId = '', aId = 0, storeId = 0, roomId = 0, fuserId = 0, tuserId = 0, msgType = 0, state = 0, msg = '', tuserType=0,ids='',sendDate = time) {
        this.id = Id;//自增id
        this.appId = appId; //小程序appid
        this.aId = aId; //小程序aid
        this.storeId = storeId; //门店id
        this.roomId = roomId; //房间id（暂时未用到）
        this.fuserId = fuserId;//发送者id (群发送，即系统消息为0)
        this.tuserId = tuserId;//接受者id (群接收为0)
        this.msgType = msgType;//消息类型 (-1:系统消息离开，0:文本，1:图像, 2:语音, 3:短视频，4:系统消息进入)
        this.state = state; //状态 默为0 (0:正常  -1:删除)
        this.msg = msg; //消息内容
        this.sendDate = sendDate; //发送时间
        this.tuserType=tuserType;//用户类型
        this.ids=ids;//唯一值 uuid
    }
    setModel(appId = '', aId = 0, storeId = 0, roomId = 0, fuserId = 0){
        this.appId = appId; 
        this.aId = aId; 
        this.storeId = storeId; 
        this.roomId = roomId;  
        this.fuserId = fuserId;
        return this;
    }
}
exports.model = new immessage();
