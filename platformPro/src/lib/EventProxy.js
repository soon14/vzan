(function (root, factory) {
    if (typeof define === 'function' && define.amd) {
        define(function () {
            return factory(root);
        });
    } else if (typeof exports === "object" && typeof module !== "undefined") {
        module.exports = factory(root);
    } else {
        root.EventProxy = factory(root);
    }
})(typeof global !== 'undefined' ? global : typeof window !== 'undefined' ? window : this, function (window) {

    //限制类型
    var limitType = function (type) {
        if (type == 'default') throw new Error('The 1st argument must be a string except default')
    }

    var isFunction = function (v) {
        return typeof v === 'function';
    }

    var __slice = Array.prototype.slice

    //深度复制
    var deepClone = function (src) {
        var temp = {}
        for (var key in src) {
            temp[key] = src[key]
        }
        return JSON.parse(JSON.stringify(temp))
    }

    //将嵌套数组扁平化 例如：[[100, [102, 103]], 400, [500, 600]] -> [100, 102, 103, 400, 500, 600]
    var flattening = function (arr) {
        var newArr = []
        var len = arr.length
        var hasArray = false
        for (var i = 0; i < len; i++) {
            var item = arr[i]
            if (Array.isArray(item)) {
                newArr = newArr.concat(item)
                hasArray = true
            } else {
                newArr.push(item)
            }
        }
        if (!hasArray) return newArr
        return flattening(newArr)
    }

    var findIndex = function (arr, key) {
        for (var i = 0; i < arr.length; i++) {
            if (arr[i] == key) return i
        }
        return false
    }


    //构造器
    var EventProxy = function () {}


    //事件库
    EventProxy.prototype.$eventLibary = {
        'default': {
            type: 'default',
            queue: [],
            isOnce: false
        }
    }


    //订阅事件
    EventProxy.prototype.on = function (type, handler) {
        limitType(type)

        if (!isFunction(handler)) {
            throw new Error('The handler of the event of ' + type + ' is not a function')
        }

        var $eventLibary = this.$eventLibary
        var currEventModel = $eventLibary[type]
        if (currEventModel) {
            return currEventModel.queue.push(handler)
        }
        $eventLibary[type] = deepClone($eventLibary['default'])
        $eventLibary[type].type = type
        $eventLibary[type].queue.push(handler)
    }


    //订阅事件 只触发一次
    EventProxy.prototype.once = function (type, handler) {
        this.on(type, handler)
        this.$eventLibary[type].isOnce = true
    }


    //事件触发
    EventProxy.prototype.emit = function () {
        var type = arguments[0]
        limitType(type)
        var args = __slice.call(arguments, 1)
        var eventModel = this.$eventLibary[type]
        var $aboutAll = this.$aboutAll
        var allEventQueueIndex = findIndex($aboutAll.eventQueue, type)

        if (allEventQueueIndex !== false) {
            this.$emitAll($aboutAll, args, allEventQueueIndex);
        } else if (eventModel && Array.isArray(eventModel.queue) && eventModel.queue.length > 0) {
            this.$emitNormal(eventModel, args)
        } else {
            throw new Error('The event of ' + type + ' is not exist')
        }
    }


    EventProxy.prototype.$aboutAll = {
        eventQueue: [],
        callBackArgs: [],
        success: null,
        reset: function () {
            this.eventQueue = []
            this.callBackArgs = []
            this.success = null
        }
    }


    EventProxy.prototype.$emitNormal = function (model, parameter) {
        var isOnce = model.isOnce
        if (isOnce === 'ran') return;
        var loop = 0,
            item;
        var queue = model.queue
        while (item = queue[loop++]) {
            item.apply(null, parameter)
        }
        if (isOnce) model.isOnce = 'ran'
    }


    EventProxy.prototype.$emitAll = function ($aboutAll, parameter, index) {
        var eventQueue = $aboutAll.eventQueue
        var callBackArgs = $aboutAll.callBackArgs
        eventQueue.splice(index, 1)
        callBackArgs[index] = parameter
        //All事件队列已全部发布标志
        if (eventQueue.length == 0) {
            //合并参数，触发success回调，重置有关all的辅助参数
            $aboutAll.callBackArgs = flattening(callBackArgs)
            $aboutAll.success.call(null, $aboutAll.callBackArgs)
            this.$aboutAll.reset()
        }
    }


    //当eventQueue触发完后时，调用success
    EventProxy.prototype.all = function (eventQueue, success) {
        if (!Array.isArray(eventQueue) || !isFunction(success)) return
        var $aboutAll = this.$aboutAll
        $aboutAll.eventQueue = $aboutAll.eventQueue.concat(eventQueue)
        $aboutAll.success = success
    }

    return EventProxy
})