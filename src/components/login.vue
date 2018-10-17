<style scoped lang="less">
     :root .wrap {
        filter: none;
    }
    .wrap {
        line-height: 1.5;
        width: 100%;
        height: 100vh;
        background: -webkit-linear-gradient(135deg, #245193, #245193);
        filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#000000', endColorstr='#ffffff', GradientType=0);
        background: -ms-linear-gradient(135deg, #245193, #245193);
        .wrap-box {
            position: absolute;
            top: 50%;
            left: 50%;
            width: 9rem;
            height: 5.7rem;
            margin-left: -4.5rem;
            margin-top: -2.85rem;
            .left-box {
                color: #fff;
                font-size: 0.2rem;
                padding: 0 0.3rem;
                font-family: "sour-re";
                width: 3.9rem;
                background: #1F4A89;
                border-top-left-radius: 0.2rem;
                border-bottom-left-radius: 0.2rem;
                >p:nth-child(2) {
                    font-family: "duSha";
                    font-size: 0.52rem;
                    margin-bottom: 0.1rem;
                }
                >div img {
                    width: 2.49rem;
                    height: 2.04rem;
                    margin-top: 0.81rem;
                }
            }
            .right-box {
                padding: 0 0.6rem;
                width: 5.1rem;
                background: #fff;
                border-top-right-radius: 0.2rem;
                border-bottom-right-radius: 0.2rem;
                .nav-head {
                    font-family: "sour-bold";
                    margin-bottom: 0.4rem;
                    >p {
                        cursor: pointer;
                    }
                    .selNone {
                        color: #A1A2A3;
                        font-size: 0.3rem;
                        margin-top: 0.06rem;
                    }
                    .sel {
                        color: #333;
                        font-size: 0.38rem;
                        font-weight: bold;
                    }
                }
                .main {
                    font-family: "sour-light";
                    >span {
                        color: #A1A2A3;
                        font-size: 0.2rem;
                        margin-bottom: 0.3rem;
                    }
                    >input {
                        border: none;
                        border-bottom: 0.01rem solid #E1E1E1;
                    }
                    .main-tip {
                        position: absolute;
                        right: 0;
                        bottom: 10%;
                        >p {
                            margin-left: 0.05rem;
                        }
                    }
                }
                .left-main {
                    font-family: "sour-normal";
                    font-size: 0.18rem;
                    color: #666;
                    .dzicon {
                        margin-right: 0.1rem;
                        font-size: 0.24rem;
                    }
                    .icon-Choice_xuanze {
                        color: #1F4A89;
                    }
                }
                .right-main {
                    font-family: "sour-normal";
                    font-size: 0.18rem;
                    color: #A1A2A3;
                    cursor: pointer;
                }
                .btn {
                    height: 0.6rem;
                    line-height: 0.6rem;
                    color: #fff;
                    font-family: "sour-re";
                    font-size: 0.22rem;
                    text-align: center;
                    border-radius: 0.05rem;
                    cursor: pointer;
                }
                .login {
                    margin-top: 0.85rem;
                }
                .register {
                    margin-top: 0.42rem;
                }
                .noUse {
                    background: #A1A2A3;
                }
                .use {
                    background: #1F4A89;
                }
            }
        }
    }
</style>

<template>
    <div class="wrap">
        <div class="wrap-box f rv">
            <div class="left-box f fv fc-h">
                <p>welcome to</p>
                <p>Gossip</p>
                <p>欢迎来到Gossip，您可以聊工作、聊私事，反正，畅所欲言。</p>
                <div class="f fc-h">
                    <img src="http://kaaden.orrzt.com/static/image/微信图片_20180808165706.png">
                </div>
            </div>
            <div class="right-box f fv fc-h">
                <div class="nav-head f fc">
                    <p :class="[{'sel':vm.sel==0},{'selNone':vm.sel!=0}]" @click="changeSel(0)">登录</p>
                    <p :class="[{'sel':vm.sel==1},{'selNone':vm.sel!=1}]" style="margin-left:0.3rem" @click="changeSel(1)">注册</p>
                </div>
                <div v-if="vm.sel==0">
                    <div class="main f fv" style="margin-bottom:0.3rem">
                        <span>账号</span>
                        <input v-model="vm.account" ref="account" @input="setPass">
                    </div>
                    <div class="main f fv rel">
                        <span>密码</span>
                        <input v-model="vm.password" type="password" ref="password" @input="setPass">
                        <div class="main-tip f fc" style="color:#ff3838;" v-if="vm.showError">
                            <span class="dzicon icon-shanchu4" />
                            <p>密码错误</p>
                        </div>
                    </div>
                    <div class="f fc fj" style="margin-top:0.2rem;">
                        <div class="left-main f fc">
                            <span class="dzicon icon-Unselected_weixuanze" v-if="vm.remeSel==false" @click="changeRe" />
                            <span class="dzicon icon-Choice_xuanze" v-if="vm.remeSel" @click="changeRe" />
                            <span>记住账号</span>
                        </div>
                        <div class="right-main">忘记密码？</div>
                    </div>
                    <div class="login btn" @click="login" @@keyup.enter="login" :class="[{'use':useLogin==true},{'noUse':useLogin==false}]">登录</div>
                </div>
                <div v-if="vm.sel==1">
                    <div class="main f fv rel" style="margin-bottom:0.3rem">
                        <span>账号</span>
                        <input v-model="vm.reaccount" ref="reacc" @blur="checkAccout" @input="getAccount">
                        <div class="main-tip f fc" style="color:#ff3838;" v-if="vm.reaccount&&vm_check.isok==false">
                            <span class="dzicon icon-shanchu4" />
                            <p>该账号已存在</p>
                        </div>
                        <div class="main-tip f fc" style="color:#63b504" v-if="vm.reaccount&&vm_check.isok">
                            <span class="dzicon icon-zhifuchenggong" />
                        </div>
                    </div>
                    <div class="main f fv" style="margin-bottom:0.3rem">
                        <span>密码</span>
                        <input type="password" v-model="vm.repassword" ref="repass">
                    </div>
                    <div class="main f fv rel">
                        <span>电子邮箱（必填，方便您找回密码）</span>
                        <input v-model="vm.email" ref="email" type="email" @input="setEmail">
                        <div class="main-tip f fc" style="color:#ff3838;" v-if="vm.email&&vm.emailError">
                            <span class="dzicon icon-shanchu4" />
                            <p>请输入正确的邮箱</p>
                        </div>
                    </div>
                    <div class="register btn" @click="register" @keyup.enter="register" :class="[{'use':useRegister==true},{'noUse':useRegister==false}]">注册</div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import {
        tools,
        core,
    } from "@/assets/util.js";
    export default {
        name: 'login',
        data() {
            return {
                vm: {
                    sel: 0,
                    account: "",
                    password: "",
                    reaccount: "",
                    repassword: "",
                    email: "",
                    remeSel: false,
                    showError: false,
                    emailError: false
                },
                useLogin: false,
                useRegister: false,
                vm_check: {},
                email: "",
                id: 0
            }
        },
        watch: {
            /**
             * @param {监听登陆以及注册}
             * @param {deep 是否开启深度监听}
             */
            vm: {
                handler: function(val, oldval) {
                    var account = val.account
                    var password = val.password
                    var reaccount = val.reaccount
                    var repass = val.repassword
                    var email = val.email
                    // 登陆
                    account && password ? this.useLogin = true : this.useLogin = false
                    // 注册
                    reaccount && repass && email ? this.useRegister = true : this.useRegister = false
                    //账号只能输入英文或数字
                    this.vm.reaccount = reaccount.replace(/[\W]/g, '');
                    this.vm.account = account.replace(/[\W]/g, '');
                },
                deep: true
            }
        },
        methods: {
            // 导航栏
            changeSel(index) {
                this.vm.sel = index
                Object.assign(this.vm, {
                    password: "",
                    reaccount: "",
                    repassword: "",
                    email: "",
                })
            },
            //是否记住密码
            changeRe() {
                this.vm.remeSel = !this.vm.remeSel
                this.vm.remeSel === false ? localStorage.clear() : ""
            },
            setPass() {
                this.vm.showError = false
            },
            setEmail() {
                this.vm.emailError = false
            },
            login() {
                let vm = this.vm
                if (vm.account === '' || vm.password === '') {
                    return
                }
                core.gossipLogin(vm).then(data => {
                    if (vm.remeSel) {
                        localStorage.setItem('account', vm.account)
                    }
                    if (data.isok) {
                        tools.goNewPage({path:"/Gossip/index",query:{userId:data.userId}}, this)
                        Object.assign(this.vm, {
                            account: "",
                            password: "",
                            reaccount: "",
                            repassword: "",
                            email: "",
                        })
                    } else {
                        this.vm.showError = true
                    }
                })
            },
            register() {
                let vm = this.vm
                let reg = /^[a-zA-Z0-9_-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$/;
                if (vm.reaccount == '' || vm.repassword == '' || vm.email == '') {
                    return;
                }
                if (!reg.test(vm.email)) {
                    this.vm.emailError = true
                    return;
                }
                core.gossipRegister(vm).then(data => {
                    if (data.isok) {
                        tools.goNewPage({path:"/Gossip/register",query:{account:data.account}}, this)
                        Object.assign(this.vm, {
                            account: "",
                            password: "",
                            reaccount: "",
                            repassword: "",
                            email: "",
                        })
                    } else {
                        tools.showError(data.msg, this)
                    }
                })
            },
            //检查注册是否存在
            checkAccout() {
                core.gossipCheck(this.vm.reaccount).then(data => {
                    this.vm_check = data
                })
            },
            getAccount() {
                this.vm_check = {}
            },
            sendMail() {
                let email = this.email
                core.gossipEmail(email)
            }
        },
        created: function() {
            let that = this
            let account = localStorage.getItem('account') || "";
            if (account) {
                that.vm.account = account
                that.vm.remeSel = true
            } else {
                that.vm.account = account
                that.vm.remeSel = false
            }
            document.onkeydown = function(e) {
                var key = window.event.keyCode;
                if (key === 13) {
                    that.vm.sel == 0 ? that.login() : that.register()
                }
            }
        },
        mounted() {
        
            tools.fontSize()
            let browser = tools.browserJudg()
            browser === 'IE' ? window.alert("请使用IE10以上或谷歌浏览器") : ""
        },
    }
</script>


