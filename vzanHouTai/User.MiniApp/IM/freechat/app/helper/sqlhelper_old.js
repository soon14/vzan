/** 
 * @desc mysql数据库数据查询帮助模块
 * @author 夜
 * @version 2018/01/28 
 */

var mysql = require('mysql');
var config = require('../config/dbconfig');
var extend = require('extend');
var connection = mysql.createConnection({
    host: config.dataconfig.host,
    user: config.dataconfig.user,
    password: config.dataconfig.password,
    database: config.dataconfig.database
})
//查询参数
var paramData = {
    tableName: '',
    sqlwhere: '',
    orderBy: '',
    pageIndex: 1,
    pageSize: 10000,
}
//更新参数
var updateData = {
    tableName: '',
    data: null,
    id: 0,
}
//插入参数
var insertData = {
    tableName: '',
    data: null,
}
//返回对象
var def_result = {
    isok: false,
    data: [],
    msg: ''
}
handleDisconnect(connection);

/**
 * @description 查询单个实体
 * @param {object} @reqData 查询参数 ：{tableName: 表名,sqlwhere: 查询条件}
 * @param {function} @callback  回调函数 
 */
exports.getModel = function (reqData, callback) {
    //console.log(connection)
    paramData = extend(true, paramData, reqData);
    //开启连接
    connection = mysql.createConnection(connection.config);
    connection.connect();
    //handleDisconnect(connection);
    //拼接sql
    var sql = "select * from " + paramData.tableName;
    //  console.log("sql:"+sql);
    if (paramData.sqlwhere) {
        sql += " where " + paramData.sqlwhere;
    }
    sql += " limit 0,1";
    //console.log(sql);
    //查询结果
    connection.query(sql, function (error, results, fields) {
        callback(getModelData(error, results, fields));
    });
    //关闭连接   
    connection.end();
}


/**
 * @description getModel 数据回调
 * @param {object} @error 查询异常
 * @param {object} @results 返回数据
 * @param {object} @fields  数据列属性
 */
function getModelData(error, results, fields) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' 查询失败 - ' + error.sqlMessage;
        console.log(error);
        return result;
    }
    result.isok = true;
    if (results == null || results.length == 0) {
        result.data = null;
    } else {
        result.data = results[0];
    }
    //console.log("result");
    console.log(result.data.Id);
    return result;
}


/**
 * @description 列表查询 
 * @param {object} @reqData 查询参数{tableName: 表名, sqlwhere: 查询条件, orderBy: 排序, pageIndex: 起始页, pageSize: 行数,} （tableName必填项)
 * @param {function} @callback  回调函数
 */
exports.getList = function (reqData, callback) {
    paramData = extend(true, paramData, reqData);
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();

    //拼接sql
    var sql = '';
    var index = (paramData.pageIndex - 1) * paramData.pageSize;
    sql = "select * from " + paramData.tableName;
    if (paramData.sqlwhere) {
        sql += " where " + paramData.sqlwhere;
    }
    if (paramData.orderBy) {
        sql += " order by " + paramData.orderBy;
    }
    sql += " limit " + index + " , " + paramData.pageSize;

    //查询结果
    connection.query(sql, function (error, results, fields) {
        callback(getListData(error, results, fields));
    });
    //关闭连接   
    connection.end();
}


/**
 * @description getList 数据回调
 * @param {object} @error 查询异常
 * @param {object} @results 返回数据
 * @param {object} @fields  数据列属性
 */
function getListData(error, results, fields) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' 查询失败 - ';
       // console.log(error);
        return result;
    }
    result.isok = true;
    result.data = results;
    return result;
}


/**
 * @description 查询数量
 * @param {object} @reqData  查询参数 {tableName: 表名, sqlwhere: 查询条件}
 * @param {function} @callback  回调函数
 */
exports.getCount = function (reqData, callback) {
    paramData = extend(true, paramData, reqData);
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();
    sql = "select count(*) count from " + paramData.tableName;
    if (paramData.sqlwhere) {
        sql += " where " + paramData.sqlwhere;
    }
    //查询结果
    connection.query(sql, function (error, results, fields) {
        callback(getCountData(error, results, fields));
    });
    //关闭连接   
    connection.end();
}

/**
 * @description getCount 数据回调
 * @param {object} @error 查询异常
 * @param {object} @results 返回数据
 * @param {object} @fields  数据列属性
 */
function getCountData(error, results, fields) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' 查询失败 - ' + error.sqlMessage;
        return result;
    }
    result.isok = true;
    result.data = results[0];
    return result;
}

/**
 * @description 更新实体
 * @param {object} @reqData 查询参数{tableName: 表名, data:更新的实体数据}
 * @param {function} @callback 数据回调
 */
