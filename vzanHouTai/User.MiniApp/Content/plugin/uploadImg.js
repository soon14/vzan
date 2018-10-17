var uploadChild = {
    props: {
        uploadData: {
        },
    },
    data: function () {
        return {
            uploader: {
                tempFilePath: null,
                img: {
                    instalce: null,
                    filePath: '',
                    element: null,
                    fileNames:[],
                },
                video: {
                    instalce: null,
                    filePath: '',
                    element: null
                },
                audio: {
                    instalce: null,
                    filePath: '',
                    element: null
                }
            },
            imgName: '',
        }
    },
    template: '<div style="display:none;"><button id="browse_file_img">上传图片</button><button id="start_upload_img" v-on:click="uploadFile(\'img\',$event)">开始上传</button><button id="browse_file_video">上传视频</button><button id="start_upload_video" v-on:click="uploadFile(\'video\',$event)">开始上传</button><button id="browse_file_audio">上传音频</button><button id="start_upload_audio" v-on:click="uploadFile(\'audio\',$event)">开始上传</button></div>',
    methods: {
        pickImg: function (imgName) {
            this.imgName = imgName;
            $("#browse_file_img").click();
        },
        initUploader: function (type) {
            var that = this;
            var options = {
                browse_button: 'browse_file_' + type,
                url: 'http://oss.aliyuncs.com',
                flash_swf_url: 'http://j.vzan.cc/dz/content/aliosslib/lib/lib/plupload-2.1.2/js/Moxie.swf',
                silverlight_xap_url: 'http://j.vzan.cc/dz/content/aliosslib/lib/lib/plupload-2.1.2/js/Moxie.xap',
                filters: {

                },
                multi_selection: true,

            };
            switch (type) {
                case 'img':

                    options.filters = {

                        mime_types: [
                            {
                                title: that.imgTitle,
                                extensions: "jpg,jpeg,gif,png"
                            },
                        ],
                        //max_file_size: '5mb'
                    }
                    break;
                case 'video':
                    options.filters = {

                        mime_types: [ //只允许上传图片和zip文件
                            {
                                title: that.videoTitle,
                                extensions: "mp4,m3u8",
                            },
                        ],
                        //max_file_size: '20mb'
                    }

                    break;
                case 'audio':
                    options.filters = {

                        mime_types: [ //只允许上传图片和zip文件
                            {
                                title: that.audioTitle,
                                extensions: "mp3",
                            },
                        ],
                        //max_file_size: '10mb'
                    }


                    break;
            }
            //如果还有当前类型的上传组件实例的话则先把其销毁
            if (that.uploader[type].instance) {
                console.log('destroy uploader :' + type);
                that.uploader[type].instance.destroy();
            }
            that.uploader[type].instance = new plupload.Uploader(options);

            that.uploader[type].instance.init();

            that.uploader[type].instance.bind('BeforeUpload', function (d, f) {
                var fileCount = that.uploadData[that.imgName].maxfile - that.uploadData[that.imgName].imgList.length;
                if (fileCount <=0) {
                    layer.msg("图片最大数量为" + that.uploadData[that.imgName].maxfile + "张");
                    d.stop();
                    that.initUploader(type);
                    return;
                }

                //取到文件后缀名

                var fileName = f.name;
                var ext = fileName.substring(fileName.lastIndexOf('.') + 1, fileName.length);

                //转成Mb
                var fileSize = f.size / 1024 / 1024;
                var flag = true;

                switch (type) {
                    case 'img':
                        if (!['jpg', 'png', 'gif', 'jpeg'].contains(ext)) {
                            layer.msg('请上传正确的图片格式，后缀名可为：jpg,png,gif,jpeg');
                            flag = false;
                        }
                        if (fileSize > that.maxImgSize) {
                            layer.msg('文件最大支持 ' + that.maxImgSize + 'Mb');
                            flag = false;

                        }
                        break;
                    case 'audio':
                        if (!['mp3'].contains(ext)) {
                            layer.msg('请上传正确的音频格式文件，后缀名可为：mp3');
                            flag = false;

                        }
                        if (fileSize > that.maxAudioSize) {
                            layer.msg('文件最大支持 ' + that.maxAudioSize + 'Mb');
                            flag = false;

                        }
                        break;
                    case 'video':
                        if (!['mp4', 'm3u8'].contains(ext)) {
                            layer.msg('请上传正确的视频格式文件，后缀名可为：mp4,m3u8');
                            flag = false;

                        }
                        if (fileSize > that.maxVideoSize) {
                            layer.msg('文件最大支持 ' + that.maxVideoSize + 'Mb');
                            flag = false;
                        }
                        break;
                }
                //如果在上传中遇到文件类型和文件大小不正确的错误时重新初始上传组件
                if (!flag) {
                    that.initUploader(type);
                }

                that.uploader.token.key += new Date().getTime() + '.' + ext;
                f.name = that.uploader.token.key;
                that.uploader[type].instance.setOption(that.uploader.token);
            });

            that.uploader[type].instance.bind('FilesAdded', function (uploader, files) {
                $('#start_upload_' + type).click();

            });
            that.uploader[type].instance.bind('UploadProgress', function (uploader, file) {
                $("#" + that.imgName).find(".layui-progress").show();
                element.progress(that.uploadData[that.imgName].progress, file.percent + '%');
                if (file.percent >= 100) {
                    $("#" + that.imgName).find(".layui-progress").hide();
                    element.progress(that.uploadData[that.imgName].progress, '0%');
                }
                // that.uploader[type].element.innerHTML = '上传进度 - ' + file.percent + "%";
            });
            that.uploader[type].instance.bind('FileUploaded', function (uploader, file, responseObject) {
                console.log(responseObject);
                if (responseObject.status == 200) {
                    var obj = {
                        id: 0,
                        filepath: filePath = that.uploader[type].filePath + file.name
                    }

                    that.uploadData[that.imgName].imgList.push(obj);
                }

            })
            that.uploader[type].instance.bind('UploadComplete', function (d, f, e) {
                var fileName, fileExt, filePath;
                $.each(f, function (i, v) {
                    
                })


                //上传完成后重新初始上传组件
                that.initUploader(type);


            });
            //获取上传按钮源HTML
            function getBtnHTML(type) {
                switch (type) {
                    case 'img':
                        return '<i class="layui-icon">&#xe67c;</i>' + that.imgTitle
                    case 'video':
                        return '<i class="layui-icon">&#xe67c;</i>' + that.videoTitle
                    case 'audio':
                        return '<i class="layui-icon">&#xe67c;</i>' + that.audioTitle
                    default:
                        return '';
                }
            }
        },
        uploadFile: function (type) {
            var that = this;
            $.get('/upload/InitUpload', {
                type: that.type
            }).then(function (data) {
                var filePath = data.dir + data.key;
                that.uploader[type].filePath = data.host + '/';

                var multipartParams = {
                    //因为不知道生成随机文件名的规则，所以，存储原始文件名
                    'key': filePath,
                    'policy': data.policy,
                    'OSSAccessKeyId': data.accessid,
                    'success_action_status': '200',
                    'signature': data.signature
                };
                that.uploader.token = multipartParams;
                that.uploader[type].instance.setOption({
                    'url': data.host,
                    'multipart_params': multipartParams
                });
                that.uploader[type].instance.start();
            }).fail(function (data) {
            });

        },
    },
    mounted: function () {
        //初始化上传组件
        this.initUploader('img');
        this.initUploader('video');
        this.initUploader('audio');
    }

}