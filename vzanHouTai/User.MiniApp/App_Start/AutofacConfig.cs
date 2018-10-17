﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Autofac;
using Autofac.Integration.Mvc;
using Autofac.Core;

namespace User.MiniApp.App_Start
{
    public class AutofacConfig
    {
        public static Container _container;
        /// <summary>
        /// 负责调用autofac框架实现业务逻辑层和数据仓储层程序集中的类型对象的创建
        /// 负责创建MVC控制器类的对象(调用控制器中的有参构造函数),接管DefaultControllerFactory的工作
        /// </summary>
        public static void Register()
        {
            ////实例化一个autofac的创建容器
            //var builder = new ContainerBuilder();

            //#region 批量注册
            ////var baseType = typeof(BLL.MiniApp.IDependency);
            ////var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList();
            ////var allBLL = assembly
            ////    .SelectMany(s => s.GetTypes())
            ////    .Where(p => baseType.IsAssignableFrom(p) && p != baseType);


            ////builder.RegisterAssemblyTypes(assembly.ToArray())
            ////    .Where(t=>baseType.IsAssignableFrom(t)&&t!=baseType)
            ////    .AsImplementedInterfaces().InstancePerLifetimeScope();

            ////告诉Autofac框架，将来要创建的控制器类存放在哪个程序集 (Wchl.CRM.WebUI)
            //Assembly controllerAss = Assembly.Load("User.MiniApp");
            //builder.RegisterControllers(controllerAss);

            //////告诉autofac框架注册数据仓储层所在程序集中的所有类的对象实例
            ////Assembly respAss = Assembly.Load("Wchl.WMBlog.Repository");
            //////创建respAss中的所有类的instance以此类的实现接口存储
            ////builder.RegisterTypes(respAss.GetTypes()).AsImplementedInterfaces();

            ////告诉autofac框架注册业务逻辑层所在程序集中的所有类的对象实例
            //Assembly bllAss = Assembly.Load("BLL.MiniApp");
            ////创建serAss中的所有类的instance以此类的实现接口存储
            //builder.RegisterTypes(bllAss.GetTypes()).AsImplementedInterfaces();

            ////创建一个Autofac的容器
            //var container = builder.Build();
            ////将MVC的控制器对象实例 交由autofac来创建
            //DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
            //#endregion

            //实例化一个autofac的创建容器
            ContainerBuilder containerBuilder = new ContainerBuilder();
            Assembly bllAss = Assembly.Load("BLL.MiniApp");
            containerBuilder.RegisterTypes(bllAss.GetTypes());
            Assembly entAss = Assembly.Load("Entity.MiniApp");
            containerBuilder.RegisterTypes(entAss.GetTypes());
            //创建一个Autofac的容器
            _container = (Container)containerBuilder.Build();
            //将MVC的控制器对象实例 交由autofac来创建
            DependencyResolver.SetResolver(new AutofacDependencyResolver(_container));
        }
    }
}