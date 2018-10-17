//启动服务进程
let Service = require('node-windows').Service;

     let svc = new Service({
        name: 'pinservice',    //服务名称
        description: '拼享惠超时订单取消', //描述
        script: 'D:/work_xiaochengxu/xiaochengxu/trunk/User.MiniApp/IM/freechat/app/pinservice.js' //nodejs项目要启动的文件路径
    });

    svc.on('install', () => {
        svc.start();
    });

    svc.install();

