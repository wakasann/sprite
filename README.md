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
2. 修改 FormMain.cs 中的GetSassCss() 方法，是因为sass background position是三位数，修改成和css一样的background position.
