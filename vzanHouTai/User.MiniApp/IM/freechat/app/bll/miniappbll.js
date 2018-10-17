var sqlhelper = require("../helper/sqlhelper");
var immessage=require("../model/immessage");
var when = require('when');
class miniappbll {
    GetXcxRelationByAppid(userInfo) {
        var resData={userInfo:userInfo,message:immessage.model}
        var deferred = when.defer();
        var reqdata = { tableName: 'xcxappaccountrelation', sqlwhere: "appid='" + userInfo.appId + "'" }
        sqlhelper.getModel(reqdata).then(function (xcxRelation) {
            resData.message.appid=xcxRelation.AppId;
            resData.message.aid=xcxRelation.Id;
            deferred.resolve(resData);
        }).otherwise(function (data) {
            deferred.reject(data);
        });
        return deferred.promise;
    }
}
var bll = new miniappbll();
exports.bll = bll;