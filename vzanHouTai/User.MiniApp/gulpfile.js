
//*：匹配任意数量的字符，不包括/
//?：匹配单个字符，不包括/
//**：匹配任意数量的字符，包括/
//{}：允许使用逗号分隔的列表，表示“or”（或）关系
//!：用于模式的开头，表示只返回不匹配的情况



var app = {  // 定义目录
    srcPath: 'Content/newhome',
    destPath: 'build/Content/newhome',
    destBootPath: 'build'
};
var gulp = require('gulp');
// 引入组件
var minifycss = require('gulp-clean-css'),//css压缩
    less = require('gulp-less'),//编译less
    uglify = require('gulp-uglify'),//js压缩
    concat = require('gulp-concat'),//文件合并
    rename = require('gulp-rename'),//文件更名
    rev = require('gulp-rev'),//给文件名添加版本号
    del = require('del'),//删除旧版本
    bom = require('gulp-bom'),//添加bom防止中文乱码
    revCollector = require('gulp-rev-collector'),//路径替换
    gulpSequence = require('gulp-sequence'), //按顺序执行任务
    //notify = require('gulp-notify'),//提示信息
    requirejsOptimize = require('gulp-requirejs-optimize'),//打包require.js
    babel = require('gulp-babel'),
    ts = require('gulp-typescript');//编译es6语法



/******************** layoutcss文件 ***************************/
//清理
gulp.task('clearcss', function () {
    return del(app.destPath + '/css/*.css');
});
//编译less文件成css,
gulp.task('compile_less', function () {
    return gulp.src([app.srcPath + '/css/homeless/app.less', app.srcPath + '/css/indexDl.less'])
    .pipe(less())
    .pipe(gulp.dest(app.srcPath + '/css/'))
})

//压缩css，添加版本号
gulp.task('build_css', function () {
    return gulp.src([app.srcPath + '/css/app.css', app.srcPath + '/css/indexDl.css'])
        .pipe(rev())
        .pipe(gulp.dest(app.destPath + '/css/'))
        .pipe(rev.manifest({//CSS生成文件hash编码并生成 rev-manifest.json文件名对照映射
            merge: true
        }))
        .pipe(gulp.dest(''))
});
//更改引用
gulp.task('newappcss', function () {
    return gulp.src(['rev-manifest.json', 'Views/Shared/_HomeLayout.cshtml'])
        .pipe(revCollector({ replaceReved: true }))
        .pipe(bom())
        .pipe(gulp.dest(app.destBootPath + '/Views/Shared/'));
});
//更改引用
gulp.task('newdlcss', function () {
    return gulp.src(['rev-manifest.json', 'Views/dzhome/indexDl.cshtml'])
        .pipe(revCollector({ replaceReved: true }))
        .pipe(bom())
        .pipe(gulp.dest(app.destBootPath + '/Views/dzhome/'));
});
//清理
gulp.task('clearjs', function () {
    return del(app.destPath + '/js/*.js');
});
// 合并、压缩 js文件
gulp.task('build_js', function () {
    return gulp.src([app.srcPath + '/js/homeLayout.js', app.srcPath + '/js/newHome.js', app.srcPath + '/js/indexDl.js'])
        //.pipe(concat('layout.js'))
        .pipe(ts({
            target: "es5",
            allowJs: true,
            module: "commonjs",
            moduleResolution: "node"
        }))
        .pipe(uglify())
        .pipe(rev())
        .pipe(gulp.dest(app.destPath + '/js/'))
        .pipe(rev.manifest({
            merge: true
        }))
        .pipe(gulp.dest(''))
});
//更改引用
gulp.task('newlayoutjs', function () {
    return gulp.src(['rev-manifest.json', 'Views/Shared/_HomeLayout.cshtml'])
        .pipe(revCollector({
            replaceReved: true
        }))
        .pipe(bom())
        .pipe(gulp.dest(app.destBootPath + '/Views/Shared/'));
});
//更改引用
gulp.task('newhomejs', function () {
    return gulp.src(['rev-manifest.json', 'Views/dzhome/newHome.cshtml'])  
        .pipe(revCollector({                                                //执行文件内文件名替换
            replaceReved: true
        }))
        .pipe(bom())
        .pipe(gulp.dest(app.destBootPath + '/Views/dzhome/'));                             //- 替换后的文件输出的目录
});
//更改引用
gulp.task('newdljs', function () {
    return gulp.src(['rev-manifest.json', 'Views/dzhome/indexDl.cshtml'])
        .pipe(revCollector({ replaceReved: true }))
        .pipe(bom())
        .pipe(gulp.dest(app.destBootPath + '/Views/dzhome/'));
});

