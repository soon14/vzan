//卸载服务进程
let Service = require('node-windows').Service;

    let svc = new Service({
        name: 'imserver',    //服务名称
        description: '私信socket', //描述
        script: 'D:/work_xiaochengxu/xiaochengxu/trunk/User.MiniApp/IM/freechat/app/addmessage.js' //nodejs项目要启动的文件路径
    });

  svc.on('uninstall',function(){
      console.log('Uninstall complete.');
      console.log('The service exists: ',svc.exists);
    });

  svc.uninstall();