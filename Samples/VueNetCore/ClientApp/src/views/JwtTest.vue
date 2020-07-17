<template>
  <div class="jwt">
    <div>
      <Button class="btn" @click="getToken">获取Token</Button>
      <Button class="btn" @click="checkToken">校验Token {{tokenState}}</Button>
      <Button class="btn" @click="removeToken">移除Token</Button>
    </div>
    <div style="margin-top:20px">
      <textarea v-model="token" wrap="soft" autocomplete="off" spellcheck="false" placeholder="这个是JwtToken" rows="8"
        class="token-input"></textarea>
    </div>
    <div style="margin-top:20px">
      刷新的Token
    </div>
    <div style="margin-top:20px">
      <input class="refresh-input" v-model="refreshToken">
    </div>
    <div style="margin-top:20px">
      过期时间
    </div>
    <div style="margin-top:20px">
      <input class="refresh-input" v-model="expireTime">
    </div>
  </div>
</template>

<script>
  import api from "../utils/api";
  import storage from "../utils/storage.js"
  export default {
    data() {
      return {
        token: '',//JwtToken
        refreshToken: '',//刷新的Token
        tokenState: '',//Token状态
        expireTime:''//过期时间
      }
    },
    methods: {
      getToken() {
        api.Common.GetToken().then(res => {
          if (res.code == 0) {
            this.token = res.data.Access_Token;
            this.refreshToken = res.data.Refresh_Token;
            this.expireTime = new Date(res.data.Expires * 1000);
            storage.localSet("tokenInfo", res.data);
          }
        });
      },
      checkToken() {
        api.Common.CheckToken().then(res => {
          if (res.code === 0) {
            this.tokenState = '成功';
            var tokenInfo = storage.localGet("tokenInfo");
            this.token = tokenInfo.Access_Token;
            this.refreshToken = tokenInfo.Refresh_Token;
            this.expireTime = new Date(tokenInfo.Expires * 1000);
          } else this.tokenState = '失败'
        });
      },
      removeToken(){
        storage.localRemove("tokenInfo");
        this.token ='';
        this.refreshToken ='';
        this.tokenState ='';
        this.expireTime= '';
      }
    }
  };
</script>
<style scoped>
  .btn {
    color: #fff;
    background-color: #2d8cf0;
    border-color: #2d8cf0;
    display: inline-block;
    font-weight: 400;
    text-align: center;
    vertical-align: middle;
    -ms-touch-action: manipulation;
    touch-action: manipulation;
    cursor: pointer;
    background-image: none;
    border: 1px solid transparent;
    white-space: nowrap;
    -webkit-user-select: none;
    -moz-user-select: none;
    -ms-user-select: none;
    user-select: none;
    height: 32px;
    padding: 0 15px;
    font-size: 14px;
    border-radius: 4px;
    transition: color 0.2s linear, background-color 0.2s linear,
      border 0.2s linear, box-shadow 0.2s linear;
    margin: 0 10px;
  }

  .token-input {
    max-width: 40%;
    height: auto;
    min-height: 32px;
    vertical-align: bottom;
    font-size: 14px;
    display: inline-block;
    width: 40%;
    line-height: 1.5;
    padding: 4px 7px;
    border: 1px solid #dcdee2;
    border-radius: 4px;
    color: #515a6e;
    background-color: #fff;
    background-image: none;
    position: relative;
    cursor: text;
  }

  .refresh-input {
    max-width: 40%;
    width: 40%;
    min-height: 32px;
    font-size: 14px;
  }
</style>
