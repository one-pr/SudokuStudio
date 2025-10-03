---
description: Relations between Domino Loop & Multifish
---

# 多米诺环和复数鱼的关系

多米诺环是可以用复数鱼技巧的视角的。我们知道，多米诺环的推理方式更类似于网结构，所以所有的强区域显然是单元格类型的。但是我们要将此转化为复数鱼那样使用行列宫强区域的形式，所以我们需要一定程度上的转化方式。下面我们来看两种转化方式。

## 用宫弱区域转化 <a href="#transform-to-block-links" id="transform-to-block-links"></a>

第一种方式是将多米诺环里使用的行列弱区域改为旁边的宫，构成宫弱区域。

<figure><img src="../../.gitbook/assets/images_0659.png" alt="" width="375"><figcaption><p>例子 1</p></figcaption></figure>

如图所示。这是一个多米诺环。这个结构想必就不用我多介绍了吧。它的复数鱼画法是将 `c47` 或 `r47` 分别转移到 `b23` 或 `b47` 里去，然后改变一下链接的数字就行。

比如这个题，它对应了两个复数鱼的视角。

<figure><img src="../../.gitbook/assets/images_0660.png" alt=""><figcaption><p>例子 1，复数鱼视角</p></figcaption></figure>

如图所示。有些怪，是吗？它利用了一个特殊的覆盖规则，宫内的弱区域。左图里将 `b47` 视为了宫内的弱区域进行覆盖，其实也可以按列弱区域覆盖，因为这两种数出来都会被计 6 次（6 个弱区域）。比如按宫的话就是 `679b4` 和 `367b7`，而按列的话就是 `7c1`、`679c2` 和 `36c3`。这都刚好是 6 个弱区域。不过宫的话归纳起来只需要两个宫就完事了，但列上需要三个不同的列参与进来。

## 用显隐性互补转化 <a href="#using-naked-and-hidden-complement" id="using-naked-and-hidden-complement"></a>

另外一种手段是利用显隐性互补的形式进行转化。这种转化会麻烦一些，因为它要大幅度修改结构。

<figure><img src="../../.gitbook/assets/images_0662.png" alt="" width="375"><figcaption><p>例子 2</p></figcaption></figure>

如图所示。这是另外一个多米诺环的例子。这个例子我们需要转化为显隐性互补的另外一个视角。转化方式是利用行列传递的弱区域进行；将 `b1379` 四个宫作为弱区域，然后将 `r28c28` 四个区域利用起来，将宫里使用的数字 1、2、6、7 选中改成强区域，并移除其他数字。

<figure><img src="../../.gitbook/assets/images_0663.png" alt="" width="375"><figcaption><p>例子 2，复数鱼视角</p></figcaption></figure>

如图所示。这样我们就得到了另外一种转化的复数鱼结构。

至此我们就把复数鱼的内容介绍完了。下面我们将继续学习秩理论的进阶分析的内容。
