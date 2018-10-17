<style scoped lang="less">
  .wrap {
    .bg-box {
      width: 100%;
      height: 100vh;
      position: fixed;
      top: 0;
      left: 0;
      z-index: -1;
    }
    .head-box {
      width: 13.2rem;
      font-family: "sour-bold";
      color: #fff;
      position: fixed;
      top: 0.4rem;
      left: 3rem;
      .sel {
        cursor: pointer;
        font-size: 0.38rem;
      }
      .selNone {
        cursor: pointer;
        font-size: 0.26rem;
        margin-top: 0.12rem;
      }
    }
    .chat-box {
      width: 100%;
      padding: 0 3rem;
      margin-top: 1.2rem;
      .left-box {
        width: 4.22rem;
        height: 7.5rem;
        background: #fff;
        margin-right: 0.2rem;
        border-radius: 0.1rem;
        .search-box {
          width: 100%;
          height: 0.81rem;
          box-shadow: 0 0.02rem 0 rgba(0, 0, 0, 0.04);
          font-family: "sour-re";
          font-size: 0.18rem;
          >i {
            width: 0.61rem;
            cursor: pointer;
            color: #d0d0d0;
            text-align: center;
          }
          .search {
            width: 2.95rem;
            height: 0.42rem;
            background: #f6f6f6;
            border-radius: 0.05rem;
            .inputAll {
              width: 2.95rem;
              text-align: center;
            }
            .inputIm {
              width: 2.27rem;
            }
            >input {
              border: none;
              height: 0.42rem;
              background: #f6f6f6;
            }
            >input::-webkit-input-placeholder {
              color: #d0d0d0;
            }
            >input::-moz-placeholder {
              color: #d0d0d0;
            }
            >input ::-moz-placeholder {
              color: #d0d0d0;
            }
            >input::-ms-input-placeholder {
              color: #d0d0d0;
            }
            >i {
              color: #d0d0d0;
              font-size: 0.25rem;
              width: 0.68rem;
              text-align: right;
            }
            .search-pop {
              width: 2.95rem;
              padding: 0.2rem;
              background: #fff;
              position: absolute;
              top: 0.42rem;
              border-radius: 0.05rem;
              border: 0.01rem solid #eee;
              left: 0;
              font-family: "sour-re";
              z-index: 1;
              .search-add {
                height: 0.5rem;
                position: absolute;
                top: 0;
              }
              .search-show {
                left: 0;
                transition: all 0.1s ease-in-out;
                visibility: visible;
              }
              .search-hide {
                left: 1rem;
                visibility: hidden;
                transition: all .01s ease-in-out;
              }
              .search-friend {
                margin-top: 0.2rem;
                >img {
                  width: 0.3rem;
                  height: 0.3rem;
                  border-radius: 0.05rem;
                  margin-right: 0.1rem;
                }
                >p {
                  font-size: 0.16rem;
                }
                .add-friend .dzicon {
                  margin-right: 0.1rem;
                  color: #d0d0d0;
                  font-size: 0.18rem;
                }
              }
            }
          }
        }
        .friend-box {
          width: 4.22rem;
          height: 6.69rem;
          overflow: hidden;
          .friend {
            width: 4.45rem;
            height: 6.69rem;
            overflow-y: scroll;
            .friendLst {
              width: 4.22rem;
              padding: 0 0.3rem;
              height: 0.8rem;
              margin-top: 0.2rem;
              .redLine {
                width: .12rem;
                height: .12rem;
                border-radius: 50%;
                position: absolute;
                background: #ff3838;
                left: 0.8rem;
                top: 0.1rem;
              }
              >img {
                width: 0.6rem;
                height: 0.6rem;
                border-radius: 0.05rem;
                border: 1px solid #eee;
                margin-right: 0.14rem;
              }
              >div {
                font-family: "sour-re";
                height: 0.58rem;
              }
              .name {
                font-size: 0.2rem;
                color: #333;
                margin-top: 0.03rem;
              }
              .time {
                font-size: 0.16rem;
                color: #888;
              }
              .Msg {
                color: #888;
                font-size: 0.18rem;
                margin-bottom: 0.03rem;
              }
            }
          }
        }
      }
      .right-box {
        width: 8.78rem;
        height: 7.5rem;
        background: #fff;
        border-radius: 0.1rem;
        .title {
          font-family: "sour-re";
          height: 0.81rem;
          border-bottom: 1px solid #ebebeb;
          line-height: 0.81rem;
          padding: 0 0.3rem 0 3.72rem;
          >p:first-child {
            color: #666;
            font-size: 0.22rem;
          }
          >p:last-child {
            font-size: 0.18rem;
            color: #888;
            >span {
              font-size: 0.22rem;
            }
          }
        }
        .main-box {
          width: 8.78rem;
          height: 5.1rem;
          background-color: #f7faff;
          overflow: hidden;
          border-bottom: 1px solid #ebebeb;
          .main {
            width: 9.01rem;
            height: 5.1rem;
            overflow-y: scroll;
            padding: 0.5rem 0.2rem 0 0.2rem;
            .user-box {
              margin-bottom: 0.1rem;
              .userLogo {
                width: 0.6rem;
                height: 0.6rem;
                border-radius: 0.05rem;
                border: 1px solid #eee;
              }
              .msg {
                max-width: 4rem;
                line-height: 1.5;
                letter-spacing: 0.01rem;
                border-radius: 0.05rem;
                margin-top: 0.06rem;
                >i {
                  width: 0;
                  height: 0;
                  border-top: 0.04rem solid transparent;
                  border-bottom: 0.04rem solid transparent;
                  position: absolute;
                  top: 0.2rem;
                }
                >pre {
                  white-space: pre-wrap;
                  white-space: -moz-pre-wrap;
                  white-space: -pre-wrap;
                  white-space: -o-pre-wrap;
                  word-wrap: break-word;
                  word-break: break-all;
                  font-family: 'sour-re';
                  /* 支持IE和chrome，FF不支持*/
                }
                >img {
                  width: 1rem;
                  height: 1rem;
                  border-radius: 0.05rem;
                  margin-bottom: 0.3rem;
                }
              }
            }
          }
        }
        .bottom-box {
          padding: 0 0.2rem;
          .emoij {
            color: #d0d0d0;
            margin-top: 0.1rem;
            >span {
              font-size: 0.35rem;
              margin-right: 0.25rem;
            }
            >input {
              width: 0.6rem;
              position: absolute;
              left: 0.68rem;
              top: 0.03rem;
              opacity: 0;
              z-index: 1000;
            }
          }
          .msg-send {
            >textarea {
              height: 1.02rem;
              padding: 0.2rem 0.2rem 0 0.2rem;
              border: none;
              resize: none;
              overflow: hidden;
            }
            >i {
              width: 0.5rem;
              height: 0.5rem;
              background: #1f4a89;
              border-radius: 50%;
              text-align: center;
              line-height: 0.5rem;
              font-size: 0.25rem;
              color: #fff;
              margin-top: 0.1rem;
            }
          }
          .msg-tip {
            padding: 0.15rem 0.3rem;
            background: rgba(0, 0, 0, 0.4);
            border-radius: 0.05rem;
            position: absolute;
            right: 0.8rem;
            top: .5rem;
            font-size: 0.16rem;
            color: #fff;
          }
        }
      }
    }
    .fun-box {
      position: fixed;
      top: 1.2rem;
      height: 7.5rem;
      left: 2.2rem;
      .iconTip {
        position: absolute;
        width: 0.54rem;
        height: 0.54rem;
        border-radius: 50%;
        background: #fff;
        margin-top: 0.3rem;
        text-align: center;
        >input {
          width: 0.6rem;
          height: 0.8rem;
          position: absolute;
          left: -0.2rem;
          top: 0.1rem;
          opacity: 0;
        }
      }
      .icon {
        color: #d0d0d0;
        line-height: 0.54rem;
        font-size: 0.3rem;
      }
      .tip {
        font-size: .16rem;
        color: #fff;
        font-family: "sour-re";
        background: rgba(0, 0, 0, 0.3);
        padding: 0 .1rem;
        position: absolute;
        top: 50%;
        margin-top: -.13rem;
        height: 0.28rem;
        line-height: 0.28rem;
        border-radius: .05rem;
        opacity: 0;
      }
      .iconTip:hover {
        cursor: pointer;
        .tip {
          opacity: 1;
        }
      }
    }
    .func-hide {
      right: -1rem;
      opacity: 0;
      animation: hideIcon 0.1s;
    }
    .func-show {
      animation: showIcon 0.3s;
    }
    @keyframes showIcon {
      0% {
        right: -1rem
      }
      50% {
        right: -0.3rem;
      }
      90% {
        right: -0.4rem;
      }
      100% {
        right: -0.5rem;
      }
    }
    @keyframes hideIcon {
      100% {
        opacity: 0;
        right: -1rem;
      }
      90% {
        opacity: 0.3;
      }
      60% {
        opacity: 0.5;
      }
      30% {
        opacity: 0.8;
      }
      0% {
        opacity: 1;
        right: -0.5rem;
      }
    }
    .addfriend-box {
      width: 4.1rem;
      height: 1.12rem;
      background: #fff;
      border-radius: .05rem;
      position: fixed;
      bottom: 0.1rem;
      right: 0;
      padding: 0.2rem;
      img {
        width: 0.7rem;
        height: 0.7rem;
        border-radius: .05rem;
      }
      .more-box {
        height: 0.7rem;
        font-family: "sour-re";
        font-size: 0.16rem;
        margin-left: 0.1rem;
      }
      .icon2 {
        width: 0.44rem;
        height: 0.44rem;
        border-radius: 50%;
        text-align: center;
        line-height: 0.44rem;
        color: #fff;
        font-size: 0.3rem;
      }
    }
    .preView-box {
      width: 100vw;
      height: 100vh;
      background: rgba(0, 0, 0, 0.4);
      position: absolute;
      left: 0;
      top: 0;
      z-index: 9;
      >img {
        width: 6rem;
        height: 6rem;
        position: absolute;
        left: 50%;
        top: 50%;
        margin-left: -3rem;
        margin-top: -3rem;
      }
    }
  }
