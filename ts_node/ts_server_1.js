// 管道流
var fs = require('fs');

//创建一个可读流
var readerStream = fs.createReadStream('input.txt');
//创建一个写入流
var writeStream = fs.createWriteStream('out.txt');

// 管道读写操作
// 读取 input.txt 文件内容，并将内容写入到 output.txt 文件中
readerStream.pipe(writeStream);



// 链式流
// 用管道和链式来压缩和解压文件。
var zlib = require('zlib');

// 压缩 input.txt 文件为 input.txt.gz
fs.createReadStream('input.txt')
    .pipe(zlib.createGzip())
    .pipe(fs.createWriteStream('input.txt.gz'));
// 解压 input.txt.gz 文件为 input.txt
fs.createReadStream('input.txt.gz')
    .pipe(zlib.createGunzip())
    .pipe(fs.createWriteStream('input.txt'));
console.log("文件解压完成。");
console.log('程序执行完毕')