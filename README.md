# css sprite

        css雪碧图简单制作工具

        可以通过图片，直接生成sprite文件，并且生成代码

        可以通过鼠标点击调整图片位置

		可以添加单张图片，以及删除单张图片

		可以保存为.sprite文件，以后好维护

# CSDN下载地址(多支持下吧)

http://download.csdn.net/detail/wx247919365/8741243

# V4.3最新版本介绍

http://www.cnblogs.com/wang4517/p/4529741.html

---- 

#### branch 2016-03-25 修正说明

1. 修改FormMain.cs 中的LoadImages()方法，将原方法的根据图片宽度进行排序，替换为根据文件名进行排序，只支持简单的数字文件名(与图片扩展名无关)排序，如:`3.png`,`1.png`,`2.png` ，是从小到大的排序,在程序中，就会以`1.png`,`2.png`,`3.png`的顺序进行排序
2. 修改 FormMain.cs 中的GetSassCss() 方法，因之前源程序生成的sass background-position是三位数，修改成和css一样的background-position.

---

#### branch 2016-06-19 更新
1. 目前的版本支持根据图片的宽度和高度，文件名(数字) 三种排序依据和排序顺序 `从小到大(ASC)`和`从大到小(DESC)`两个选择。其中`文件名(数字)`:目前只支持导入的图片的文件名都是数字，如:`1`.jpg,`1234`.jpg或者`1`.png,`1234`.png的形式。
2. 根据 XXX 排序，这个下拉框会动态改变,当你`选择多幅图片`的文件名(`文件名是文件名称不包含文件格式的名称，如:一个图片文件是:1.jpg,那么这个文件的文件名是:1`)全为数字时，`文件名(数字)`是可以选择的。因为在添加图片时，会判断的图片的文件名是否是数字字符串


exe文件下载链接:[360云盘下载链接](https://yunpan.cn/cR253qBsq8fQy) （提取码：56de）
##### 说明
* 运行环境依赖 .Net Framework 4.0 或者大于.Net Framework 4.0
* exe文件已在Windows 7 64位测试可运行


##### 目前未能处理的问题

1. 在我的开发环境中，当导入的所有图片的高度和或者宽度和 大于32599px时，生成的css background-position 的y或者x会都是-32599px,查了[winform panel加载多个PictureBox后面的堆一起了](http://bbs.csdn.net/topics/391048464?page=1)和[Windows control size limit](http://www.telerik.com/forums/windows-control-size-limit)中有提到panel是有宽度和宽度最大限制的。。