</style>
<template>
  <div class="wrap">
    <img class="bg-box" :src="vm.bg" />
    <div class="head-box f fc fj">
      <div class="f fc" style="cursor: pointer;" @click="changeMode" v-if="vm.chat==false">
        <span class="dzicon icon-xiayibu-copy" style="font-size:0.44rem" />
        <p style="font-size:0.44rem;margin-right:0.1rem">聊天室</p>
      </div>
      <div class="f fc" v-if="vm.chat">
        <p :class="[{'sel':vm.sel==0},{'selNone':vm.sel!=0}]" style="margin-right:0.3rem" @click="changeSel(0)">聊天</p>
        <p :class="[{'sel':vm.sel==1},{'selNone':vm.sel!=1}]" @click="changeSel(1)">好友</p>
      </div>
      <div class="f fc" style="cursor: pointer;" @click="changeMode" v-if="vm.chat">
        <p style="font-size:0.44rem;margin-right:0.1rem">社区</p>
        <span class="dzicon icon-xiayibu" style="font-size:0.44rem" />
      </div>
    </div>
    <div class="chat-box f">
      <div class="left-box f fv">
        <div class="search-box f fc">
          <i class="dzicon icon-shezhi1" style="font-size: 0.25rem;" @click="showLeft" />
          <div class="search f fc rel">
            <i class="dzicon icon-sousuo_rearch" v-if="vm.search==''" />
            <input :class="[{'inputAll':vm.search!=''},{'inputIm':vm.search==''}]" type="text" placeholder="搜索好友或添加新朋友" v-model="vm.search" @input="searchFriend" />
            <div class="search-pop rel" v-show="vm.search" :style="{height:vm.showAdd?'1rem':''}">
              <div class="f" v-if="vm.showAdd==false">搜"
                <p class="line line1">{{vm.search}}</p>"
              </div>
              <p style="margin-top:0.2rem" v-if="vm.searchLst.length==0&&vm.showAdd==false">暂无该账号</p>
              <div v-for="(item,index) in vm.searchLst">
                <div class="search-friend f fc" v-if="vm.showAdd==false" @click="chatOne(item,'search')">
                  <img :src="item.logo||'http://kaaden.orrzt.com/static/image/微信图片_20180809162909.png'" />
                  <p>{{item.name}}</p>
                  <div class="add-friend f fc f1 f-end" v-if="item.state==1" @click.stop="addFiend(index)">
                    <i class="dzicon icon-icon_tianjia" />
                    <p style="color:#666">加好友</p>
                  </div>
                </div>
              </div>
              <div class="search-add f fc" :class="[{'search-show':vm.showAdd},{'search-hide':vm.showAdd==false}]">
                <input placeholder="验证消息" v-model="vm.detail">
                <p style="color:#666" @click="sendAddFriend">发送</p>
              </div>
            </div>
          </div>
          <i class="dzicon icon-xiaoxitishi" style="font-size: 0.23rem;" />
        </div>
        <div class="friend-box">
          <div class="friend">
            <ul v-if="vm.sel==0">
              <li v-for="(item,index) in vm.chatLst" class="friendLst f fc rel" :class="{'bgf6':vm.selIndex==item.friendId}" @click="chatOne(item,'chat')">
                <span class="redLine" v-if="item.newMsg==1" />
                <img :src="item.friendImg">
                <div class="f fv fj f1">
                  <div class="f fc fj">
                    <p class="name">{{item.friendName}}</p>
                    <span class="time">{{item.time}}</span>
                  </div>
                  <p class="Msg line line1" v-if="item.msgType==0">{{item.lastMsg}}</p>
                  <p class="Msg line line1" v-if="item.msgType==1">[图片]</p>
                </div>
              </li>
            </ul>
            <ul v-if="vm.sel==1">
              <li v-for="(item,index) in vm.friendLst" v-cloak class="friendLst f fc" @click="addChat(item)">
                <img :src="item.imgUrl">
                <div class="f fv fj f1">
                  <div class="f fc fj">
                    <p class="name">{{item.name}}</p>
                    <span class="time"></span>
                  </div>
                  <p class="Msg"></p>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </div>
      <div class="right-box">
        <div class="title f fc fj">
          <p v-if="vm.chatTitle">{{vm.chatTitle}}聊天</p>
          <p class="f fc" v-if="vm.chatTitle">进入TA的主页 <span class="dzicon icon-zhuanyebaniconkuozhan_Distribu2" /></p>
        </div>
        <div class="main-box">
          <div class="main" ref="viewBox">
            <div v-for="(item,index) in vm.chatOneLst" class="user-box f" :class="{'f-end':vm.userInfo.userId==item.sId}" ref="viewBox2">
              <img :src="item.slogo" style="margin-right:0.3rem" class="userLogo" v-if="vm.userInfo.userId!=item.sId" />
              <div class="msg f fc rel" :style="{padding:item.msgType!=1?' 0.1rem 0.2rem':''}" :class="[{'bgff':vm.userInfo.userId!=item.sId&&item.msgType!=1},{'bg1f':vm.userInfo.userId==item.sId&&item.msgType!=1}]">
                <pre v-if="item.msgType==0">{{item.msg}}</pre>
                <img :src="item.msg" v-if="item.msgType==1" @click="preViewImg(item.msg)" />
                <i style="border-right: 0.08rem  solid #fff; left: -0.08rem;" v-if="vm.userInfo.userId!=item.sId&&item.msgType==1" />
                <i style="border-left: 0.08rem  solid #1F4A89; right: -0.08rem;" v-if="vm.userInfo.userId==item.sId&&item.msgType!=1" />
              </div>
              <img :src="item.slogo" style="margin-left:0.3rem" class="userLogo" v-if="vm.userInfo.userId==item.sId" />
            </div>
          </div>
        </div>
        <div class="bottom-box rel">
          <div class="emoij rel">
            <span class="dzicon icon-biaoqing" />
            <span class="dzicon icon-tupian1" />
            <input type="file" title="" accept="image/png,image/jpg,image/jpeg" @change="handleUpload" />
          </div>
          <div class="msg-send f">
            <textarea id="txtarea" class="f1" v-model="vm.msg" @focus="textChange" />
            <i class="dzicon icon-fasong1" @click="sendMsg" @keyup.enter="sendMsg" />
          </div>
          <div class="msg-tip" v-if="vm.showTip">不能发送空消息</div>
        </div>
      </div>
    </div>
    <!-- 左侧功能栏 -->
    <div class="fun-box rel">
      <div class="iconTip rel" :class="[{'func-show':vm.showLeft},{'func-hide':vm.showLeft==false}]" style="top: 0;">
        <i class="dzicon icon-bianji4 icon" />
        <span class="tip" style="left: -1rem;">编辑资料</span>
      </div>
      <div class="iconTip rel" :class="[{'func-show':vm.showLeft2},{'func-hide':vm.showLeft2==false}]" style="top: 0.9rem;">
        <input type="file" title="" accept="image/png,image/jpg,image/jpeg" @change="handleBg" />
        <i class="dzicon icon-huanfu icon" />
        <span class="tip" style="left: -1rem;">桌面壁纸</span>
      </div>
      <div class="iconTip rel" :class="[{'func-show':vm.showLeft3},{'func-hide':vm.showLeft3==false}]" style="top: 1.8rem;">
        <i class="dzicon icon-tuichujieshu icon" style="color:#E44A4A" />
        <span class="tip" style="left: -1.3rem;">退出当前账号</span>
      </div>
    </div>
    <!-- 加好友通知 -->
    <div class="addfriend-box f fc" v-if="vm.showAddPop">
      <img :src="vm.friendInfo.userInfo.logo||'http://kaaden.orrzt.com/static/image/微信图片_20180809162909.png'" />
      <div class="more-box f fv fj">
        <p style="color:#666">{{vm.friendInfo.userInfo.name||'伍六七'}}</p>
        <p style="color:#666">申请您添加为好友</p>
        <p style="color:#333" class="line line1">{{vm.friendInfo.detail||"我是你爸爸"}}</p>
      </div>
      <div class="f f1 f-end">
        <i class="dzicon icon-xuanze_Choice icon2" style="background:#00a228" @click="acceptFriend" />
        <i class="dzicon icon-icon_guanbi icon2" style="background:#e44a4a;margin-left:0.2rem" />
      </div>
    </div>
    <!-- 点击放大图片 -->
    <div class="preView-box rel" @click="closePreview" v-if="vm.showPreView" v-loading="loading" element-loading-background="rgba(0, 0, 0, 0.4)">
      <img :src="vm.preViewImg" v-if="vm.showPre">
    </div>
  </div>
