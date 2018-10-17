
; (function () {
    $(function () {
        //滚动条在Y轴上的滚动距离
        function getScrollTop() {
            var scrollTop = 0,
                bodyScrollTop = 0,
                documentScrollTop = 0;
            if (document.body) {
                bodyScrollTop = document.body.scrollTop;
            }
            if (document.documentElement) {
                documentScrollTop = document.documentElement.scrollTop;
            }
            scrollTop = (bodyScrollTop - documentScrollTop > 0) ? bodyScrollTop : documentScrollTop;
            return scrollTop;
        }
        //文档的总高度
        function getScrollHeight() {
            var scrollHeight = 0,
                bodyScrollHeight = 0,
                documentScrollHeight = 0;
            if (document.body) {
                bodyScrollHeight = document.body.scrollHeight;
            }
            if (document.documentElement) {
                documentScrollHeight = document.documentElement.scrollHeight;
            }
            scrollHeight = (bodyScrollHeight - documentScrollHeight > 0) ? bodyScrollHeight : documentScrollHeight;
            return scrollHeight;
        }
        //浏览器视口的高度
        function getWindowHeight() {
            var windowHeight = 0;
            if (document.compatMode == "CSS1Compat") {
                windowHeight = document.documentElement.clientHeight;
            } else {
                windowHeight = document.body.clientHeight;
            }
            return windowHeight;
        }

        var loadprecodeimg = false;
        var vm = new Vue({
            el: "#MyWorkbench",
            data: {
                cases: [],
                tags: [],
                caseCount: 0,
                casePostData: {
                    pageIndex: 1,
                    pageSize: 8,
                    tagId: 0,
                    id: 0,
                    loadmore: false
                },
                mytemplates: [],
                recordCount: 0,
                userlist: [],
                Xcxlist: [],
                selectTid: [],
                activeClass: 'fp-font-blue line-color03',
                selectIndex: 0,
                postdata: {
                    pageindex: 0,
                    pagesize: 12,
                },
                currenttemplate: {
                    tname: '',
                    username: '',
                    price: 0,
                },
                customer: null,
                isloading: false,
            },
            watch: {
                mytemplates: {
                    handler: function (nums) {
                        nums.forEach(function (o, i) {
                            switch (o.Type) {
                                case 1: o.iosimg = "s-gw.png"; break; //官网
                                case 4: o.iosimg = "s-dy.png"; break; //单页版
                                case 6: o.iosimg = "s-ds.png"; break; //电商版
                                case 8: o.iosimg = "s-cy.png"; break; //餐饮版
                                case 12: o.iosimg = "s-jc.png"; break; //行业版
                                case 22: o.iosimg = "s-gj.png"; break; //专业版
                                case 26: o.iosimg = "s-dmd.png"; break; //专业版
                                default: o.iosimg = "s-dmd.png"; break; //专业版
                            }
                        })

                    },
                    deep: true
                }
            },
            methods: {
                //产品案例切换（切换模板）
                GetDataByTid: function (index) {
                    this.casePostData.id = this.Xcxlist[index].Id;
                    this.casePostData.tagId = 0;
                    this.casePostData.pageIndex = 1;
                    this.GetCaseList();
                },
                //产品案例切换（切换标签）
                GetDataByTagId: function (index) {
                    if (index >= 0) {
                        this.casePostData.tagId = this.tags[index].id;
                    } else {
                        this.casePostData.tagId = 0;
                    }
                    this.casePostData.pageIndex = 1;
                    this.GetCaseList();
                },
                //产品案例切换(加载更多)
                LoadMoreData: function () {
                    this.casePostData.pageIndex++;
                    this.casePostData.loadmore = true;
                    this.GetCaseList();
                },
                GetCaseList: function () {
                    var layerIndex = layer.load(1);
                    $.post("/dzhome/GetCaseList", { id: this.casePostData.id, tagId: this.casePostData.tagId, pageIndex: this.casePostData.pageIndex, pageSize: this.casePostData.pageSize }, function (data) {
                        layer.close(layerIndex);

                        if (data.isok) {
                            vm.recordCount = data.recordCount;
                            if (vm.casePostData.loadmore) {
                                vm.casePostData.loadmore = false;
                                vm.cases = vm.cases.concat(data.cases);
                                vm.tags = data.tags;
                            } else {
                                vm.cases = data.cases;
                                vm.tags = data.tags;
                            }
                        } else {
                            layer.msg(data.msg);
                        }
                    }, "json")
                },
                scrollevent: function () {
                    alert("adsfsdf");
                },
                //获取用户模板数据
                getuserlist: function () {
                    if (this.isloading) {
                        layer.msg("正在加载");
                        return;
                    }
                    this.isloading = true;
                    this.postdata.pageindex++;
                    $.post("/dzhome/GetUserTemplatesList", this.postdata, function (data) {
                        vm.isloading = false;
                        if (!data.isok) {
                            return;
                        } else {
                            vm.mytemplates = vm.mytemplates.concat(data.obj);
                            if (vm.mytemplates == null || vm.mytemplates.length <= 0) {
                                $("#nodata").show();
                            }
                            else {
                                $("#nodata").hide();
                            }
                        }
                    })
                },
                //获取市场模板数据
                getXcxlist: function () {
                    $.post("/dzhome/GetTemplatesList", function (data) {
                        vm.isloading = false;
                        if (!data.isok) {
                            return;
                        } else {
                            vm.Xcxlist = data.dataObj;
                            vm.GetDataByTid(0);
                        }
                    })

                },
                //试用模板
                TestTemplate: function (index) {
                    var tid = vm.Xcxlist[index].Id;
                    var formdata = { tid: tid };
                    $.post("/dzhome/TestTemplate", formdata, function (data) {
                        layer.msg(data.Msg);
                        if (data.isok) {
                            setTimeout(function () {
                                window.location.reload();
                            }, 1000)
                        }
                    })
                },
                //上下架模型
                UpperOrLowerModel: function (index) {
                    var aid = vm.mytemplates[index].Id;
                    var state = vm.mytemplates[index].modelstate;
                    if (state == null || state == 0)
                    {
                        state = 1;
                        var name = vm.mytemplates[index].modelname;
                        var desc = vm.mytemplates[index].modeldesc;
                        var modelimgurl = vm.mytemplates[index].modelimgurl;
                        
                        $("#editeaid").val(aid);
                        layer.open({
                            type: 1,
                            zIndex: 999999,
                            title: "上架",
                            shade: 0.3,
                            area: ['350px', '480px'], //宽高
                            content: $("#modelInfo").html().replace("tempmodelname", "modelname").replace("tempmodeldesc", "modeldesc").replace("tempmodelimgurl", "modelimgurl").replace("tempmodelupload", "modelupload"),
                            btn: ["确定", "取消"],
                            end: function () {
                                currenteditid = 0;
                            },
                            yes: function () {
                                if (vm.logdindindex>0) {
                                    return;
                                }
                                var modelname = $("#modelname").val();
                                if (modelname == undefined || modelname.length <= 0) {
                                    return layer.msg("请输入名称");
                                }
                                var modeldesc = $("#modeldesc").val();
                                if (modeldesc == undefined || modeldesc.length <= 0) {
                                    return layer.msg("请输入备注");
                                }
                                
                                var uploadImgs = $("#uploadImgs").val();
                                if (uploadImgs==undefined || uploadImgs.length <= 0)
                                {
                                    return layer.msg("请上传封面");
                                }

                                vm.logdindindex = layer.load(1);
                                $.post("/dzhome/SaveModelState", { aid: aid, state: state, name: modelname, desc: modeldesc, imgurl: uploadImgs }, function (data) {
                                    if (data.isok) {
                                        layer.msg(data.Msg, { anim: 0, time: 1000 }, function () {
                                            window.parent.location.reload();
                                        });
                                    }
                                    else {
                                        layer.msg(data.Msg);
                                        layer.close(vm.logdindindex);
                                        vm.logdindindex = 0;
                                    }
                                })
                            },
                            success: function (layero, index) {
                                $("#modelname").val(name)
                                $("#modeldesc").val(desc)
                                if (modelimgurl != null && modelimgurl != undefined && modelimgurl.length > 0)
                                {
                                    $("#uploadImgs").val(modelimgurl)
                                    $("#modelimgurl").attr("src", modelimgurl);
                                    $("#modelimgurl").show();
                                    $("#modelupload").hide();
                                }
                            }
                        })
                    }
                    else {
                        state = 0;
                        layer.confirm('你确定要下架该装修模型？', {
                            btn: ['确定', '取消'] //按钮
                        }, function () {
                            if (vm.logdindindex > 0) {
                                return;
                            }
                            vm.logdindindex = layer.load(1);
                            $.post("/dzhome/SaveModelState", { aid: aid, state: state}, function (data) {
                                    if (data.isok) {
                                        layer.msg(data.Msg, { anim: 0, time: 1000 }, function () {
                                            layer.load(1);
                                            window.parent.location.reload();
                                        });
                                    }
                                    else {
                                        layer.msg(data.Msg);
                                        layer.close(vm.logdindindex);
                                        vm.logdindindex = 0;
                                    }
                                });
                        }, function () {
                        });
                    }
                },
                //获取体验二维码
                GetYuLangQRCode: function (url) {
                    if (loadprecodeimg) {
                        return;
                    }
                    loadprecodeimg = true;
                    $.post(url, function (result) {
                        loadprecodeimg = false;
                        if (result.isok > 0) {
                            layer.open({
                                title: '微信扫一扫预览效果',
                                type: 1,
                                offset: '100px',
                                content: '<img style="width: 100%;" src="' + "data:image/jpeg;base64," + result.imgdata + '" />',
                            });
                        }
                        else {
                            layer.msg(result.msg);
                        }
                    });
                },
                selectClick: function (index) {
                    vm.selectIndex = index;
                    vm.GetDataByTid(index);
                },
                resetPage: function (/*issearch*/) {
                    layui.use('laypage', function () {
                        var laypage = layui.laypage;
                        laypage.render({
                            elem: 'pages'
                            , count: vm.recordCount //数据总数，从服务端得到
                            , curr: vm.postdata.pageIndex //当前页
                            , limit: vm.postdata.pagesize
                            , jump: function (obj, first) {

                                console.log(obj.limit); //得到每页显示的条数
                                vm.postdata.pageIndex = obj.curr;

                                //首次执行
                                if (!first) {
                                    vm.getuserlist();
                                }
                            }
                            , theme: '#1E9FFF'
                            , layout: ['prev', 'page', 'next', 'skip']
                        });
                    })
                },
                Search: function () {
                    this.pageIndex = 1;
                    this.postdata.starttime = $("#begintime").val();
                    this.postdata.endtime = $("#endtime").val();
                    this.getuserlist();
                },
                buyClick: function () {
                    show("gotobuy", 0);
                    popUp("gotobuy");
                },
                orderNow: function () {
                    //$("#btnSubmit").click(function () {
                    submitInformation();
                    // })
                },
                
                resetSearch: function () {
                    this.postdata.loginname = '';
                    this.postdata.username = '';
                    this.postdata.state = 0;
                    this.postdata.starttime = '';
                    this.postdata.endtime = '';
                    this.postdata.tids = '';
                    this.selectTid = [];
                },
                changePwd: function (index) {
                    var id = this.userlist[index].id;
                    layer.open({
                        type: 2,
                        title: "修改密码",
                        //shade: 0,
                        shade: [0.8, '#000'],
                        // skin: 'layui-layer-rim', //加上边框
                        area: ['650px', '350px'], //宽高
                        content: '/MiappAgentManager/updatePwd?id=' + id
                    });
                },
            },
            created: function (index) {
                $("#MyWorkbench").show();
                this.getXcxlist();
                this.getuserlist();
            },
            mounted:function()
            {
                window.onscroll = function () {
                    if (getScrollTop() + getWindowHeight() >= getScrollHeight() - 20) {
                        if (!vm.isloading) {
                            vm.getuserlist();
                        }
                    }
                }
            }
        })
    })

    
})()

function UploadImg(type, isSpec, index) {
    var aid = $("#editeaid").val();
    var that = this;
    var framSrc;
    var maxCount = 1;
    var remainCount = 1;
    framSrc = "/tools/UpLoadImgFrm?Id=" + aid + "&appId=" + aid + "&multi_selection=0&maxImgSize=1&objKey=modelimgurl&objType=1&frontMethod=1&remainCount=" + remainCount;
    $("#uploadFrame").attr("src", framSrc);
    $("#addModal_UploadImg").modal('show');
}
function clearImg(type, index) {
    var that = this;
    if (type > 0) {
        //表示轮播图
        if (that.bannerImg.length > 0) {
            that.bannerImg.splice(index, 1);
        }

        that.p.slideimgs = that.bannerImg.join(",");
    } else {
        that.p.img = "";
    }

}