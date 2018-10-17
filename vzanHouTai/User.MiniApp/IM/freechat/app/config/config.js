fs = require("fs");
/**服务器配置 */
var config = {
    /**数据请求host */
    host:'http://testwtapi.vzan.com',
    /**端口号*/
    port: 9527,//足浴
    proPort:9528,//专业版
    /**连接超时时间*/
    connTimeoutLimit: 1000 * 5,
    /**超时连接检查间隔时间*/
    checkTimeoutInterval: 1000 * 3,
    /**websocket 连接配置 */
    wss_options: {
        secure: true,//是否使用wss协议
      //  key: fs.readFileSync('../cafile/dzwss.key'),//加密证书密钥
       // cert: fs.readFileSync('../cafile/dzwss.pem'), //加密证书私钥    
        passphrase: '214522721910725'//如果秘钥文件有密码的话，用这个属性设置密码
    },
    redisServerPort: '6379',
    redisServerIP: 'r-2zef341cae637764302.redis.rds.aliyuncs.com',
    reidsPassWord:'voGLJ6g1ciWs6aq9'
}
module.exports = config;