exports.updateModel = function (reqData, callback) {
    updateData = extend(true, updateData, reqData);
    var result=extend(true,{},def_result)
    if (updateData.data === null || updateData.id === 0) {
        result.msg = "更新失败- 参数错误";
        callback(result);
        return;
    }

    var sql = "update " + updateData.tableName + " set ";
    var arr = new Array();
    var vals = new Array();
    for (var val in updateData.data) {
        arr.push(val + " = ?");
        vals.push(updateData.data[val]);
    }
    sql += arr.join(',');
    sql += " where id=?";
    vals.push(updateData.id);
    // console.log(sql);
    // console.log(vals);
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();
    connection.query(sql, vals, function (error, results) {
        callback(getUpdateResult(error, results));
    });
    //关闭连接
    connection.end();
}

/**
 * @description 返回更新结果回调
 * @param {object} @error 查询异常
 * @param {object} @result 返回结果
 */
function getUpdateResult(error, results) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' 更新失败 - ' + error.sqlMessage;
        return result;
    }
    result.isok = true;
    result.data = results.affectedRows;
    return result;
}

/**
 * @description 插入数据 实体第一个属性固定放自增列,设值为0
 * @param {object} @reqData {tableName:表名,data:实体数据}
 * @param {function} @callback 回调函数
 */
exports.insertModel = function (reqData, callback) {
    insertData = extend(true, insertData, reqData);
    var result=extend(true,{},def_result)
    if (insertData.data === null) {
        result.msg = "插入失败- 参数错误";
        callback(result);
        return;
    }
    var sql = "insert into " + insertData.tableName;
    var arr = new Array();
    var vals = new Array();
    var params = new Array();
    for (var val in insertData.data) {
        arr.push(val);
        vals.push(insertData.data[val]);
        params.push('?');
    }

    sql += "(" + arr.join(',') + ") values (" + params.join(',') + ")";
    console.log(sql);
    console.log(vals);
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();
    connection.query(sql, vals, function (error, results) {
        callback(getInsertResult(error, results));
    });
    //关闭连接
    connection.end();

}
/**
 * @description 返回插入结果回调
 * @param {object} @error 查询异常
 * @param {object} @result 返回 插入数据的id
 */
function getInsertResult(error, results) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' 插入失败 - ' + error.sqlMessage;
        return result;
    }
    result.isok = true;
    result.data = result.insertId;
    return result;
}

/**
 * @description 自定义sql语句
 * @param {string} @sql sql语句
 * @param {function} @callback 回调函数
 */
exports.bulidSql = function (sql, callback) {
    var result=extend(true,{},def_result)
    if (sql === '') {
        result.msg = "请输入sql语句";
        callback(result);
        return;
    }
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();
    //查询结果
    connection.query(sql, function (error, results, fields) {
        callback(selectData(error, results, fields));
    });
    //关闭连接   
    connection.end();
}
/**
 * @description bulidSql数据回调
 * @param {object} @error 查询异常
 * @param {object} @result 返回结果
 */
function selectData(error, results, fields) {
    var result=extend(true,{},def_result)
    if (error) {
        result.msg = ' sql执行失败 - ' + error.sqlMessage;
        return result;
    }
    result.isok = true;
    result.data = results;
    return result;
}

/**
* @description 带参自定义sql
* @param {string} @sql sql语句
* @param {Array} @params 数据参数数组
* @param {function} @callback 回调函数
*/
exports.bulidSqlwithParams = function (sql, params, callback) {
    var result=extend(true,{},def_result)
    if (sql === '') {
        result.msg = "请输入sql语句";
        callback(result);
        return;
    }
    connection = mysql.createConnection(connection.config);
    //开启连接
    connection.connect();
    //查询结果
    connection.query(sql, params, function (error, results, fields) {
        callback(selectData(error, results, fields));
    });
    //关闭连接   
    connection.end();
}

function handleDisconnect(connection) {
    if (!connection) {
        connection = mysql.createConnection({
            host: config.dataconfig.host,
            user: config.dataconfig.user,
            password: config.dataconfig.password,
            database: config.dataconfig.database
        })
    }
    connection.on('error', function (err) {
        if (!err.fatal) {
            return;
        }

        if (err.code !== 'PROTOCOL_CONNECTION_LOST') {
            throw err;
        }

        console.log('Re-connecting lost connection: ' + err.stack);

        connection = mysql.createConnection({
            host: config.dataconfig.host,
            user: config.dataconfig.user,
            password: config.dataconfig.password,
            database: config.dataconfig.database
        })
        handleDisconnect(connection);
        connection.connect();
    });
}