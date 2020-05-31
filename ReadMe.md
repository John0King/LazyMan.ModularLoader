LayMan.ModularLoader
=========================

一个方面的插件加载器，适用于.net core , 支持插件程序集隔离，和插件依赖， 支持加载原生库

> 该库目前处于非常早期的状态，请勿用于生产环境

## 项目结束

### 1. LazyMan.ModularLoader

基础的插件加载器，这里面所有的类都是抽象的，要想使用需要里面里面`HostLoader`来 自己封装自己的加载器


### 2. LazyMan.ModularLoader.AspNetCore

asp.net core 的 子应用程序（SubApp） 加载器，目的是将任意的现有基于asp.net core的web应用，以完全隔离的方式
加载当前进程，并且映射到子路径上（注意，静态html文件和js文件路径可能出现错误）


## 捐赠 Donate

手头富裕的可以支持我将此项目继续下去i😜， 一毛不嫌少， 一万不嫌多 哈哈 

![wechat](./docs/assets/donate_wechat.png)
![alipay](./docs/assets/donate_alipay.jpg)