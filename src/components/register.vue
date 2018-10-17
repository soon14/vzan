<style scoped lang="less">
     :root .wrap {
        filter: none;
    }
    .wrap {
        line-height: 1.5;
        width: 100%;
        height: 100vh;
        background: -webkit-linear-gradient(135deg, #245193, #3d74c4);
        filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#245193', endColorstr='#245193', GradientType=0);
        background: -ms-linear-gradient(135deg, #245193, #245193);
        .reg-box {
            width: 5.82rem;
            height: 8.21rem;
            background: #fff;
            position: absolute;
            top: 50%;
            left: 50%;
            margin-top: -4.105rem;
            margin-left: -2.91rem;
            border-radius: 0.2rem;
            .top {
                margin-top: 0.45rem;
                margin-bottom: 0.3rem;
                >p {
                    font-family: "sour-bold";
                    font-size: 0.28rem;
                }
                .upload-img {
                    margin-top: 0.3rem;
                    >img {
                        width: 1.1rem;
                        height: 1.1rem;
                        border-radius: 0.05rem;
                    }
                    >span {
                        font-family: "sour-light";
                        font-size: 0.2rem;
                        color: #666;
                        margin-top: 0.2rem;
                    }
                    >input {
                        position: absolute;
                        width: 1.1rem;
                        height: 1.1rem;
                        line-height: 1.1rem;
                        opacity: 0;
                        z-index: 100;
                    }
                }
            }
            .main {
                margin-bottom: 0.2rem;
                padding-left: 0.61rem;
                >p {
                    font-size: 0.2rem;
                    font-family: "sour-light";
                    color: #666;
                    margin-bottom: 0.1rem;
                }
                .userName {
                    width: 4.09rem;
                    height: 0.45rem;
                    border: 1px solid #d9d8d9;
                    border-radius: 0.05rem;
                }
                .sex {
                    font-family: "sour-re";
                    margin-right: 0.38rem;
                    .dzicon {
                        font-size: 0.23rem;
                        margin-right: 0.1rem;
                    }
                    >p {
                        font-size: 0.2rem;
                    }
                }
                .time {
                    width: 1.04rem;
                    height: 0.48rem;
                    line-height: 0.48rem;
                    padding-left: 0.2rem;
                    border: 1px solid #d9d8d9;
                    margin-right: 0.2rem;
                    border-radius: 0.05rem;
                    cursor: pointer;
                    .icon {
                        position: absolute;
                        top: 0;
                        right: 0.1rem;
                        font-size: 0.12rem;
                    }
                    .sel-box {
                        width: 1.04rem;
                        background: #fff;
                        overflow-y: auto;
                        border: 1px solid #b8c4ce;
                        border-top: none;
                        background-color: #fff;
                        position: absolute;
                        left: -1px;
                        text-align: center;
                    }
                    .year-show {
                        height: 2rem;
                        opacity: 1;
                        visibility: visible;
                        transition: all 0.2s ease-in-out;
                    }
                    .year-hide {
                        height: 0;
                        opacity: 0;
                        visibility: hidden;
                        transition: all 0.2s ease-in-out;
                    }
                }
            }
            .btn {
                width: 3.9rem;
                height: 0.6rem;
                line-height: 0.6rem;
                text-align: center;
                background: #1f4a89;
                color: #fff;
                font-family: "sour-re";
                font-size: 0.22rem;
                margin: 0.6rem auto 0.2rem auto;
                border-radius: 0.05rem;
                cursor: pointer;
            }
            .tip {
                font-size: 0.18rem;
                font-family: "sour-light";
                text-align: center;
                color: #999;
            }
        }
    }
</style>

<template>
    <div class="wrap">
        <div class="reg-box rv">
            <div class="top f fc fv">
                <p>填写基本资料</p>
                <div class="upload-img f fv fc rel">
                    <el-progress type="circle" :percentage="num" v-if="num!=0&&fullscreenLoading"></el-progress>
                    <input type="file" title="" accept="image/png,image/jpg,image/jpeg" @change="handleUpload" />
                    <img src="http://kaaden.orrzt.com/static/image/微信图片_20180809162909.png" v-if="vm.logo==''&&fullscreenLoading==false" />
                    <img :src="vm.logo" v-if="vm.logo&&fullscreenLoading==false" style="border:1px solid #a1a2a3" />
                    <span>—— 上传头像 ——</span>
                </div>
            </div>
            <div class="main">
                <p>用户名<span style="color:#ff3838">*</span></p>
                <input v-model="vm.name" class="userName" style="padding-left:0.2rem">
            </div>
            <div class="main">
                <p>性别<span style="color:#ff3838">*</span></p>
                <div class="f fc">
                    <div class="sex f fc">
                        <span class="dzicon icon-Unselected_weixuanze" v-if="vm.sex" @click="changeSex" />
                        <span class="dzicon icon-Choice_xuanze" v-if="vm.sex==false" @click="changeSex" :class="{'c1f':vm.sex==false}" />
                        <p>男</p>
                    </div>
                    <div class="sex f fc">
                        <span class="dzicon icon-Unselected_weixuanze" v-if="vm.sex==false" @click="changeSex" />
                        <span class="dzicon icon-Choice_xuanze" v-if="vm.sex" @click="changeSex" :class="{'c1f':vm.sex}" />
                        <p>女</p>
                    </div>
                </div>
            </div>
            <div class="main">
                <p>出生日期<span style="color:#ff3838">*</span></p>
                <div class="f fc">
                    <div class="time rel" @click="pickYear">
                        <p>{{vm.year||'年'}}</p>
                        <span class="dzicon icon-xiala1 icon" />
                        <ol class="sel-box" :class="[{'year-show':vm.yearShow},{'year-hide':vm.yearShow==false}]">
                            <li v-for="item in vm.yearArray" v-cloak style="cursor: pointer;" @click="selYear(item)" :class="{'bg1f':vm.year==item}">{{item}}</li>
                        </ol>
                    </div>
                    <div class="time rel" @click="pickMonth">
                        <p>{{vm.month||'月'}}</p>
                        <span class="dzicon icon-xiala1 icon" />
                        <ol class="sel-box" :class="[{'year-show':vm.monthShow},{'year-hide':vm.monthShow==false}]">
                            <li v-for="item in vm.monthArray" v-cloak style="cursor: pointer;" @click="selMonth(item)" :class="{'bg1f':vm.month==item}">{{item}}</li>
                        </ol>
                    </div>
                    <div class="time rel" @click="pickDay">
                        <p>{{vm.day||'日'}}</p>
                        <span class="dzicon icon-xiala1 icon" />
                        <ol class="sel-box" :class="[{'year-show':vm.dayShow},{'year-hide':vm.dayShow==false}]">
                            <li v-for="item in vm.dayArray" v-cloak style="cursor: pointer;" @click="selDay(item)" :class="{'bg1f':vm.day==item}">{{item}}</li>
                        </ol>
                    </div>
                </div>
            </div>
            <div class="btn" @click="regiter">进入Gossip</div>
            <div class="tip">后续资料如需补充，可进入个人设置填写</div>
        </div>
    </div>
</template>

<script>
    import {
        core,
        tools,
        time
    } from "@/assets/util.js";
    export default {
        name: "register",
        data() {
            return {
                vm: {
                    id: "",
                    logo: "",
                    name: "",
                    sex: false,
                    sexname: '男',
                    year: "",
                    yearArray: [],
                    yearShow: false,
                    month: "",
                    monthArray: [],
                    monthShow: false,
                    day: "",
                    dayArray: [],
                    dayShow: false,
                },
                fullscreenLoading: false,
                num: 0,
            };
        },
        methods: {
            handleUpload(e) {
                let that = this
                let file = e.target.files[0]
                that.fullscreenLoading = true
                that.num = 40
                tools.upLoadBaseImg(file).then(data => {
                    that.num = 80
                    that.vm.logo = data.url
                    that.num = 100
                    setTimeout(() => {
                        that.fullscreenLoading = false
                    }, 1000);
                })
            },
            changeSex() {
                this.vm.sex = !this.vm.sex;
                this.vm.sex ? this.vm.sexname = '女' : this.vm.sexname = '男';
            },
            //年
            pickYear() {
                this.vm.yearShow = !this.vm.yearShow
                if (this.vm.yearShow) {
                    this.vm.monthShow = false
                    this.vm.dayShow = false
                    this.vm.yearArray = time.year()
                }
            },
            selYear(num) {
                this.vm.year = num
            },
            //月
            pickMonth() {
                this.vm.monthShow = !this.vm.monthShow
                if (this.vm.monthShow) {
                    this.vm.yearShow = false
                    this.vm.dayShow = false
                    this.vm.monthArray = time.timeSpan(12)
                }
            },
            selMonth(num) {
                this.vm.month = num
            },
            //日 
            pickDay() {
                this.vm.dayShow = !this.vm.dayShow
                if (this.vm.dayShow) {
                    this.vm.yearShow = false
                    this.vm.monthShow = false
                    this.vm.dayArray = time.timeSpan(31)
                }
            },
            selDay(num) {
                this.vm.day = num
            },
            regiter() {
                let vm = {
                    id: this.vm.id,
                    logo: this.vm.logo,
                    name: this.vm.name,
                    birth: this.vm.year + "-" + this.vm.month + "-" + this.vm.day,
                    sex: this.vm.sexname,
                }
                if (vm.logo == '' || vm.name == '' || vm.birth == '' || vm.sex == '') {
                    return;
                }
                core.gossipUpdate(vm).then(data => {
                    if (data.isok) {
                        tools.goNewPage({
                            path: "/Gossip/index",
                            query: {
                                userId: data.userId
                            }
                        }, this)
                    } else {
                        tools.showError(msg, this)
                    }
                })
            }
        },
        mounted(options) {
            tools.fontSize();
            let account = this.$route.query.account
            core.gossipFind(1, '', account).then(data => {
                if (data.isok) {
                    this.vm.id = data.id
                } else {
                    tools.goBack(this)
                }
            })
        },
    };
</script>


