var when = require("when");
var config = require("../config/config");
var extend = require('extend');
var redis = require('redis'),
        RDS_PORT = config.redisServerPort,                //端口号    
        RDS_HOST = config.redisServerIP,    //服务器IP  要连接的A服务器redis    
        RDS_PWD = config.reidsPassWord,     //密码    
        RDS_OPTS = {};                 //设置项    

var obj = {
        key: '',
        value: ''
};
var returnData = {
        isok: false,
        msg: ''
}
function createClient() {
        var client = redis.createClient(RDS_PORT, RDS_HOST, RDS_OPTS);
        client.auth(RDS_PWD, function () {
                //console.log('通过认证');
        });
        client.on('ready', function (err) {
                //console.log('ready');
        });
        return client;
}
/**
 * @description 加入redis
 * @param {obj} @setData {key:keyname,value:data}
 * @param {function} @callback {isok:true|false,msg:errorMsg}
 */
exports.setRedis = function (setData) {
        var deferred = when.defer();
        obj = extend(true, obj, setData);
        if (obj.key.length === 0 || obj.value.length === 0) {
                returnData.isok = false;
                returnData.msg = '缓存数据为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.set(obj.key, obj.value, function (err, replay) {

                        if (replay == 'OK') {
                                returnData.isok = true;
                                deferred.resolve(returnData)
                        } else {
                                returnData.isok = false;
                                returnData.msg = "设置失败";
                                deferred.reject(returnData)
                        }
                });
        });
        return deferred.promise;
}

/**
 * @description 获取redis数据
 * @param {string} @keyname 键值
 * @param {function} @callback {isok:true|false}
 */
exports.getRedis = function (keyname) {
        var deferred = when.defer();
        if (keyname.length == 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.get(keyname, function (err, replay) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = replay;
                                deferred.resolve(returnData)
                        }

                });
        })
        return deferred.promise;
}
/**
 * @description 设置散列redis
 * @param {string} hashkey 
 * @param {string} keyname 
 * @param {object} value 
 */
exports.hmsetRedis = function (hashkey, keyname, value) {
        var client = createClient();
        var deferred = when.defer();
        if (keyname.length === 0 || hashkey.length === 0) {
                console.log("redis键值不能为空");
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        client.on('connect', function () {
                client.hmset(hashkey, keyname, value, function (err, replay) {
                        if (replay == 'OK') {
                                returnData.isok = true;
                                deferred.resolve(returnData)
                        } else {
                                returnData.isok = false;
                                returnData.msg = "设置失败";
                                deferred.reject(returnData);
                        }

                });
        })
        return deferred.promise;
}
/**
 * @description 获取指定散列所有redis缓存
 * @param {string} hashkey 
 */
exports.hgetallRedis = function (hashkey) {
        var client = redis.createClient(RDS_PORT, RDS_HOST, RDS_OPTS);
        client.auth(RDS_PWD, function () {
                //console.log('通过认证');
        });
        client.on('ready', function (err) {
                // console.log('ready');
        });
        var deferred = when.defer();
        if (hashkey.lenght === 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.hgetall(hashkey, function (err, res) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = res;
                                deferred.resolve(returnData)
                        }
                })
        })
        return deferred.promise;
}
/**
 * @description 获取散列指定键值
 * @param {string} hashkey 
 * @param {string} keyname 
 */
exports.hmgetRedis = function (hashkey, keyname) {
        var deferred = when.defer();
        if (hashkey.lenght === 0 || keyname.length === 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.hmget(hashkey, keyname, function (err, res) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = res;
                                deferred.resolve(returnData)
                        }
                })
        })
        return deferred.promise;
}
exports.delRedis = function (hashkey) {
        var deferred = when.defer();
        if (hashkey.lenght === 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.del(hashkey, function (err, res) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = res;
                                deferred.resolve(returnData)
                        }
                })
        })
        return deferred.promise;
}
exports.rpushRedis = function (keyname, objData) {
        var datass = JSON.stringify(objData);
        var deferred = when.defer();
        if (keyname.lenght === 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.rpush(keyname, datass, function (err, res) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = res;
                                deferred.resolve(returnData)
                        }
                })
        })
        return deferred.promise;
}
exports.lrange = function (keyname) {
        var deferred = when.defer();
        if (keyname.lenght === 0) {
                returnData.isok = false;
                returnData.msg = 'redis键值不能为空';
                deferred.reject(returnData)
        }
        var client = createClient();
        client.on('connect', function () {
                client.lrange(keyname, 0, -1, function (err, res) {
                        if (err) {
                                returnData.isok = false;
                                returnData.msg = err;
                                deferred.reject(returnData)
                        } else {
                                returnData.isok = true;
                                returnData.msg = res;
                                deferred.resolve(returnData)
                        }
                })
        })
        return deferred.promise;
}