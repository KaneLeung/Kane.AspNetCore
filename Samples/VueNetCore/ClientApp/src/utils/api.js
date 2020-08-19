import axios from "axios";
import Storage from "./storage.js";
import router from '@/router'; //如果JwtToken失效不重定向，可以不加入
const host =
  process && process.env && process.env.NODE_ENV === "development" ?
  "http://localhost:5000" :
  "";
let refreshFlag = false; // 是否正在刷新的标记
let requestList = []; // 重试队列，每一项将是一个待执行的函数形式
const http = axios.create(); //创建Axios实例 
http.defaults.timeout = 10000; //设置超时，单位毫秒
http.defaults.baseURL = host; // 默认地址
//http.defaults.headers.post['Content-Type'] = 'application/json;charset=UTF-8';//全局设置，默认为【application/x-www-form-urlencoded】
//http.setToken = (token) => {
//  http.defaults.headers.Authorization = token;
//}
http.defaults.transformRequest = data => { //整理数据
  //data = Qs.stringify(data);
  data = JSON.stringify(data);
  return data;
};
//请求拦截器配置
const httpConf = (config) => {
  //config.headers["Content-Type"] = "application/json;charset=UTF-8";
  const tokenInfo = Storage.localGet("tokenInfo")
  if (tokenInfo && tokenInfo.Access_Token) {
    config.headers["Authorization"] = `Bearer ${tokenInfo.Access_Token}`;
  }
  //可启动加载动画
  return config;
}
//HttpRequest请求拦截器
http.interceptors.request.use(httpConf, error => {
  return Promise.reject(error.response);
});
//HttpResponse响应拦截器
http.interceptors.response.use(async (response) => {
  let data = {};
  if (response && response.data) {
    const code = Number(response.data.code);
    data = response.data;
    if (response.status == 200 && code != 401) {
      data = response.data;
    } else if (response.status == 200 && code == 401) {
      if (!refreshFlag) {
        refreshFlag = true;
        try {
          const token = await refreshToken();
          if (token) {
            requestList.forEach(item => item()); // 已经刷新了token，将所有队列中的请求进行重试
            requestList = [];
            return http(httpConf(response.config));
          } else {
            routerRedirect({
              redirect: router.currentRoute.fullPath
            });
          }
        } catch (error) { //刷新时候直接判断token 不用判断code
          console.error('refreshtoken error =>', error);
          routerRedirect({
            redirect: router.currentRoute.fullPath
          });
        } finally {
          refreshFlag = false;
        }
      } else {
        return new Promise((resolve) => { // 正在刷新token，将返回一个未执行resolve的promise
          requestList.push(() => { // 将resolve放进队列，用一个函数形式来保存，等token刷新后直接执行
            resolve(http(httpConf(response.config)));
          });
        });
      }
    } else {
      console.log('网络连接出错!请稍后刷新重试!');
      //可以加入弹窗提示
    }
  }
  //if (loading) loading.close();//关闭加载动画
  return data;
}, (error) => {
  console.log(`object`, error);
  //if (loading) loading.close();//关闭加载动画
  //可以加入弹窗提示
  return Promise.reject(error.response.data);
});
const refreshToken = async () => { //根据RefreshToken获取新的Jwt
  try {
    const tokenInfo = Storage.localGet("tokenInfo")
    if (tokenInfo && tokenInfo.Refresh_Token) { //没有Token信息，直接返回null
      const {
        data
      } = await axios.post(`Jwt/Refresh`, { //用刷新Token获取新的的Token
        token: tokenInfo.Refresh_Token
      });
      if (data.code == 0 && data.data.Access_Token && data.data.Refresh_Token) {
        Storage.localSet("tokenInfo", data.data); //更新Token信息
        return data.data.Access_Token;
      } else Storage.localRemove("tokenInfo"); //移除Token信息
    }
    return null;
  } catch (error) {
    console.log(error);
  }
};
const routerRedirect = ({ //Token校验失败，重定向到登录页
  path = '/login',
  redirect
}) => {
  console.log('重定向到登录页', path, redirect)
  //Message.warning(`身份过期，请重新登录!`);
  // if (router.currentRoute.path != '/login') {
  //     setTimeout(() => {
  //         router.replace({ path, query: { redirect } });
  //     }, 1200);
  // }
}
export default { //公开的成员
  Host: host,
  Common: {
    GetToken: () => http.get("Jwt"), //获取Token
    CheckToken: () => http.get("Jwt/Check"), //检验Token
  }
};
