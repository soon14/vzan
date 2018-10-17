var sql = require('./sql');


// 登陆验证
exports.login = function (req, res) {
    let vm = {}
    let sqlite = 'SELECT * FROM login Where name=' + '"' + req.body.name + '"' + ' and password=' + '"' + req.body.password + '"';
    sql.query(sqlite).then(data => {

        if (data.length) {
            vm.isok = true
            vm.msg = '查找到数据'
            vm.user = data[0]
            req.session.user = vm.user;
        } else {
            vm.isok = false;
            vm.msg = '您输入的帐号或者密码不正确，请重新输入。';
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}
// 登陆信息
exports.userInfo = function (req, res) {
    let vm = req.session.user
    res.end(JSON.stringify(vm));
}
// 查询内容列表
exports.queryContent = function (req, res) {
    let vm = {}
    let typeid = Number(req.body.typeid);
    let [sqlite, sqlength] = ['', '']
    let [pageindex, pagesize] = [req.body.pageindex, req.body.pagesize]
    if (typeid) {
        sqlength = 'SELECT COUNT(*) From product where c_id=' + typeid
        sqlite = 'SELECT * From product where c_id=' + typeid + ' Limit ' + (pageindex - 1) * pagesize + ',' + pagesize;
    } else {
        sqlength = 'SELECT COUNT(*) From product'
        sqlite = 'SELECT * From product Limit ' + (pageindex - 1) * pagesize + ',' + pagesize;
    }
    sql.query(sqlite).then(data => {
        sql.query(sqlength).then(num => {
            if (data.length) {
                vm.isok = true
                vm.lst = data
                vm.msg = '查询成功'
                vm.num = num[0]["COUNT(*)"]
            } else {
                vm.isok = false
                vm.lst = []
                vm.msg = '暂无数据'
                vm.num = 0
            }
            res.writeHead(200, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify(vm));
        })
    })
}
//添加内容
exports.addContent = function (req, res) {
    let vm = {}
    let reb = req.body.data
    reb.c_id = Number(reb.c_id)
    var addSql = 'INSERT INTO product(title,descript,image,content,c_id,type_name) VALUES(?,?,?,?,?,?)';
    var addSqlParams = [reb.title, reb.desc, reb.img, reb.content, reb.c_id, reb.type_name];
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '添加成功'
        } else {
            vm.isok = false
            vm.msg = '添加失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    });
}
//更新内容
exports.updateContent = function (req, res) {
    let vm = {}
    let reb = req.body.data
    reb.id = Number(reb.id)
    reb.c_id = Number(reb.c_id)
    var addSql = 'UPDATE product SET title=?,descript=?,image=?,content=?,c_id=?,type_name=? WHERE Id=?';
    var addSqlParams = [reb.title, reb.desc, reb.img, reb.content, reb.c_id, reb.type_name, reb.id];
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '更新成功'
        } else {
            vm.isok = false
            vm.msg = '更新失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    });
}
// 删除内容
exports.deleteContent = function (req, res) {
    let vm = {}
    let id = req.body.id
    var delSql = 'DELETE FROM product where id=' + id;
    sql.query(delSql).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '删除成功'
        } else {
            vm.isok = false
            vm.msg = '删除失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}
// 查询分类
exports.queryClassify = function (req, res) {
    var sqlite = 'SELECT * From classify'
    sql.query(sqlite).then(data => {
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(data));
    })
}
// 添加分类
exports.addClassify = function (req, res) {
    let vm = {}
    let reb = req.body.value
    var addSql = 'INSERT INTO classify(type_name) VALUES(?)';
    var addSqlParams = [reb];
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '添加成功'
        } else {
            vm.isok = false
            vm.msg = '添加失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    });
}
// 更新分类
exports.updateClassify = function (req, res) {
    let vm = {}
    let reb = req.body
    let updateSql = 'UPDATE classify SET type_name=? WHERE Id=?';
    let updateSqlParams = [reb.typeName, reb.id];
    sql.insert(updateSql, updateSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '更新成功'
        } else {
            vm.isok = false
            vm.msg = '更新失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}
//删除分类
exports.deleteClassify = function (req, res) {
    let vm = {}
    let id = req.body.id
    let sqlite = 'SELECT COUNT(*) FROM product WHERE c_id=' + id
    sql.query(sqlite).then(data => {
        let num = data[0]["COUNT(*)"]
        if (num > 0) {
            vm.isok = false
            vm.msg = '该分类下还有内容，请先删除'
            res.writeHead(200, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify(vm));
        } else {
            let delSql = 'DELETE FROM classify where id=' + id;
            sql.query(delSql).then(data => {
                if (data) {
                    vm.isok = true
                    vm.msg = '删除成功'
                } else {
                    vm.isok = false
                    vm.msg = '删除失败'
                }
                res.writeHead(200, {
                    'Content-Type': 'application/json'
                });
                res.end(JSON.stringify(vm));
            })
        }
    })
}
// 查询所有用户
exports.queryUser = function (req, res) {
    let vm = {}
    let [pageindex, pagesize] = [req.body.pageindex, req.body.pagesize]
    let sqlite = 'SELECT * From login Limit ' + (pageindex - 1) * pagesize + ',' + pagesize;
    let sqlength = 'SELECT COUNT(*) From login'
    sql.query(sqlite).then(data => {
        sql.query(sqlength).then(num => {
            let length = num[0]["COUNT(*)"]
            vm.count = length
            vm.lst = data
            res.writeHead(200, {
                'Content-Type': 'application/json'
            });
            res.end(JSON.stringify(vm));
        })
    })
}
//增加用户
exports.addUser = function (req, res) {
    let vm = {}
    let reb = req.body.data
    let addSql = 'INSERT INTO login(name,password,username,userlogo) VALUES(?,?,?,?)';
    let addSqlParams = [reb.acount, reb.password, reb.name, reb.logo]
    sql.insert(addSql, addSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '添加成功'
        } else {
            vm.isok = false
            vm.msg = '添加失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}
// 更新用户
exports.updateUser = function (req, res) {
    let vm = {}
    let reb = req.body.vm
    reb.id = Number(reb.id)
    let updateSql = 'UPDATE login SET name=?,password=?,username=?,userlogo=? WHERE Id=?';
    let updateSqlParams = [reb.acount, reb.password, reb.name, reb.logo, reb.id];
    sql.insert(updateSql, updateSqlParams).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '更新成功';
            if (req.session.user.id == reb.id) {
                vm.state = -1
                vm.user = {
                    name: reb.acount,
                    password: reb.password,
                    username: reb.name,
                    userlogo: reb.logo,
                    id: reb.id
                }
                req.session.user = vm.user
                vm.orther = req.session.user
            } else {
                vm.state = 0
            }
        } else {
            vm.isok = false
            vm.msg = '更新失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}
//删除用户
exports.deleteUser = function (req, res) {
    let vm = {}
    let id = req.body.id
    let delSql = 'DELETE FROM login WHERE id=' + id;
    sql.query(delSql).then(data => {
        if (data) {
            vm.isok = true
            vm.msg = '删除成功'
        } else {
            vm.isok = false
            vm.msg = '删除失败'
        }
        res.writeHead(200, {
            'Content-Type': 'application/json'
        });
        res.end(JSON.stringify(vm));
    })
}