using BLL.MiniApp.Conf;
using BLL.MiniApp.Dish;
using BLL.MiniApp.Ent;
using BLL.MiniApp.Fds;
using BLL.MiniApp.Stores;
using Entity.MiniApp;
using Entity.MiniApp.Conf;
using Entity.MiniApp.Dish;
using Entity.MiniApp.Ent;
using Entity.MiniApp.Fds;
using Entity.MiniApp.Stores;
using Entity.MiniApp.User;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utility;

namespace BLL.MiniApp.Helper
{
    public static class PrinterHelper
    {
        #region 专业版打印内容拼接
        /// <summary>
        /// 专业版普通订单打印内容拼接
        /// </summary>
        /// <param name="food"></param>
        /// <param name="foodGoodsOrder"></param>
        /// <param name="cars"></param>
        /// <param name="foodPrintList"></param>
        /// <returns></returns>
        public static string entPrintOrderContent(EntGoodsOrder goodsOrder)
        {
            

            //打印内容
            string content = "";

            List<EntGoodsCart> cars = EntGoodsCartBLL.SingleModel.GetList($" GoodsOrderId = {goodsOrder.Id} ") ?? new List<EntGoodsCart>();

            string goodsIds = string.Join(",",cars.Select(s=>s.FoodGoodsId).Distinct());
            List<EntGoods> entGoodsList = EntGoodsBLL.SingleModel.GetListByIds(goodsIds);

            cars.ForEach(c =>
            {
                c.goodsMsg = entGoodsList?.FirstOrDefault(f=>f.id == c.FoodGoodsId) ?? new EntGoods();
            });

            XcxAppAccountRelation curXcx = XcxAppAccountRelationBLL.SingleModel.GetModel(goodsOrder.aId);
            if (curXcx == null)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), "专业版打印内容拼接失败 curXcx_null");
                return content;
            }
            //获取店铺名
            OpenAuthorizerConfig XUserList = OpenAuthorizerConfigBLL.SingleModel.GetModelByAppids(curXcx.AppId);
            if (XUserList == null)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), "专业版打印内容获取不到小程序名,但仍旧打印 XUserList_null");
            }

            string zqStoreInfo = "送货地址：无(到店自取)\r\n";
            if (!string.IsNullOrEmpty(goodsOrder.attribute))
            {
                goodsOrder.attrbuteModel = JsonConvert.DeserializeObject<EntGoodsOrderAttr>(goodsOrder.attribute);
            }
            PickPlace storeAddress = null;
            if (goodsOrder.attrbuteModel.zqStoreId > 0)
            {
                storeAddress = PickPlaceBLL.SingleModel.GetModel(goodsOrder.attrbuteModel.zqStoreId);
            }
            else if (!string.IsNullOrWhiteSpace(goodsOrder.attrbuteModel.zqStoreName))
            {
                storeAddress = PickPlaceBLL.SingleModel.GetModelByAid_Name(goodsOrder.aId, goodsOrder.attrbuteModel.zqStoreName);
            }
            if (storeAddress != null)
            {
                zqStoreInfo = $"送货地址：【{storeAddress?.name}】{storeAddress?.address}\r\n";
            }

            //拼接订单内容排版 
            content = $"<MC>0,00005,0</MC><FS>{XUserList?.nick_name ?? string.Empty}</FS>\r\n";
            content += "<table><tr><td>商品</td><td>数量</td><td>金额(元)</td></tr>";
            content += "<tr><td>@@2................................</td></tr>";
            foreach (var car in cars)
            {
                //car.goodsMsg = foodGoodsBLL.GetModel($"Id={car.FoodGoodsId}");
                content += $"<tr><td>{car.goodsMsg.name}</td><td>{car.Count}</td><td>{(car.Price * car.Count * 0.01).ToString("0.00")}</td></tr>";
                if (!string.IsNullOrEmpty(car.SpecInfo))
                {
                    content += $"<tr><td>规格:{car.SpecInfo}</td></tr>";
                }

                content += $"<tr><td></td></tr>";

            }
            content += "<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>";
            content += $"<tr><td>实收:</td><td>-</td><td>{(goodsOrder.BuyPrice * 0.01).ToString("0.00")}</td></tr>";
            content += $"<tr><td></td><td></td><td>({Enum.GetName(typeof(miniAppBuyMode), goodsOrder.BuyMode)})</td></tr></table>";
            content += "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n";
            content += $"订单号：{goodsOrder.OrderNum}\r\n";
            content += $"下单时间：{goodsOrder.CreateDate}\r\n";
            content += $"打印时间：{DateTime.Now}\r\n";
            content += $"收货人：{goodsOrder.AccepterName}\r\n";
            content += $"联系方式:{goodsOrder.AccepterTelePhone}\r\n";
            switch (goodsOrder.GetWay)
            {
                case (int)miniAppOrderGetWay.商家配送:
                    content += $"送货地址：{goodsOrder.Address}\r\n";

                    break;
                case (int)miniAppOrderGetWay.到店自取:
                case (int)miniAppOrderGetWay.到店消费:
                    content += zqStoreInfo;

                    break;
            }
            content += $"订单备注：{goodsOrder.Message}\r\n";

            return content;
        }


        #endregion

        #region 电商版打印内容拼接

        public static string storePrintOrderContent(StoreGoodsOrder goodsOrder)
        {
            if (goodsOrder == null || goodsOrder.Id <= 0)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), "电商拼接打印订单内容错误 goodsOrder_null/id_error");
            }

            
            

            List<StoreGoodsCart> goodCarts = StoreGoodsCartBLL.SingleModel.GetList($"GoodsOrderId={goodsOrder.Id} and state=1 ");
            if (goodCarts == null)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), "电商拼接打印订单内容错误 goodCarts_null");
            }
            string content = "<MC>0,00005,0</MC><table><tr><td>商品名称</td><td>数量</td><td>单价(元)</td></tr>";
            content += "<tr><td>@@2................................</td></tr>";

            string goodsIds = string.Join(",",goodCarts.Select(s=>s.GoodsId).Distinct());
            List<StoreGoods> storeGoodsList = StoreGoodsBLL.SingleModel.GetListByIds(goodsIds);

            foreach (Entity.MiniApp.Stores.StoreGoodsCart goodCart in goodCarts)
            {
                goodCart.goodsMsg = storeGoodsList?.FirstOrDefault(f=>f.Id == goodCart.GoodsId);
                if (goodCart.goodsMsg != null)
                {
                    content += $"<tr><td>{goodCart.goodsMsg.GoodsName}</td><td>{goodCart.Count}</td><td>{goodCart.Price * 0.01}</td></tr>";
                    if (!string.IsNullOrEmpty(goodCart.SpecInfo))
                    {
                        content += $"<tr><td>规格:{goodCart.SpecInfo.Replace(' ', '|')}</td></tr>";
                    }
                    else
                    {
                        content += $"<tr><td></td></tr>";
                    }
                }
            }
            content += "</table>";
            content += "┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n";
            content += $"<table><tr><td>总价:</td><td>-</td><td>{(goodsOrder.BuyPrice * 0.01).ToString("0.00")}</td></tr></table>";
            content += $"收货人：{goodsOrder.AccepterName}\r\n";
            content += $"联系方式:{goodsOrder.AccepterTelePhone}\r\n";
            content += $"送货地址:{goodsOrder.Address}\r\n";
            if (!string.IsNullOrEmpty(goodsOrder.Message))
            {
                content += $"买家留言:{goodsOrder.Message}";
            }
            return content;
        }
        #endregion

        /// <summary>
        /// 打印机打印
        /// </summary>
        /// <param name="prints">打印机集合</param>
        /// <param name="content">要打印的内容</param>
        /// <param name="orderId">订单的ID,目前无作用,仅记录方便以后跟进,没有可传0</param>
        public static void printContent(List<FoodPrints> prints, string content, int orderId, int tId = 0, Account account = null)
        {
            
            
            SystemUpdateMessage msg = null;


            if (!string.IsNullOrWhiteSpace(content) && prints != null && prints.Any())
            {
                
                prints.ForEach(print =>
                {
                    var returnMsg = FoodYiLianYunPrintHelper.printContent(print.APIKey, print.UserId, print.PrintNo, print.PrintKey, content);
                    var returnModel = SerializeHelper.DesFromJson<FoodYlyReturnModel>(returnMsg);
                    //记录订单打印日志
                    string remark = string.Empty;
                    if (returnModel.state == 2)
                    {
                        remark = "提交时间超时";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 3)
                    {
                        remark = "参数有误";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 4)
                    {
                        remark = "sign加密验证失败";
                        returnModel.state = -1;
                    }
                    else if (returnModel.state == 1)
                    {
                        remark = "发送成功";
                        returnModel.state = 0;
                    }

                    //打印失败通过系统消息 => 通知相应后台商家
                    if (returnModel.state != 1 && returnModel.state != 0)
                    {
                        if (account != null)
                        {
                            msg = new SystemUpdateMessage();
                            msg.AccountId = account.Id.ToString();
                            msg.Title = $"打印机[{print?.Name}]打单异常 ";
                            msg.PublishUser = "系统监测自动添入";
                            msg.State = 0;
                            msg.Type = 2;
                            msg.AddTime = DateTime.Now;
                            msg.UpdateTime = DateTime.Now;
                            msg.Year = DateTime.Now.Year;
                            msg.Month = DateTime.Now.Month;
                            msg.Day = DateTime.Now.Day;
                            msg.IsRead = 0;
                            msg.Type = 2;
                            msg.TId = tId;
                            msg.Content = $"与打印机官方通讯出现问题,恢复前无法打印任何单据,故障信息:{remark};";

                            SystemUpdateMessageBLL.SingleModel.Add(msg);
                        }
                    }
                    var log = new FoodOrderPrintLog()
                    {
                        Dataid = returnModel.id,
                        addtime = DateTime.Now,
                        machine_code = print.PrintNo,
                        state = returnModel.state,
                        isupdate = 0,
                        remark = remark,
                        orderId = orderId,
                        printsId = print.Id
                    };
                    FoodOrderPrintLogBLL.SingleModel.Add(log);
                });

            }
        }


        #region 智慧餐厅打印
        /// <summary>
        /// 根据参数配置打单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="print_type">打单类型 0:直接所有相关打印机打单 1:下单后打单 2.支付后打单</param>
        public static void DishPrintOrderByPrintType(DishOrder order, int print_type = 0, string printerIds = null)
        {
            if (order == null) return;
            DishStore store = DishStoreBLL.SingleModel.GetModel(order.storeId);
            if (store == null) return;
            List<DishShoppingCart> carts = DishShoppingCartBLL.SingleModel.GetCartsByOrderId(order.id);
            DishPrintOrderByPrintType(order, store, carts, print_type, printerIds);
        }

        /// <summary>
        /// 根据参数配置打单
        /// </summary>
        /// <param name="order"></param>
        /// <param name="print_type">打单类型 0:直接所有相关打印机打单 1:下单后打单 2.支付后打单</param>
        public static void DishPrintOrderByPrintType(DishOrder order, DishStore store, List<DishShoppingCart> carts, int print_type = 0, string printerIds = null)
        {
            Task.Factory.StartNew(() =>
            {
                List<DishPrint> prints = string.IsNullOrWhiteSpace(printerIds) ? DishPrintBLL.SingleModel.GetPrintsByParams(store.aid, store.id, print_type) : DishPrintBLL.SingleModel.GetByIds(printerIds);
                DishPrintOrder(store, order, carts, prints);
            });
        }

        /// <summary>
        /// 餐饮打印订单
        /// </summary>
        /// <param name="food"></param>
        /// <param name="order"></param>
        /// <param name="carts"></param>
        /// <param name="foodPrintList"></param>
        /// <param name="account">加传参数,打单失败会通过提示该用户,若不想提示,可传null</param>
        /// <returns></returns>
        public static void DishPrintOrder(DishStore store, DishOrder order, List<DishShoppingCart> carts, List<DishPrint> prints)
        {
            
            if (store == null || order == null || carts == null || !carts.Any() || prints == null || !prints.Any())
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), $"参数为空导致无法打印:dishStore:{store == null},order :{ order == null }, carts: {carts == null || !carts.Any()}, foodPrintList : {prints == null || !prints.Any()}");
                return;
            }

            prints.ForEach(print =>
            {
                if (print.print_d_type == 1) //整单打印
                {
                    string totalPrintContent = GetPrintContent_Total(store, order, carts, print); //整单打印内容
                    if (!string.IsNullOrWhiteSpace(totalPrintContent))//当此打印的设定打单匹配到有要打印的内容时,才去打印
                    {
                        FoodYiLianYunPrintHelper.printContent(print.apiPrivateKey, print.platform_userId.ToString(), print.print_bianma, print.print_shibiema, totalPrintContent);
                    }

                }
                else //分单打印
                {
                    List<string> singlePrintContentsByGoods = GetPrintContent_Part(store, order, carts, print); //分单(按菜品)打印内容
                    if (singlePrintContentsByGoods?.Count > 0)
                    {
                        foreach (string content in singlePrintContentsByGoods)
                        {
                            if (!string.IsNullOrWhiteSpace(content))//当此打印的设定打单匹配到有要打印的内容时,才去打印
                            {
                                FoodYiLianYunPrintHelper.printContent(print.apiPrivateKey, print.platform_userId.ToString(), print.print_bianma, print.print_shibiema, content);
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// 智慧餐厅打单内容 - 总单
        /// </summary>
        /// <returns></returns>
        public static string GetPrintContent_Total(DishStore store, DishOrder order, List<DishShoppingCart> carts, DishPrint print)
        {
            string printContent = string.Empty;
            List<DishGood> goods = DishGoodBLL.SingleModel.GetList($" id in ({string.Join(",", carts.Select(c => c.goods_id))}) ");
            if (goods == null)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), $"参数为空导致无法打印:goods:{goods == null}");
                return printContent;
            }

            List<DishShoppingCart> printShoppingCarts = carts;
            //得出此次要打印的购物车记录
            if (!string.IsNullOrWhiteSpace(print.print_tags))
            {
                int[] selTagsId = print.print_tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => Convert.ToInt32(t)).ToArray();
                printShoppingCarts = (from c in carts
                                      join g in goods on c.goods_id equals g.id
                                      where selTagsId.Contains(g.g_print_tag)
                                      select c)?.ToList();
            }
            if (printShoppingCarts == null || !printShoppingCarts.Any()) return printContent; //当前打印机无要打印的内容

            //处理商品字体
            Func<string, string> goodsFont = (info) =>
            {
                if (print.print_goods_ziti_type == 0)
                {
                    return $"{info}";
                }
                else
                {
                    return $"<FS2>{info}</FS2>";
                }
            };

            //处理其它资料字体
            Func<string, string> msgFont = (info) =>
            {
                if (print.print_ziti_type == 0)
                {
                    return $"{info}";
                }
                else
                {
                    return $"<FS>{info}</FS>";
                }
            };

            #region 订单打单内容拼接
            printContent = string.Empty;
            printContent += $"<MC>0,00005,0</MC><MN>{print.print_dnum}</MN>";
            printContent += $"<center>{print.print_top_copy}</center>\r\n";
            printContent += $"<FS2>{store.dish_name}</FS2>\r\n";
            printContent += $"<FS2>{order.order_haoma}号</FS2>\r\n";
            printContent += $"<FS2>{order.order_type_txt}订单{(!string.IsNullOrWhiteSpace(order.order_table_id) ? $":{order.order_table_id}" : string.Empty)}</FS2>\r\n";
            printContent += "<table><tr><td>@@2................................</td></tr>";
            //printContent += $"<tr><td>{msgFont("名称")}   </td><td>{msgFont("单价")}  </td><td>{msgFont("数量")} </td><td>{msgFont("金额")}</td></tr>";
            printContent += $"<tr><td>{msgFont("名称")}   </td><td>{msgFont("数量")}  </td><td>{msgFont("金额")}  </td></tr>";
            printContent += $"<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>";
            foreach (DishShoppingCart item in printShoppingCarts)
            {
                if (!string.IsNullOrEmpty(item.goods_attr))
                {
                    //printContent += $"<tr><td>{goodsFont($"{item.goods_name}({item.goods_attr})")}</td><td>{msgFont(item.goods_price.ToString())}  </td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                    printContent += $"<tr><td>{goodsFont($"{item.goods_name}({item.goods_attr})")}</td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                }
                else
                {
                    //printContent += $"<tr><td>{goodsFont(item.goods_name)}</td><td>{msgFont(item.goods_price.ToString())}  </td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                    printContent += $"<tr><td>{goodsFont(item.goods_name)}</td><td>x{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                }
            }

            #region 优惠内容
            string youhuiContent = string.Empty;
            if (order.shipping_fee > 0.00d)
            {
                youhuiContent += $"<tr><td>{msgFont("配送费")}</td><td></td><td></td><td>{msgFont($"￥{order.shipping_fee}")}</td></tr>";
            }
            if (order.dabao_fee > 0.00d)
            {
                youhuiContent += $"<tr><td>{msgFont("餐盒费")}</td><td></td><td></td><td>{msgFont($"￥{order.dabao_fee}")}</td></tr>";
            }
            if (order.huodong_quan_jiner > 0.00d)
            {
                youhuiContent += $"<tr><td>{msgFont("优惠券")}</td><td></td><td></td><td>{msgFont($"￥{order.huodong_quan_jiner}")}</td></tr>";
            }
            if (order.huodong_shou_jiner > 0.00d)
            {
                youhuiContent += $"<tr><td>{msgFont("首单减")}</td><td></td><td></td><td>{msgFont($"￥{order.huodong_shou_jiner}")}</td></tr>";
            }
            if (order.huodong_manjin_jiner > 0.00d)
            {
                youhuiContent += $"<tr><td>{msgFont("满减")}</td><td></td><td></td><td>{msgFont($"￥{order.huodong_manjin_jiner}")}</td></tr>";
            }

            //优惠文案不为空时才添加到打印内容
            if (!string.IsNullOrWhiteSpace(youhuiContent))
            {
                printContent += $"<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>" + youhuiContent;
            }
            #endregion

            printContent += $"</table>";
            printContent += $"┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n";

            string payText = order.pay_name;
            if ((int)DishEnums.PayMode.微信支付 == order.pay_id)
            {
                payText = order.pay_status == (int)DishEnums.PayState.未付款 ? "微信未支付" :
                          order.pay_status == (int)DishEnums.PayState.已付款 ? "微信已支付" : payText;
            }
            printContent += $"{msgFont($"合计:￥{order.settlement_total_fee} {(order.pay_id > 0 ? $"({payText})" : "")}")}\r\n";
            printContent += $"{msgFont($"订单号：{order.order_sn}")}\r\n";
            printContent += $"{msgFont($"下单时间：{order.add_time_txt}")}\r\n";
            printContent += $"{msgFont($"订单备注：{order.post_info}")}\r\n";

            if (order.order_type == (int)DishEnums.OrderType.外卖)//外卖额外信息
            {
                printContent += $"┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n";
                printContent += $"<FW2><FH2>配送信息</FH2></FW2>\r\n";
                printContent += $"{msgFont($"收货人:{order.consignee}")}\r\n";
                printContent += $"{msgFont($"收货地址:{order.address}")}\r\n";
                printContent += $"{msgFont($"联系电话:{order.mobile}")}\r\n";
            }
            if (order.is_fapiao == 1)//如果要发票
            {
                printContent += $"{msgFont($"发票:【{order.fapiao_leixing_txt}】【{order.fapiao_text}】【{order.fapiao_no}】")}\r\n";
            }
            printContent += $"<center>{print.print_bottom_copy}</center>\r\n";
            #endregion

            return printContent;
        }


        /// <summary>
        /// 智慧餐厅打单内容 - 分单
        /// </summary>
        /// <returns></returns>
        public static List<string> GetPrintContent_Part(DishStore store, DishOrder order, List<DishShoppingCart> carts, DishPrint print)
        {
            List<string> printContents = new List<string>();
            string printContent = string.Empty;
            List<DishGood> goods = DishGoodBLL.SingleModel.GetList($" id in ({string.Join(",", carts.Select(c => c.goods_id))}) ");
            if (goods == null)
            {
                log4net.LogHelper.WriteInfo(typeof(PrinterHelper), $"参数为空导致无法打印:goods:{goods == null}");
                return printContents;
            }

            //得出此次要打印的购物车记录
            List<DishShoppingCart> printShoppingCarts = carts;
            if (!string.IsNullOrWhiteSpace(print.print_tags))
            {
                int[] selTagsId = print.print_tags.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(t => Convert.ToInt32(t)).ToArray();
                printShoppingCarts = (from c in carts
                                      join g in goods on c.goods_id equals g.id
                                      where selTagsId.Contains(g.g_print_tag)
                                      select c)?.ToList();
            }
            if (printShoppingCarts == null || !printShoppingCarts.Any()) return printContents; //当前打印机无要打印的内容

            //处理商品字体
            Func<string, string> goodsFont = (info) =>
            {
                if (print.print_goods_ziti_type == 0)
                {
                    return $"{info}";
                }
                else
                {
                    return $"<FS2>{info}</FS2>";
                }
            };

            //处理其它资料字体
            Func<string, string> msgFont = (info) =>
            {
                if (print.print_ziti_type == 0)
                {
                    return $"{info}";
                }
                else
                {
                    return $"<FS>{info}</FS>";
                }
            };

            #region 分单打单
            foreach (DishShoppingCart item in printShoppingCarts)
            {
                printContent = string.Empty;
                printContent += $"<MC>0,00005,0</MC><MN>{print.print_dnum}</MN>";
                printContent += $"<FS2>{store.dish_name}</FS2>\r\n";
                printContent += $"<FS2>{order.order_haoma}号</FS2>\r\n";
                printContent += $"<FS2>{order.order_type_txt}订单{(!string.IsNullOrWhiteSpace(order.order_table_id) ? $":{order.order_table_id}" : string.Empty)}</FS2>\r\n";
                printContent += "<table><tr><td>@@2................................</td></tr>";
                //printContent += $"<tr><td>{msgFont("菜品")}       </td><td>{msgFont("单价")} </td><td>{msgFont("数量")} </td><td>{msgFont("金额")} </td></tr>";
                printContent += $"<tr><td>{msgFont("菜品")}       </td><td>{msgFont("数量")} </td><td>{msgFont("金额")} </td></tr>";
                printContent += $"<tr><td>┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄</td></tr>";
                if (!string.IsNullOrEmpty(item.goods_attr))
                {
                    //printContent += $"<tr><td>{goodsFont($"{item.goods_name}({item.goods_attr})")}</td><td>{msgFont(item.goods_price.ToString())}  </td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                    printContent += $"<tr><td>{goodsFont($"{item.goods_name}({item.goods_attr})")}</td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                }
                else
                {
                    //printContent += $"<tr><td>{goodsFont(item.goods_name)}</td><td>{msgFont(item.goods_price.ToString())}  </td><td>{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                    printContent += $"<tr><td>{goodsFont(item.goods_name)}</td><td>x{msgFont(item.goods_number.ToString())}  </td><td>{msgFont((item.goods_price * item.goods_number).ToString())}  </td></tr>";
                }
                printContent += $"</table>";
                printContent += $"┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄┄\r\n";
                printContent += $"{msgFont($"订单号：{order.order_sn}")}\r\n";
                printContent += $"{msgFont($"下单时间：{order.add_time_txt}")}\r\n";
                printContent += $"{msgFont($"订单备注：{order.post_info}")}\r\n";

                printContents.Add(printContent);//添加入分单内容集合
            }
            #endregion
            return printContents;
        }
        #endregion
    }
}