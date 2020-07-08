// vue.config.js
module.exports = {
    devServer: {
        progress: false//Output running progress to console.
    },
    productionSourceMap: process.env.NODE_ENV === 'development', // 生产环境是否生成 sourceMap 文件
}