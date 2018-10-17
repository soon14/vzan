var AppTools = function () {

    // 远程加载 Modal
    var _loadModal = function (selector, url, params, options, callback) {
        $('body').modalmanager('loading');
        var $modal = $(selector);

        $modal.load(url, params, function () {

            if (callback && callback instanceof Function) {
                callback();
            }

            var mops = $.extend({ width: 800 }, options);

            $modal.modal(mops);

            var form = $(selector + " form")
                        .removeData("validator") /* added by the raw jquery.validate plugin */
                        .removeData("unobtrusiveValidation");  /* added by the jquery unobtrusive plugin */

            $.validator.unobtrusive.parse(form);

            $modal.find("input[type=checkbox]").uniform();
            $modal.find(" input[type=radio]").uniform();

        });
    }

    var _alert = function (message, callback) {
        bootbox.alert(message, callback);
    }

    var _confirm = function (message, callback) {
        bootbox.confirm(message, callback);
    }

    var _dialog = function (options) {
        bootbox.dialog(options);
    }

    var _pulsate = function (selector) {
        $(selector).pulsate({
            color: "#ff3300",
            reach: 10,
            repeat: 3,
            speed: 250,
            glow: true
        });
    }

    var _blockUI = function (selector) {
        Metronic.blockUI({
            target: selector,
            iconOnly: true
        });
    }

    var _unblockUI = function (selector) {
        Metronic.unblockUI($(selector));
    }

    var _notify = function (msg, success) {
        var settings = {
            theme: success ? "amethyst" : "lime", // amethyst,tangerine,lime
            sticky: false,
            horizontalEdge: "bottom",
            verticalEdge: "right"
        };
        $button = $(this);

        if (!settings.sticky) {
            settings.life = 5000;
        }
        settings.heading = "<i class='icon-info'></i> 提示"
        $.notific8('zindex', 11500);
        $.notific8(msg, settings);
    }

    var _goto = function (obj) {
        var formId = $(obj).attr("formId");

        var $input = $("input[formId=" + formId + "]");
        var p = Number($input.val());
        if (!/^[0-9]+$/.test(p)) {
            App.Notify("页码格式不正确，请输入数字", true);
            $input.val("");
            return;
        }

        var total = $(".gridpager").find(".timelyTotalPage").attr("timelytotalpage");
        if (p < 1) p = 1;
        if (p > total) p = total;
        $(formId + " input[name=__pageIndex]").val(p);
        $(formId).submit();
    }

    var _initGridView = function () {
        $(".gridpager .size").die().live("change", function (e) {
            var jq = $(this);
            var formId = jq.closest("div.gridpager").attr("form");
            $("#" + formId + " [name=__pageSize]").val(jq.val());
            $("#" + formId).submit();
        });


        $("input.goto").die().live("keydown", function (e) {
            if (e.keyCode == 13)
                _goto(this);
        });

        $("button.goto").die().live("click", function () {
            _goto(this);
        });
    }

    var _init = function () {
        _initGridView();

        $("#btnLoadProfile").click(function () {
            App.LoadModal("#profileModel", "/account/userinfo", { id: $(this).attr("userId") }, { width: 600 });
        });

        $("#btnResetPassword").click(function () {
            App.LoadModal("#resetPasswordModel", "/account/resetpassword", { id: $(this).attr("userId") }, { width: 600 });
        });

        $("button[type=reset]").live("click", function () {
            $(this).closest("form")[0].reset();
            $("input[type=checkbox]").uniform();
            $(" input[type=radio]").uniform();
        });

    }

    var _registerDate = function (obj) {
        $(obj).each(function (i, obj) {
            var formatType = $(obj).attr("data-format");
            $(obj).datetimepicker({
                autoclose: true,
                language: 'zh-CN',
                formshowSeconds: true,
                format: formatType ? (formatType == "0" ? "yyyy-mm-dd" : "yyyy-mm-dd hh:ii:ss") : "yyyy-mm-dd",
                minView: formatType ? (formatType == "0" ? "month" : "hour") : "month",
                maxView: "decade",
                todayBtn: 1,
                startView: 2,
                clearBtn: 1,
                todayHighlight: 1,
                startDate: $(obj).attr("data-start") ? $(obj).attr("data-start") : null,
                endtDate: $(obj).attr("data-end") ? $(obj).attr("data-start") : null,
                pickerPosition: (Metronic.isRTL() ? "bottom-right" : "bottom-left")
            });
        });
    }

    var _exportFile = function (formId, controller, action) {
        var form = document.createElement("form");
        form.method = 'post';
        form.id = "__exportForm";
        form.style.display = "none";
        form.action = "/" + controller + "/" + action + "";
        form.target = '_self';
        document.body.appendChild(form);

        var para = $("#" + formId).serializeArray();
        $.each(para, function (i, field) {
            var input = document.createElement("input");
            input.type = "hidden";
            input.name = field.name;
            input.value = field.value;
            form.appendChild(input);
        });
        form.submit();
        $("#__exportForm").remove();
    }

    var _reloadBefore = function (decount) {
        var curPageRows = $("tbody tr").length;
        var curPageIndex = $("input[name='__pageIndex']").val();
        var totalPage = $(".gridpager").find(".timelyTotalPage").attr("timelytotalpage");
        if (decount == curPageRows && curPageIndex > 1 && curPageIndex == totalPage) {
            curPageIndex = curPageIndex - 1;
            $("input[name='__pageIndex']").val(curPageIndex);
        }
    }

    return {
        Init: _init,
        LoadModal: _loadModal,
        Alert: _alert,
        Confirm: _confirm,
        Dialog: _dialog,
        Pulsate: _pulsate,
        BlockUI: _blockUI,
        UnblockUI: _unblockUI,
        Notify: _notify,
        exportFile: _exportFile,
        RegisterDate: _registerDate,
        reloadBefore: _reloadBefore
    };

}();