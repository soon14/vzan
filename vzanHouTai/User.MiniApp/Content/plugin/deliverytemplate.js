$(function () {
    Vue.component('delivery-app', delieryAPP);
});

var delieryAPP = {
    template: '#appTemplate',
    props: {
        set_append_para: String,
        set_sum_rule: Number,
        set_enable: Boolean,
        set_pagetype: Number,
        set_appid: Number,
        set_discount: Number,
        set_discount_enable: Boolean
    },
    data: function () {
        return {
            appId: 0,
            pageType: 0,//小程序类型
            pageIndex: 1,
            pageSize: 99, //默认加载全部运费模板
            enable: false, //启用运费模板
            discount: 0,//运费满级阈值
            discountEnable: false,//运费满级开关

            ///筛选模板tab
            tab: '0',
            templates: [],//模板列表
            templateTab: 'basic',//编辑模板tab
            editSection: '',
            isEditRegion: false,//编辑配送区域
            isEditTemplate: false,//编辑运费模板
            isEditProduct: false,//编辑商品运费模板
            isUpdating: false,//更新运费模板中
            isloading: false,//加载运费模板中
            isloadingRegion: false,//加载区域列表中
            isLoadingTemplate: false,//加载区域模板中
            isLoadingProduct: false,//加载商品数据中

            selectRule: 0,//选中计费规则

            editTemplateId: 0,//选中模板ID
            templateInput: {//编辑模板表单
                Name: '', Base: 1, BaseCost: 0, Extra: 0, ExtraCost: 0, UnitType: 0, DeliveryRegion: '', IsFree: false, EnableDiscount: false, Discount: 0,
            },
            unitType: '0',//模板单位类型

            region: [],//选中省份
            regionSub: [],//选中市区
            regionData: [],//省份数据
            regionSubData: [],//市区数据
            regionTab: 'province',//当前显示区域tab
            currRegions: [],//当前区域运费
            newRegions: [],//新增区域运费
            editRegionTemplate: {},

            products: [],
            productPageIndex: 1,
            productTotal: 0,
            selProductData: [],
            selProduct: [],
            selProductIndex: [],
            selProductTotal: 0,
            appendQueryPara: ''
        }
    },
    created: function () {
        this.selectRule = this.set_sum_rule;
        this.enable = this.set_enable;
        this.pageType = this.set_pagetype;
        this.appId = this.set_appid;
        this.discount = Number(this.set_discount || 0);
        this.discountEnable = !!this.set_discount_enable;

        this.getArea(this.bindProvice, '0');
        this.getTemplateList();
        this.getAvailableProduct(1);
    },
    computed: {
        //计价单位标题
        unitTitle: function () {
            unit = { '0': '件数', '1': 'kg', '2': '件数' };
            return unit[this.editingTemplate.UnitType];
        },
        //计价单位精度
        unitFormat: function () {
            return this.unitTitle === 'kg' ? 1000 : 1;
        },
        //操作模板窗口文案
        windowTitle: function () {
            if (!this.editTemplateId) {
                return '添加运费模板';
            } else {
                var editingTemplate = this.templates.find(template => template.Id === this.editTemplateId);
                return editingTemplate ? editingTemplate.Name : '加载中';
            }
        },
        //编辑中运费模板
        editingTemplate: {
            get: function () {
                var copy = JSON.parse(JSON.stringify(this.templateInput));
                copy.IsFree = copy.IsFree ? 1 : 0;
                copy.Discount = parseInt(this.safeCal(copy.Discount, 100));
                return copy;
            },
            set: function (template) {
                var thisApp = this;
                thisApp.currRegions = [];
                thisApp.newRegions = [];
                thisApp.editTemplateId = 0;
                thisApp.templateInput = {
                    Name: '', Base: 1, BaseCost: 0, Extra: 0, ExtraCost: 0, UnitType: 0, DeliveryRegion: '', IsFree: false, EnableDiscount: false, Discount: 0,
                };
                if (template) {
                    thisApp.editTemplateId = template.Id;
                    thisApp.templateInput.Discount = thisApp.safeCal(template.Discount, 0.01);
                    thisApp.templateInput.IsFree = !!template.IsFree;
                    ['Name', 'UnitType', 'EnableDiscount'].forEach(field => thisApp.templateInput[field] = template[field]);
                }
            }
        },
        //加载市区列表
        isLoadCity: function () {
            return this.regionTab === 'city';
        },
        //添加运费模板模式
        isAdd: function () {
            return !this.editTemplateId;
        },
        //编辑模板模式
        isEdit: function () {
            return !!this.editTemplateId;
        },
        //配送区域列表
        regionTemplate: function () {
            return this.currRegions.concat(this.newRegions);
        },
        //配送区域列表分组（一列3行）
        regionTemplateGroup: function () {
            var allRegion = this.regionTemplate;
            var groupCount = Math.ceil(allRegion.length / 3);
            var group = [];
            for (var i = 0; i < groupCount; i++) {
                var start = i * 3;
                var end = start + 3;
                group.push(allRegion.slice(start, end));
            }
            return group;
        },
        //选中省份/直辖市
        selectedRegion: {
            get: function () {
                var checkedRegion = Array.from(this.region);
                var checkedRegionFromSub = Array.from(new Set(this.regionSubData.filter(region => this.regionSub.indexOf(region.Code) > -1).map(region => region.ParentCode)));
                return Array.from(new Set(checkedRegion.concat(checkedRegionFromSub)));
            },
            set: function (checkRegion) {
                var removeRegion = this.selectedRegion.filter(code => checkRegion.indexOf(code) === -1);
                if (removeRegion.length) {
                    //批量移除市区
                    var removeRegionSub = this.regionSubData.filter(region => removeRegion.indexOf(region.ParentCode) > -1).map(region => region.Code);
                    this.regionSub = this.regionSub.filter(code => removeRegionSub.indexOf(code) === -1);
                }
                var addRegion = checkRegion.filter(code => this.selectedRegion.indexOf(code) === -1);
                if (addRegion.length) {
                    //批量添加市区
                    var addRegionSub = this.regionSubRemain.filter(region => addRegion.indexOf(region.ParentCode) > -1).map(region =>region.Code).filter(code => this.regionSub.indexOf(code) === -1);
                    this.regionSub = this.regionSub.concat(addRegionSub);
                }
                this.region = checkRegion;
            }
        },
        //获取排除区域
        selectedSubRegion: {
            get: function () {
                return Array.from(this.regionSub);
            },
            set: function (checkSubRegion) {
                this.region = Array.from(new Set(this.regionSubData.filter(region => checkSubRegion.indexOf(region.Code) > -1).map(region => region.ParentCode)));
                this.regionSub = checkSubRegion;
            }
        },
        //剩余可选省份
        regionRemain: function () {
            var thisApp = this;
            //编辑中省份
            var regionSubRemain = Array.from(new Set(thisApp.regionSubRemain.map(region => region.ParentCode)));
            //筛选剩余可选的省份 = 所有省份 - 已设置省份/已设置市区 + 编辑中省份
            var remainRegion = thisApp.regionData.filter(region => regionSubRemain.indexOf(region.Code) > -1);
            return remainRegion;
        },
        //剩余可选市区
        regionSubRemain: function () {
            var thisApp = this;
            //所有市区
            var allRegion = Array.from(thisApp.regionSubData);
            //已设置市区
            var selectedRegionSub = thisApp.getSubCode(thisApp.regionTemplate);
            //编辑中市区
            var editingRegionSub = thisApp.getSubCode([thisApp.editRegionTemplate]);
            //筛选剩余可选的市区 = 所有市区 - 已设置市区 + 编辑中市区
            var remainRegion = allRegion.filter(region => selectedRegionSub.indexOf(region.Code) === -1 || editingRegionSub.indexOf(region.Code) > -1);
            return remainRegion;
        },
        //新增/编辑区域运费设置
        newRegion: {
            get: function () {
                var thisApp = this;
                var parse = JSON.parse(JSON.stringify(thisApp.editRegionTemplate));
                //单位"kg"转换"g"
                parse.Base = parseInt(thisApp.safeCal(parse.Base, thisApp.unitFormat));
                parse.Extra = parseInt(thisApp.safeCal(parse.Extra, thisApp.unitFormat));
                //金额"元"转换"分"
                parse.BaseCost = parseInt(thisApp.safeCal(parse.BaseCost, 100));
                parse.ExtraCost = parseInt(thisApp.safeCal(parse.ExtraCost, 100));
                //省
                parse.DeliveryRegion = thisApp.selectedSubRegion.join(',');
                //包邮
                parse.IsFree = parse.IsFree ? 1 : 0;
                parse.UpdateTime = parse.localId ? '未保存' : parse.UpdateTime;
                return parse;
            },
            set: function (editRegion) {
                var thisApp = this;
                if (editRegion) {
                    //编辑
                    thisApp.region = thisApp.getParentCode([editRegion]);
                    thisApp.regionSub = thisApp.getSubCode([editRegion]);
                    var parse = JSON.parse(JSON.stringify(editRegion));
                    //单位"g"转换"kg"
                    parse.Base = thisApp.safeDiv(parse.Base, thisApp.unitFormat);
                    parse.Extra = thisApp.safeDiv(parse.Extra, thisApp.unitFormat);
                    //金额"分"转换"元"
                    parse.BaseCost = thisApp.safeCal(parse.BaseCost, 0.01);
                    parse.ExtraCost = thisApp.safeCal(parse.ExtraCost, 0.01);
                    //包邮
                    parse.IsFree = !!parse.IsFree;
                    thisApp.editRegionTemplate = parse;
                } else {
                    //新增
                    thisApp.region = [];
                    thisApp.regionSub = [];
                    thisApp.editRegionTemplate = {
                        Id: 0, localId: Date.now(), UpdateTime: '',
                        Base: 1, BaseCost: 0, Extra: 0, ExtraCost: 0, UnitType: 0, DeliveryRegion: '', IsFree: false,
                    };
                }
            }
        },
        //模板全国包邮
        isFree: function () {
            return this.editingTemplate.IsFree;
        },
        //区域包邮
        isRegionFree: function () {
            return this.editRegionTemplate.IsFree;
        },
        //商品列表 = 可选商品 + 已选商品
        allProduct: function () {
            return this.products ? this.products.concat(this.selProductData.filter(product => !this.products.find(thisProduct => thisProduct.Id === product.Id))) : [];
        },
        appendPara: function () {
            return this.set_append_para ? JSON.parse(this.set_append_para) : {};
        },
        //编辑运费基本设置中
        editBasicSection: function () {
            return this.editSection === 'Basic';
        },
        //编辑运费配送区域中
        editRegionSection: function () {
            return this.editSection === 'Region';
        }
    },
    watch: {
        templateTab: function (tab) {
            var thisApp = this;
            var currTemplate = null;
            if (tab === 'region' && thisApp.isEdit) {
                currTemplate = thisApp.templates.find(template => template.Id === thisApp.editTemplateId);
            }
            if (currTemplate) {
                this.editingTemplate.UnitType = currTemplate.UnitType;
            }
        },
        unitTitle: function (unit) {
            var thisApp = this;
            unitConvert = unit === 'kg' ? 1000 : 0.001;
            function convert(region) {
                region.Base = thisApp.safeCal(region.Base, unitConvert);
                region.Extra = thisApp.safeCal(region.Extra, unitConvert);
            };
            thisApp.newRegions.forEach(convert);
            thisApp.currRegions.forEach(convert);
        },
        productPageIndex: function (pageIndex) {
            this.getAvailableProduct(pageIndex);
        },
        selProductIndex: function (pageIndex) {
            this.getSelectedProduct(1, this.editTemplateId);
        },
    },
    methods: {
        //----------管理运费模板----------//
        //获取模板列表
        getTemplateList: function () {
            var thisApp = this;
            if (thisApp.isloading) {
                layer.msg("努力加载ing...")
                return;
            }
            thisApp.isloading = true;
            $.get("/tools/GetDeliveryTemplate", $.extend({
                appId: thisApp.appId,
                pageType: thisApp.pageType,
                pageIndex: thisApp.pageIndex,
                pageSize: thisApp.pageSize,
                unitType: thisApp.tab,
            }, thisApp.appendPara)).then(function (data) {
                thisApp.isloading = false;
                if (!data.isok) {
                    layer.msg(data.msg);
                    return;
                }
                thisApp.templates = data.dataObj;
            });
        },
        //新增模板
        addTempate: function () {
            this.editingTemplate = null;
            this.newRegion = null;
            this.isEditTemplate = true;
        },
        //编辑模板
        editTemplate: function (template, isEditRegion) {
            //显示编辑页面
            var thisApp = this;
            thisApp.editingTemplate = template;
            thisApp.isEditTemplate = true;
            thisApp.getDeliveryRegion();
        },
        //保存模板
        saveTemplate: function (showEditLayer) {
            var thisApp = this;
            if (thisApp.isUpdating) {
                layer.msg('请稍等,保存中....');
                return;
            }
            if (!thisApp.checkInput(thisApp.editingTemplate)) {
                return;
            }
            thisApp.isUpdating = true;
            //保存提交模板
            $.post('/tools/UpdateDeliveryTemplate', $.extend({
                appId: thisApp.appId,
                newTemplate: thisApp.editingTemplate,
                templateId: thisApp.editTemplateId,
                regionTemplate: thisApp.newRegions,
            }, thisApp.appendPara)).then(function (result) {
                if (!result.isok) {
                    layer.msg(result.Msg);
                    return;
                }
                layer.msg('保存成功');
                thisApp.isEditTemplate = showEditLayer;
                thisApp.tab = thisApp.editingTemplate.UnitType.toString();
                //清空区域更新队列
                thisApp.newRegions = [];
                //刷新列表
                thisApp.getTemplateList();
            }).always(function () {
                thisApp.isUpdating = false;
            });
        },
        //删除模板
        deleteTemplate: function (template, callBack) {
            var thisApp = this;
            var post = function () {
                var index = layer.load(1);
                $.post('/tools/DeleteDeliveryTemplate', $.extend({
                    appId: thisApp.appId,
                    templateId: template.Id,
                }, thisApp.appendPara)).then(function (result) {
                    layer.close(index);
                    thisApp.isloading = false;
                    if (!result.isok) {
                        layer.msg(result.Msg); return;
                    }
                    layer.msg('删除成功');
                    thisApp.getTemplateList();
                    callBack ? callBack() : null;
                })
            }
            alertMsg = template.ApplyCount ? String.raw`有${template.ApplyCount}件商品在使用该模板，确认还要删除吗？（共${template.ApplyCount}件商品运费将会清零）` : '确认删除模板吗？';
            layer.confirm(alertMsg, { title: '提示', icon: 7, }, function (index) {
                layer.close(index);
                post();
            });
        },
        //更新运费配置
        updateConfig: function () {
            thisApp = this;
            var post = function () {
                if (!thisApp.isValidNumber(thisApp.freight) || thisApp.freight < 0) {
                    thisApp.freight = 0;
                }
                var index = layer.load(1);
                $.post('/tools/UpdateDeliveryConfig', $.extend({
                    appId: thisApp.appId,
                    pageType: thisApp.pageType,
                    rule: thisApp.selectRule,
                    enable: thisApp.enable,
                    EnableDiscount: thisApp.discountEnable,
                    discount: thisApp.discount,
                }, thisApp.appendPara)).then(function (result) {
                    layer.close(index);
                    thisApp.isloading = false;
                    if (thisApp.setUniform) {
                        thisApp.enableTemplate = 0;
                    }
                    layer.msg(result.isok ? '保存成功' : result.msg);
                });
            }

            layer.confirm('确定选择保存规则吗?', function (index) {
                layer.close(index);
                post();
            });
        },

        //----------管理配送区域----------//
        //获取区域列表数据
        getArea: function (bindData, areaId) {
            if (!areaId) { bindData([]); return; }
            var thisApp = this;
            thisApp.isloadingRegion = true;
            $.get('/Tools/GetAreaList', $.extend({
                appId: thisApp.appId,
                areaId: areaId
            }, thisApp.appendPara)).then(function (result) {
                thisApp.isloadingRegion = false;
                //排序
                result.dataObj.sort((x, y) => {
                    return x.Code - y.Code;
                })
                //去除国外区域（什么鬼数据 ~ (╯‵□′)╯︵┻━┻ ）
                var excludeArea = [710000, 810000, 820000, 900000, 910000, 920000, 930000, 110100, 110200, 120100, 120200];
                excludeArea.forEach((code) => {
                    var removeIndex = result.dataObj.findIndex((thisArea, index) => {
                        return thisArea.Code === code;
                    })
                    if (removeIndex > -1) {
                        result.dataObj.splice(removeIndex, 1);
                    }
                });
                bindData(result.dataObj);
            });
        },
        //获取配送区域
        getDeliveryRegion: function () {
            var thisApp = this;
            if (!thisApp.editTemplateId) {
                return;
            }
            thisApp.isloadingRegion = true;
            $.get("/tools/GetDeliveryTemplate", $.extend({
                appId: thisApp.appId,
                pageType: thisApp.pageType,
                pageIndex: 1,
                pageSize: 99,
                parentId: thisApp.editTemplateId,
            }, thisApp.appendPara)).then(function (result) {
                thisApp.isloadingRegion = false;
                if (!result.isok) {
                    layer.msg(result.Msg);
                    return;
                }
                if (result.dataObj && result.dataObj.length) {
                    thisApp.currRegions = result.dataObj;
                }
            });
        },
        //新增区域运费设置
        addRegion: function () {
            this.newRegion = null;
            this.isEditRegion = true;
        },
        //编辑区域运费设置
        editRegion: function (region) {
            this.newRegion = region;
            this.isEditRegion = true;
            this.regionTab = 'province';
        },
        //保存区配送区域
        confirmRegion: function () {
            var thisApp = this;
            var newRegion = thisApp.newRegion;
            if (!thisApp.checkRegionInput(newRegion)) {
                return;
            }

            if (newRegion.Id) {
                //编辑区域：立刻更新
                thisApp.commitRegion([newRegion]);
                return;
            }

            if (newRegion.localId) {
                //新增区域
                var index = thisApp.newRegions.findIndex(region => region.localId === newRegion.localId);
                if (index > -1) {
                    //编辑新增区域
                    thisApp.newRegions.splice(index, 1);
                }
                thisApp.newRegions.unshift(newRegion);
                thisApp.isEditRegion = false;
            }

            if (thisApp.isEdit) {
                //编辑模板：
                thisApp.commitRegion([newRegion])
            }
        },
        //提交配送区域
        commitRegion: function (region) {
            var thisApp = this;
            thisApp.isUpdating = true;
            //保存提交模板
            $.post('/tools/UpdateDeliveryTemplate', $.extend({
                appId: thisApp.appId,
                templateId: thisApp.editTemplateId,
                newTemplate: '重要占位符（勿动）',
                regionTemplate: region,
            }, thisApp.appendPara)).then(function (result) {
                thisApp.isUpdating = false;
                if (!result.isok) {
                    layer.msg(result.Msg);
                    return;
                }
                layer.msg('保存成功');
                thisApp.isEditRegion = false;
                //清空区域更新队列
                thisApp.newRegions = [];
                //刷新列表
                thisApp.getDeliveryRegion();
            });
        },
        //删除配送区域
        removeRegion: function (region) {
            var thisApp = this;

            if (region.Id) {
                if (thisApp.currRegions.length <= 1) {
                    layer.msg('配送区域不能为空');
                    return;
                }
                thisApp.deleteTemplate(region, thisApp.getDeliveryRegion);
                return;
            }

            var removeEvent = function () {
                var index = thisApp.newRegions.findIndex(thisRegion => thisRegion.localId === region.localId);
                if (index === -1) {
                    return;
                }
                thisApp.newRegions.splice(index, 1);
            }

            layer.confirm('确认删除吗?', { title: '提示' }, function (index) {
                layer.close(index);
                removeEvent();
            });
        },

        //----------管理商品模板----------//
        //获取商品列表
        getProducts: function (pageIndex, callBack, templateId) {
            var thisApp = this;
            $.get('/Tools/GetProductList', $.extend({
                appId: thisApp.appId,
                pageIndex: pageIndex,
                templateId: templateId,
            }, thisApp.appendPara)).then(function (result) {
                if (!result.isok) {
                    return;
                }
                callBack(result.dataObj)
            });
        },
        //获取可选商品列表
        getAvailableProduct: function (pageIndex) {
            var thisApp = this;
            thisApp.getProducts(pageIndex, function (result) {
                thisApp.products = result.products;
                if (pageIndex === 1) {
                    thisApp.productTotal = result.count;
                }
            });
        },
        //获取已选中商品列表
        getSelectedProduct: function (pageIndex, templateId) {
            var thisApp = this;
            thisApp.isLoadingProduct = true;
            thisApp.getProducts(pageIndex, function (result) {
                thisApp.isLoadingProduct = false;
                thisApp.selProduct = result.products.map(product => product.Id);
                thisApp.selProductData = result.products;
                if (pageIndex === 1) {
                    thisApp.selProductTotal = thisApp.count;
                }
            }, templateId);
        },
        //编辑商品设置运费模板
        editProduct: function (template) {
            this.editTemplateId = template.Id;
            this.isEditProduct = true;
            if (this.selProductIndex === 1) {
                this.getSelectedProduct(1, template.Id);
            } else {
                this.selProductIndex = 1;
            }
        },
        //保存商品设置运费模板
        saveProduct: function () {
            var thisApp = this;
            if (thisApp.isUpdating) {
                return;
            }
            thisApp.isUpdating = true;
            $.post('/Tools/UpdateProductTemplate', {
                appId: thisApp.appId,
                templateId: thisApp.editTemplateId,
                productIds: thisApp.selProduct.join(','),
            }).then(function (result) {
                thisApp.isUpdating = false;
                if (!result.isok) {
                    layer.msg(result.Msg);
                    return;
                }
                thisApp.getTemplateList();
            });
        },

        //----------其它----------//
        //检验用户输入运费模板
        checkInput: function (template) {
            app = this;
            var isAllVaild = true;
            var inputTemplate = template;
            var isFree = inputTemplate.IsFree;

            if (!inputTemplate.Name) {
                app.templateTab = 'basic';
                setTimeout(function () { app.showMessage('请输入模板名称', app.$refs.templateName); }, 120);
                isAllVaild = false;
            }

            if (!isFree && (!app.regionTemplate || !app.regionTemplate.length)) {
                app.templateTab = 'region';
                setTimeout(function () { app.showMessage('请设置配送区域', app.$refs.regionBtn) }, 120);
                isAllVaild = false;
            }

            return isAllVaild;
        },
        //检查用户输入区域运费
        checkRegionInput: function (region) {
            app = this;
            var isAllVaild = true;
            var inputTemplate = region;
            var isFree = inputTemplate.IsFree;

            if (!this.isValidNumber(inputTemplate.Base) || inputTemplate.Base === 0) {
                this.showMessage('请输入' + this.unitTitle, app.$refs.Base);
                isAllVaild = isFree;
                if (isFree) {
                    inputTemplate.Base = 1;
                }
            }

            if (!this.isValidNumber(inputTemplate.Extra)) {
                this.showMessage('请输入' + this.unitTitle, app.$refs.Extra);
                isAllVaild = isFree;
                if (isFree) {
                    inputTemplate.Extra = 0;
                }
            }

            if (!this.isValidNumber(inputTemplate.BaseCost)) {
                this.showMessage('请输入有效价格', app.$refs.BaseCost);
                isAllVaild = isFree;
                if (isFree) {
                    inputTemplate.BaseCost = 0;
                }
            }

            if (!this.isValidNumber(inputTemplate.ExtraCost)) {
                this.showMessage('请输入有效价格', app.$refs.ExtraCost);
                isAllVaild = isFree;
                if (isFree) {
                    inputTemplate.ExtraCost = 0;
                }
            }

            if (!inputTemplate.DeliveryRegion || !inputTemplate.DeliveryRegion.trim() || inputTemplate.DeliveryRegion.trim() === '0') {
                this.showMessage('请选择配送区域', app.$refs.DeliveryRegion);
                isAllVaild = isFree;
                if (isFree) {
                    inputTemplate.ExtraCost = 0;
                }
            }

            return isAllVaild;
        },
        //检验数字
        isValidNumber: function (number) {
            inputNumber = parseInt(number);
            return !isNaN(inputNumber) && inputNumber >= 0;
        },
        //表单提示
        showMessage: function (msg, el) {
            layer.tips(msg, el, { tips: [1, '#20a0ff'] });
        },
        //切换模板列表Tab
        switchTab: function () {
            if (this.tab) {
                this.getTemplateList();
            }
        },
        //序列化显示配送区域
        formatRegion: function (region) {
            if (!region.DeliveryRegion) {
                return '全国配送';
            }
            var allCode = region.DeliveryRegion.split(',').map(code => Number(code));
            var allCity = this.regionSubData.filter(city => allCode.indexOf(city.Code) > -1);
            var allProvince = this.regionData.filter(province => allCity.find(city => city.ParentCode === province.Code) || allCode.indexOf(province.Code) > -1);

            var formatRegion = '';
            allProvince.forEach((province) => {
                var citys = allCity.filter(city => city.ParentCode === province.Code).map(city => city.Name).join(',');
                formatRegion += String.raw`<b>${province.Name}</b>（${citys ? citys + '）</br>': '全省/直辖市）'}`;
            });
            return formatRegion.trim() ? formatRegion : '全国配送';
        },
        //绑定省份列表控件数据
        bindProvice: function (data) {
            this.regionData = data;
            this.getArea(this.bindCity, data.map(area=>area.Code).join(','));
        },
        //绑定市区列表控件数据
        bindCity: function (data) {
            this.regionSubData = data;
        },
        //模糊搜索区域
        filterRegion: function (query, item) {
            return item.PingYin.indexOf(query.toUpperCase()) > -1;
        },
        //获取省份区域码
        getParentCode: function (regions) {
            var allCode = regions.map(template => template.DeliveryRegion).join(',').split(',').map(code => Number(code));
            //获取
            var parentCode = allCode.map(code => Math.floor(code * 0.0001) * 10000);
            //去重
            return Array.from(new Set(parentCode));
        },
        //获取市级区域码
        getSubCode: function (regions) {
            //获取
            var allCode = regions.map(template => template.DeliveryRegion).join(',').split(',').map(code => Number(code));
            var cityCode = this.regionSubData.filter(region => allCode.find(code => code === region.ParentCode || code === region.Code)).map(region => region.Code);
            //去重
            return Array.from(new Set(cityCode));
        },
        //安全计算
        safeCal: function (x, y) {
            return Number(parseFloat(x * y).toFixed(2));
        },
        //安全相除
        safeDiv: function (x, y) {
            return Number(parseFloat(x / y).toFixed(2));
        }
    },
}