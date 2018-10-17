var util = require('util');
// util.inherits(constructor, superConstructor)是一个实现对象间原型继承 的函数。
function Base() {
    this.name = 'base';
    this.base = 1991;
    this.sayHello = function () {
        console.log('hello' + this.name);

    };
}

Base.prototype.showName = function () {
    console.log(this.name);
};

function Sub() {
    this.name = 'sub';
}
util.inherits(Sub, Base);
var objBase = new Base();
objBase.showName();
objBase.sayHello();
console.log(objBase);
var objSub = new Sub();
objSub.showName();
console.log(objSub);

//util.inspect(object,[showHidden],[depth],[colors])是一个将任意对象转换 为字符串的方法，通常用于调试和错误输出
function person() {
    this.name = 'Kaaden';
    this.toString = function () {
        return this.name
    }
}

var _obj=new person();
console.log(util.inspect(_obj))
console.log(util.inspect(_obj,true))

// util.isArray(object)如果给定的参数 "object" 是一个数组返回true，否则返回false。
console.log(util.isArray([]))
console.log(util.isArray(new Array))
console.log(util.isArray({}))