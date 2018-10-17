var mysql = require('mysql')
var connection = mysql.createConnection({
    host: '127.0.0.1',
    user: 'root',
    password: '12345678',
    database: 'kaaden',
    port: 3306,
});

// 插入数据
function insert(addsql, paras) {
    connection.connect();
    connection.query(addsql, paras, function (err, result) {
        if (err) {
            console.log('[INSERT ERROR] - ', err.message);
            return;
        }

        console.log('--------------------------INSERT----------------------------');
        console.log('INSERT ID:', result);
        console.log('-----------------------------------------------------------------\n\n');
    });
    connection.end();
}

function query(sql) {
    return new Promise(function (resolve, reject) {

        connection.query(sql, function (err, result) {
            let data={}
            if (err) {
                data = err.message
            } else {
                data =result
            }
            resolve(data)
        });
    })
    connection.end();
}
module.exports = {
    insert,
    query
}