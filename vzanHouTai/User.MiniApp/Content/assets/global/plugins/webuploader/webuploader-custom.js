var $list = $('.uploader-list');

// 初始化Web Uploader
var uploader = WebUploader.create({

    // 选完文件后，是否自动上传。
    auto: false,

    // swf文件路径
    swf: 'Uploader.swf',

    // 文件接收服务端。
    server: '/Upload/UploadImg',

    // 选择文件的按钮。可选。
    // 内部根据当前运行是创建，可能是input元素，也可能是flash.
    pick: '.filePicker',

    // 是否支持多选
    multiple: false,

    // 只允许选择图片文件。
    accept: {
        title: 'Images',
        extensions: 'gif,jpg,jpeg,bmp,png',
        mimeTypes: 'image/jpeg,image/png,image/gif'
    },

    // 限制图片数量
    fileNumLimit: 5,
    // 单张图片大小限制
    fileSingleSizeLimit: 1 * 1024 * 1024, //1M

    // 压缩
    compress: {
        width: 700,
        height: 700,

        // 图片质量，只有type为`image/jpeg`的时候才有效。
        quality: 50,

        // 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
        allowMagnify: false,

        // 是否允许裁剪。
        crop: false,

        // 是否保留头部meta信息。
        preserveHeaders: true,

        // 如果发现压缩后文件大小比原来还大，则使用原来图片
        // 此属性可能会影响图片自动纠正功能
        noCompressIfLarger: false,

        // 单位字节，如果图片大小小于此值，不会采用压缩。
        compressSize: 0
    }
});

// 当有文件添加进来的时候
uploader.on('fileQueued', function (file) {
    var $li = $(
            '<div id="' + file.id + '" class="file-item thumbnail">' +
            '<img>' +
            '<div class="info">' + file.name + '</div>' +
            '</div>'
        ),
        $img = $li.find('img');
    $btns = $('<div class="file-panel">' +
        '<span class="cancel">删除</span></div>')
    .appendTo($li);


    // $list为容器jQuery实例
    $list.append($li);
    $li.on('mouseenter', function () {
        $btns.stop().animate({ height: 30 });
    });

    $li.on('mouseleave', function () {
        $btns.stop().animate({ height: 0 });
    });

    // 创建缩略图
    // 如果为非图片文件，可以不用调用此方法。
    // thumbnailWidth x thumbnailHeight 为 100 x 100
    uploader.makeThumb(file, function (error, src) {
        if (error) {
            $img.replaceWith('<span>不能预览</span>');
            return;
        }

        $img.attr('src', src);
    }, 100, 100);
});

uploader.on('fileQueued', function (file) {
    $btns.on('click', 'span', function () {
        $(this).parent().parent().remove();
        uploader.removeFile(file);
    });
});

// 文件上传过程中创建进度条实时显示。
uploader.on('uploadProgress', function (file, percentage) {
    var $li = $('#' + file.id),
        $percent = $li.find('.progress span');

    // 避免重复创建
    if (!$percent.length) {
        $percent = $('<p class="progress"><span></span></p>')
            .appendTo($li)
            .find('span');
    }

    $percent.css('width', percentage * 100 + '%');
});

// 文件上传成功，给item添加成功class, 用样式标记上传成功。
uploader.on('uploadSuccess', function (file, response) {
    $('#' + file.id).addClass('upload-state-done');
    if (response.Success) {
        $('#' + file.id).append("<input type='hidden' value='" + response.Path + "'/>");
    }
});

// 文件上传失败，显示上传出错。
uploader.on('uploadError', function (file) {
    var $li = $('#' + file.id),
        $error = $li.find('div.error');

    // 避免重复创建
    if (!$error.length) {
        $error = $('<div class="error"></div>').appendTo($li);
    }

    $error.text('上传失败');
});

// 完成上传完了，成功或者失败，先删除进度条。
uploader.on('uploadComplete', function (file) {
    $('#' + file.id).find('.progress').remove();
});

