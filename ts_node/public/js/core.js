// const host = 'http://kaaden.orrzt.com';
const host="http://127.0.0.1:8082"
const addr = {
  login: host + '/login', //登陆
  userInfo: host + "/userInfo", //获取用户信息
  newsList: host + "/newsList", //获取新闻列表
  newsClassify: host + '/classify', //获取新闻分类
  upload: host + '/uploadFile', //图片上传
  addnews: host + '/addnews', //新增新闻
  delete: host + "/delete", //删除
  addType: host + '/addType', //分类添加
  updateNews: host + "/updateNews", //更新新闻
  updateType: host + "/updateType", //更新分类
  deleType: host + "/deleType", //删除分类
  userlist: host + "/userlist", //查询所有用户
  adduser: host + '/adduser', //添加用户
  updateUser: host + "/updateUser", //更新用户
  deletUser: host + "/deleUser", //删除用户
};
const http = {
  post: function (url, data) {
    return new Promise(function (resolve, reject) {
      $.ajax({
        url,
        data,
        type: 'POST',
        dataType: 'json',
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        success: function (data) {
          resolve(data)
        }
      });
    })
  },
  uploadImg: function (formData) {
    return new Promise(function (resolve, reject) {
      $.ajax({
        url: 'http://kaaden.orrzt.com/uploadFile',
        type: 'POST',
        data: formData,
        // 告诉jQuery不要去处理发送的数据
        processData: false,
        // 告诉jQuery不要去设置Content-Type请求头
        contentType: false,
        mimeType: "multipart/form-data",
        success: function (data) {
          resolve(JSON.parse(data))
        },
        error: function (data) {
          resolve(data)
        }
      });
    })
  },
  get: function (url, data) {
    return new Promise(function (resolve, reject) {
      $.ajax({
        url,
        data,
        type: 'GET',
        dataType: 'json',
        contentType: "application/x-www-form-urlencoded; charset=utf-8",
        success: function (data) {
          resolve(data)
        }
      });
    })
  }
};
const tools = {
  goNewPage(url) {
    window.location.href = url;
  },
  //成功提示
  showTip(msg, target) {
    target.$message({
      message: msg,
      type: 'success'
    });
  },
  showError(msg, target) {
    target.$message.error(msg);
  },


  fresh() {
    window.location.reload()
  }

    
};
/******************************请求封装***************************************/
const core = {
  // 新闻列表请求
  newslist(index, pageindex, target) {
    let _pageindex = pageindex || 1
    http.post(addr.newsList, {
      typeid: index || '',
      pageindex: _pageindex,
      pagesize: 9
    }).then(data => {
      for (let i = 0, len = data.lst.length; i < len; i++) {
        data.lst[i].time = data.lst[i].time.replace("T", " ");
        data.lst[i].time = data.lst[i].time.replace(".000Z", " ");

      }
      target.newsLst.list = data.lst
      target.newsLst.count = data.num || 1
      target.newsIndex = index
      http.post(addr.newsClassify).then(res => {
        target.classify = res
      })

    })
  },
  //添加新闻
  addNewsLst(_g, target) {
    let that = target
    let _index = that.newsLst.selValue
    if (_index == undefined) {
      _index = ''
    }
    http.post(addr.addnews, {
      data: _g
    }).then(data => {
      if (data.isok) {
        that.fullscreenLoading = true;
        that.news.title = ''
        that.news.desc = ''
        that.news.img = ''
        that.news.selType = ''
        that.news.content = ''
        core.newslist(_index, 1, target)
        setTimeout(() => {
          that.fullscreenLoading = false;
          tools.showTip(data.msg, that)
          that.selIndex = 1
        }, 2000);
      } else {
        tools.showError(data.msg, that)
      }
    })
  },
  //更新分类
  updateNewsLst(_g, target) {
    let that = target
    let _index = that.newsLst.selValue
    if (_index == undefined) {
      _index = ''
    }
    http.post(addr.updateNews, {
      data: _g
    }).then(data => {
      if (data.isok) {
        that.fullscreenLoading = true;
        that.news.title = ''
        that.news.desc = ''
        that.news.img = ''
        that.news.selType = ''
        that.news.content = ''
        that.update = false
        core.newslist(_index, 1, target)
        setTimeout(() => {
          that.fullscreenLoading = false;
          tools.showTip(data.msg, that)
          that.selIndex = 1
        }, 2000);
      } else {
        tools.showError(data.msg, that)
      }
    })
  },
  //删除内容
  deleteNews(id, target) {
    let _index = target.newsLst.selValue
    if (_index == undefined) {
      _index = ''
    }
    http.post(addr.delete, {
      id
    }).then(data => {
      if (data.isok) {
        tools.showTip(data.msg, target)
        core.newslist(_index, 1, target)
      } else {
        tools.showError(data.msg, target)
      }
    })
  },
  // 分类请求
  newsType(target) {
    http.post(addr.newsClassify).then(data => {
      target.classifyLst = data
    })
  },
  //分类添加
  addType(value, target) {
    let that = target
    http.post(addr.addType, {
      value
    }).then(data => {
      if (data.isok) {
        core.newsType(that)
        that.openFullScreen()
        setTimeout(() => {
          tools.showTip(data.msg, that)
          that.selIndex = 2
        }, 2000);
      } else {
        tools.showTip(data.msg, that)
      }
    })
  },
  //更新分类
  updateType(id, value, target) {
    let that = target
    http.post(addr.updateType, {
      id,
      typeName: value,
    }).then(data => {
      if (data.isok) {

        that.openFullScreen()
        core.newsType(that)
        setTimeout(() => {
          tools.showTip(data.msg, that)
          that.selIndex = 2
        }, 2000);
      } else {
        tools.showTip(data.msg, that)
      }
    })
  },
  //用户列表
  getUserLst(target) {
    let vm = target.userlst
    http.post(addr.userlist, {
      pageindex: vm.pageindex,
      pagesize: vm.pagesize,
    }).then(data => {
      target.userlst.list = data.lst
      target.userlst.count = data.count || 1
    })
  },
  //新增用户
  addUser(vm, target) {
    let that = target
    http.post(addr.adduser, {
      data: vm,
    }).then(data => {
      if (data.isok) {
        that.fullscreenLoading = true;
        that.userVm.acount = ''
        that.userVm.password = ''
        that.userVm.repeatpass = ''
        that.userVm.name = ''
        that.userVm.logo = ''
        that.userVm.updateUser = false
        core.getUserLst(that)
        setTimeout(() => {
          that.fullscreenLoading = false;
          tools.showTip(data.msg, that)
          that.selIndex = 3
        }, 2000);
      }
    })
  },
  //更新用户
  updateUser_re(vm, target) {
    let that = target
    http.post(addr.updateUser, {
      vm,
    }).then(data => {
      if (data.isok) {
        that.fullscreenLoading = true;
        that.userVm.acount = ''
        that.userVm.password = ''
        that.userVm.repeatpass = ''
        that.userVm.name = ''
        that.userVm.logo = ''
        that.userVm.updateUser = false
        if(data.state==-1){
          that.userInfo.userlogo=data.user.userlogo
           that.userInfo.username=data.user.username
        }
        core.getUserLst(that)
        setTimeout(() => {
          that.fullscreenLoading = false;

          tools.showTip(data.msg, that)
          that.selIndex = 3
        }, 2000);
      }
    })
  },
  //删除用户
  deleteUser(id,target) {
    http.post(addr.deletUser, {
      id
    }).then(data => {
      if (data.isok) {
        tools.showTip(data.msg, target)
        core.getUserLst(target)
      } else {
        tools.showError(data.msg, target)
      }
    })
  },
}