</template>

<script>
  import {
    core,
    tools,
    time,
  } from "@/assets/util.js";
  import OSS from 'ali-oss';
  export default {
    name: 'index',
    data() {
      return {
        vm: {
          sel: 0,
          chat: true,
          search: "",
          selIndex: 0,
          friendLst: [],
          chatLst: [],
          userInfo: {},
          rUserInfo: {},
          chatTitle: "",
          chatOneLst: [],
          msg: '',
          showTip: false,
          searchLst: [],
          friendInfo: {},
          bg: '',
          showLeft: false,
          showLeft2: false,
          showLeft3: false,
          showAdd: false,
          showAddPop: false,
          detail: "",
          preViewImg: "",
          showPreView: false,
          showPre: false,
        },
        loading: false
      }
    },
    sockets: {
      userInfo: function(val) {
        if (val.isok) {
          this.vm.userInfo = val.userInfo
        }
      },
      addLst: function(val) {
        if (val.isok) {
          this.vm.sel = 0
          tools.showChat(val.obj[0], this)
        }
      },
      chatLst: function(val) {
        var nowDate = new Date(),
          year = nowDate.getFullYear(),
          day = nowDate.getDate(),
          _day = [],
          timearray = [];
        if (val.isok) {
          for (let i = 0, len = val.lst.length; i < len; i++) {
            timearray = val.lst[i].time.split(' ');
            _day = timearray[0].split("-")
            if (Number(_day[0]) === year) {
              if (Number(_day[2]) === day) {
                val.lst[i].time = timearray[1]
              } else {
                val.lst[i].time = Number(_day[2]) === day - 1 ? '昨天' : timearray[0];
              }
            } else {
              val.lst[i].time = timearray[0]
            }
          }
          this.vm.chatLst = val.lst
        }
      },
      Msg: function(val) {
        if (val.length == 0) {
          this.vm.chatOneLst = [...this.vm.chatOneLst, ...val]
          return;
        }
        if ((val[0].sId == this.vm.userInfo.userId && val[0].rId == this.vm.rUserInfo.friendId) || (val[0].rId == this.vm.userInfo.userId && val[0].sId == this.vm.rUserInfo.friendId)) {
          this.vm.chatOneLst = [...this.vm.chatOneLst, ...val]
        }
        if (this.$refs.viewBox2) {
          this.$refs.viewBox.scrollTop = this.$refs.viewBox2[this.$refs.viewBox2.length - 1].offsetTop
        }
      },
      msghistory: function(val) {
        this.vm.chatOneLst = val.lst
      },
      search: function(val) {
        this.vm.searchLst = val.lst
      },
      addFriend: function(val) {
        if (val.Id === this.vm.userInfo.userId) {
          this.vm.friendInfo = val
          this.vm.showAddPop = true
        }
      }
    },
    methods: {
      preViewImg(img) {
        this.vm.showPre = false
        this.vm.preViewImg = img
        this.vm.showPreView = true
        this.loading = true
        let that = this
        setTimeout(() => {
          that.loading = false
          that.vm.showPre = true
        }, 400);
      },
      closePreview() {
        this.vm.showPreView = false
      },
      //左右弹出显示
      showLeft() {
        let that = this
        that.vm.showLeft = !that.vm.showLeft
        setTimeout(() => {
          that.vm.showLeft2 = !that.vm.showLeft2
        }, 100);
        setTimeout(() => {
          that.vm.showLeft3 = !that.vm.showLeft3
        }, 200);
      },
      //显示验证消息
      addFiend(index) {
        this.vm.showAdd = true
        this.addIndex = index
      },
      //发送验证消息
      sendAddFriend() {
        let vm = {
          userInfo: this.vm.userInfo,
          friendInfo: this.vm.searchLst[this.addIndex],
          detail: this.vm.detail,
        }
        this.$socket.emit('addFriend', vm)
      },
      //同意加为好友
      acceptFriend() {
        this.vm.showAddPop = false
        let vm = {
          userInfo: this.vm.userInfo,
          friendInfo: this.vm.friendInfo
        }
        core.gossipFriend(vm).then(data => {
          console.log(data)
        })
      },
      //上传图片
      handleUpload(e) {
        let that = this
        let file = e.target.files[0]
        tools.upLoadBaseImg(file).then(data => {
          let vm = that.vm
          let g = {
            sId: vm.userInfo.userId,
            rId: vm.rUserInfo.friendId,
            msgType: 1,
            msg: data.url,
            slogo: vm.userInfo.logo,
            newMsg: 0,
          }
          tools.sendMsg(g, that)
        })
      },
      //更换壁纸
      handleBg(e) {
        let that = this
        let file = e.target.files[0]
        tools.upLoadBaseImg(file).then(data => {
          that.vm.bg = data.res.requestUrls[0]
          let vm = {
            state: 1,
            bg: data.res.requestUrls[0],
            userId: that.vm.userInfo.userId,
          }
          core.gossipUpdateUserInfo(vm).then(res => {
            if (res.isok) {
              that.vm.bg = res.lst.bg
            }
          })
        })　　　　　　　　　
      },
      //获取焦点
      textChange() {
        let that = this
        document.getElementById('txtarea').addEventListener('keydown', function(e) {
          if (e.shiftKey && e.keyCode == 13) {
            document.getElementById('txtarea').value.replace(/\r\n/g, '<br/>').replace(/\n/g, '<br/>').replace(/\s/g, ' ')
          }
          if (e.shiftKey == false && e.keyCode == 13) {
            if (that.vm.msg === '') {
              that.vm.showTip = true
              clearTimeout(that.timeout)
              that.timeout = setTimeout(() => {
                that.vm.showTip = false
              }, 1000);
            }
            e.preventDefault();
            return;
          }
        })
      },
      //选择聊天
      chatOne(item, type) {
        if (type == 'search') {
          if (item.state != 2) {
            return;
          }
          item.friendId = item.userId
          item.friendName = item.name
          item.friendImg = item.logo
        }
        tools.showChat(item, this)
      },
      //发送消息
      sendMsg() {
        if (this.vm.msg === '') {
          return;
        }
        let vm = this.vm
        let g = {
          sId: vm.userInfo.userId,
          rId: vm.rUserInfo.friendId,
          msgType: 0,
          msg: vm.msg,
          slogo: vm.userInfo.logo,
          newMsg: 0,
        }
        tools.sendMsg(g, this)
        document.getElementById("txtarea").value = "";
      },
      //添加聊天
      addChat(item) {
        let vm = {
          userId: this.vm.userInfo.userId,
          friendId: item.id,
          friendImg: item.imgUrl,
          friendName: item.name,
          lastMsg: "",
          msgType: 3,
        }
        this.vm.selIndex = item.id
        this.vm.chatTitle = item.name
        this.$socket.emit('addChat', vm)
        this.$socket.emit('chatLst', this.vm.userInfo.userId)
      },
      changeSel(index) {
        this.vm.sel = Number(index)
        if (Number(index) == 1) {
          core.gossipConcactLst(this.vm.userInfo.userId).then(data => {
            if (data.isok) {
              this.vm.friendLst = data.list
            }
          })
        }
      },
      changeMode() {
        this.vm.chat = !this.vm.chat
      },
      //搜索好友
      searchFriend() {
        this.vm.showAdd = false
        this.vm.search = this.vm.search.replace(/[\W]/g, '')
        let vm = {
          userId: this.vm.userInfo.userId || '311',
          search: this.vm.search,
        }
        this.$socket.emit('search', vm)
      }
    },
    created() {
      let that = this
      document.onkeydown = function(e) {
        var key = window.event.keyCode;
        if (window.event.shiftKey == false && key === 13) {
          that.sendMsg()
        }
      }
    },
    mounted() {
      let that = this
      tools.fontSize()
      let userId = this.$route.query.userId
      core.gossipFind(2, userId, '').then(data => {
        if (data.bg) {
          that.vm.bg = data.bg
        } else {
          that.vm.bg = "https://kaaden.oss-cn-hangzhou.aliyuncs.com/font/%E5%BE%AE%E4%BF%A1%E5%9B%BE%E7%89%87_20180810163222.jpg"
        }
        if (data.isok) {
          that.$socket.emit('userInfo', data.userId) //在这里触发connect事件
          that.$socket.emit('chatLst', data.userId)
          that.setDown = setInterval(() => {
            that.$socket.emit('chatLst', data.userId)
          }, 5000)
        }
      })
    },
    updated() {
      if (this.$refs.viewBox2 && this.$refs.viewBox2.length) {
        this.$refs.viewBox.scrollTop = this.$refs.viewBox2[this.$refs.viewBox2.length - 1].offsetTop
      }
    },
    destroyed() {
      clearInterval(this.setDown)
    }
  }
</script>