//清理app.js,app_pro.js
gulp.task('clear_app', function (cb) {
    return del('build/content/enterprise/*.js', cb);
});
//构建app.js,app_pro.js
gulp.task("build_app", function () {
    return gulp.src(['Content/enterprise/app.js', 'Content/enterprise/app_pro.js', 'Content/enterprise/app_multiStore.js'])

        .pipe(requirejsOptimize(function (file) {
            return {
                optimize: 'none',//uglify2
                baseUrl: 'Content/enterprise/',
            };
        }))
        .pipe(babel({
            presets: ['es2015']
        }))
        .pipe(uglify())
        .pipe(rev())
        .pipe(gulp.dest('build/content/enterprise/'))
        .pipe(rev.manifest({
            merge: true
        }))
        .pipe(gulp.dest(''));

});
//更改app.js,app_pro.js在页面中的引用
gulp.task('usenew_app', function () {
    return gulp.src(['rev-manifest.json', 'Views/enterprise/pageset.cshtml'])
        .pipe(revCollector({
            replaceReved: true
        }))
        .pipe(bom())
        //- 执行文件内css名的替换
        .pipe(gulp.dest('build/Views/enterprise/'));
});

//生成皮肤样式 begin
//删除皮肤样式
gulp.task('clear_skin', function () {
    return del('Content/enterprise/css/skin.css');
});
//编译皮肤less
gulp.task('build_skin', function () {
    return gulp.src('Content/enterprise/css/skin.less')
        .pipe(less())
        //.pipe(minifycss())
        //解析less到css中调试
        .pipe(gulp.dest('Content/enterprise/css/'))

    //.pipe(rev())
    //.pipe(gulp.dest(app.destPath + '/css/'))
    //.pipe(rev.manifest({
    //    merge: true
    //}))
    //.pipe(gulp.dest(''))
});
/*----------------------------生成小程序用的皮肤-------------------------*/
gulp.task('build_miniapp_skin', function () {
    gulp.src('../miniapp/EnterprisePro/skinColor/skin.less')
        //.pipe(gulpless())
        .pipe(less())
        //.pipe(minifycss())
        .pipe(rename("skin.wxss"))
        .pipe(gulp.dest("../miniapp/EnterprisePro/skinColor/"))
});

//生成皮肤样式 end 


/**************************************************/

// 默认任务
gulp.task('default', function () {
    gulpSequence(
        'clearcss', 'clearjs',
         'compile_less', 'build_css','newappcss','newdlcss', 
         'build_js', 'newlayoutjs', 'newdljs','newhomejs',
        'build_skin',
        'build_miniapp_skin'
    )(function (err) {
        if (err) console.log(err)
    })
    //gulp.run('dlless');
    // 监听 库及插件改变则更新到压缩项目中
    //gulp.watch(app.srcPath + 'lib/**/*', ['lib']);
    // 监听less
    gulp.watch(app.srcPath + '/css/**/*.less', ['compile_less']);
    // 监听css
    gulp.watch(app.srcPath + '/css/**/*.css', ['clearcss', 'build_css', 'newappcss', 'newdlcss']);
    // 监听js
    gulp.watch(app.srcPath + '/js/**/*.js', ['clearjs', 'build_js', 'newhomejs', 'newlayoutjs']);
    //监听皮肤
    gulp.watch('Content/enterprise/css/skin.less', ['build_skin']);

    //监听小程序皮肤
    gulp.watch('../miniapp/EnterprisePro/skinColor/skin.less', ['build_miniapp_skin']);
    //生成专业版js
    gulpSequence(
        'clear_app', 'build_app', 'usenew_app')(function (err) {
            if (err) console.log(err)
        })
    gulp.watch(['Views/enterprise/pageset.cshtml'], function () {
        gulpSequence('usenew_app')(function (err) {
            if (err) console.log(err)
        })
    });
    gulp.watch(['Content/enterprise/*.js'], function () {
        gulpSequence(
            'clear_app', 'build_app', 'usenew_app')(function (err) {
                if (err) console.log(err)
            })
    });
});