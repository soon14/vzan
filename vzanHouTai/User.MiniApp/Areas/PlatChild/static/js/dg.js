"use strict";
(function (dg) {
	layer.config({moveType: 1, shade: 0.1, shift: 0,});//extend: 'extend/layer.ext.js'

	/**
	 * 信息框
	 * @param {string} msg
	 * @param {object} [options]
	 * @param {Function} [callback]
	 */
	dg.alert = function (msg, callback, options) {
		layer.alert(msg, options, callback);
	};

	/**
	 * 询问框
	 * @param {string} msg
	 * @param {object} [options]
	 * @param {Function} [callback]
	 */
	dg.confirm = function (msg, callback, options) {
		layer.confirm(msg, options, function (index) {
			if (callback({confirm: true, cancel: false}) !== false)
				layer.close(index);
		}, function () {
			callback({confirm: false, cancel: true});
		});
	};

	/**
	 * 提示框
	 * @param {string} msg
	 * @param {object} [options]
	 * @param {Function} [callback]
	 */
	dg.toast = function (msg, options, callback) {
		layer.msg(msg, options, callback);
	};

	/**
	 * 输入层
	 * @param {string} msg
	 * @param {object} [value]
	 * @param {Function} [callback]
	 */
	dg.prompt = function (msg, value, callback) {
		layer.prompt({title: msg, value: value}, function (value, index) {
			if (callback(value) !== false) layer.close(index);
		});
	};

	/**
	 * 加载层索引
	 * @type {Number}
	 */
	var loadingIndex = null;

	/**
	 * 显示loading
	 */
	dg.showLoading = function () {
		this.hideLoading();
		loadingIndex = layer.load(0, {anim: 5, shift: 5, shade: false});
	};

	/**
	 * 隐藏loading
	 */
	dg.hideLoading = function () {
		if (loadingIndex) {
			layer.close(loadingIndex);
		}
	};

})(window.dg || ( window.dg = {}));

/**
 * ajax
 */
(function () {
	/**
	 * get请求
	 * @param {string} url
	 * @param {string,object} data
	 * @param {Function} callback
	 */
	dg.get = function (url, data, callback) {
		return dg.request(url, "GET", data, {success: callback});
	};

	/**
	 * post请求
	 * @param {string} url
	 * @param {string,object} data
	 * @param {Function} callback
	 */
	dg.post = function (url, data, callback) {
		return dg.request(url, "POST", data, {success: callback});
	};

	/**
	 * ajax 请求
	 * @param {string} url
	 * @param {string} method
	 * @param {string,object} data
	 * @param {object} options
	 */
	dg.request = function (url, method, data, options) {
		return $.ajax({
			url: url,
			type: method,
			data: data,
			dataType: options.dataType || "json",
			beforeSend: function () {
				dg.showLoading();
			},
			complete: function () {
				dg.hideLoading();
			},
			success: function (res) {
				var code = res.code === undefined ? res.status : res.code,
					msg = res.info === undefined ? res.msg : res.info;

				if (code === 1) {
					if (options.success && options.success.call(null, res.data, res) === false)
						return;
					dg.toast(msg || "操作成功！");
				} else {
					if (options.error && options.error.call(null, code, msg, res) === false)
						return;
					dg.toast(msg);
				}
			},
			error: function (XMLHttpRequest, textStatus, errorThrown) {
				if (options.error && options.error.call(null, textStatus, errorThrown, XMLHttpRequest) === false) {
					return;
				}
				dg.toast('请求失败，请联系管理员！');
			}
		});
	};

})(window.dg);

/**
 * jquery 相关初始化
 */
$(function () {

	/**
	 * AJAX
	 */
	function ajax() {
		var self = $(this), url = self.attr('href') || self.attr('url') || self.data('url'),
			target = $(self.data('target') ? self.data('target') : self.attr('target')),
			isPost = self.data('ajaxPost') !== undefined,
			refresh = self.data('refresh') !== false, confirm = self.data('confirm') !== false,
			confirmContent = self.data('confirmContent') || "你确定要执行此操作吗？";

		if (!url) url = target.attr('action') || target.attr('href') || target.attr('url') || target.data('url');
		if (!url) throw new Error(self.text() + "中使用ajax，请确保url 不能为空");

		var handler = function () {
			var param = target.serialize();
			var successHandler = function (data, res) {
				self.trigger('success', [data, res]);

				if (!refresh) return;
				setTimeout(function () {
					if (res.url) window.location.href = res.url;
					else window.location.reload();
				}, 1000);
			};
			if (isPost) {
				dg.post(url, param, successHandler);
			} else {
				dg.get(url, param, successHandler);
			}
		};

		if (confirm) {
			dg.confirm(confirmContent, function (res) {
				if (res.cancel) return;
				handler();
			});
		} else {
			handler();
		}

		return false;
	}

	$(document).on('click','[data-ajax-get]', ajax);
	$(document).on('click','[data-ajax-post]', ajax);

	//查看相册
	$('[data-photos]').each(function () {
		var self = $(this), photos = self.data('photos'),
			options = {photos: this, anim: 5, shift: 5};
		photos = !photos ? {} : eval("(" + photos + ")");

		layer.photos(options);

		if (photos.dynamic) {
			self.on('click', '.photo-item', function () {
				layer.photos(options);
			});
		}
	});

	//多图上传
	$('.photolist').each(function () {
	});
	$('.photolist').on('click', '.remove', function () {
		$(this).closest('.photo-item').remove();
		event.stopPropagation();
	});

	$('[data-open]').on('click', function () {
		var self = $(this), options = self.data('open'),
			url = self.attr('href') || self.data('href') || self.data('url');
		if (!url) throw new Error("无效的请求的地址");
		options = options ? eval("(" + options + ")") : {};

		layer.open({
			type: 2, maxmin: true,
			area: [options.width || "780px", options.height || "500px"],
			content: url,
			success: function (layero, index) {
				var iframe = layero.find("iframe"),
					closeByThis = function () {
						layer.close(index);
					};
				if (iframe[0].contentWindow) {
					iframe[0].contentWindow.closeByThis = closeByThis;
					layer.title(iframe[0].contentWindow.document.title, index);
				} else {
					layero.find("iframe").on("load", function () {
						iframe[0].contentWindow.closeByThis = closeByThis;
						layer.title(iframe[0].contentWindow.document.title, index);
					});
				}
			}
		});

		return false;
	});

	$('[data-date]').each(function () {
		$(this).datetimepicker({format: 'yyyy-mm-dd', language: "zh-CN", minView: 2, autoclose: true});
	});

	$('[data-time]').each(function () {
		$(this).datetimepicker({format: 'hh:ii', language: "zh-CN", minView: 0, autoclose: true});
	});

	$('[data-datetime]').each(function () {
		$(this).datetimepicker({format: 'yyyy-mm-dd hh:ii', language: "zh-CN", minView: 0, autoclose: true});
	});

});