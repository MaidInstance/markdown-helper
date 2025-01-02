# MarkdownHelper

## 简介
这个小程序会帮你把`.md`文件图片里面的提取出来，让你在做一些`.md`文件同步时，文件内的图片使用的绝对地址不会使你感到糟糕。

## 行为过程
- 小程序运行时，默认将运行目录下的所有`.md`文件内的图片提取出来，放到程序运行目录下的`./Photos/`文件夹内（如不存在会自动创建），同时也会在`Photos`问价夹内创建与`.md`文件同名的文件夹，每个`.md`文件对应一个单独的文件夹，所以不用担心会产生图片冲突。
- 你可以通过拖拽，将你需要处理的`.md`文件选中拖到程序上让程序执行（并非打开程序再拖拽，而是将文件拖拽到程序上），让你选择的那部分Markdown文件被程序处理。  

## 另外
- 程序可能存在一些问题，欢迎提交PR。
- 项目最初在`Gitee`发布，会同步到`GitCode`，再推送到`GitHub`。后两个仓库会有延迟。

### 项目地址
- [Gitee](https://gitee.com/MaidInstance/markdown-helper/)  
- [GitCode](https://gitcode.com/MaidInstance/markdown-helper/)  
- [GitHub](https://github.com/MaidInstance/markdown-helper/)  