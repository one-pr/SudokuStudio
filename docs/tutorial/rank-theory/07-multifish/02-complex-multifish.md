---
description: Complex Multifish
---

# 复杂复数鱼

下面我们来看一些类似复杂鱼的**复杂复数鱼**（Complex Multifish）逻辑。

不过，因为复数鱼会同时使用行、列、宫、单元格四种不同的强弱区域类型，这一点和鱼不太一样，因此对复数鱼而言就没有必要严格区分宫内鱼还是交叉鱼了。

## 例子 1：单元格强区域 <a href="#example-1" id="example-1"></a>

<figure><img src="../../.gitbook/assets/images_0656.png" alt="" width="375"><figcaption><p>例子 1</p></figcaption></figure>

如图所示。这个题的强区域是 `1357r2458` 和 `r6c6` 一共 17 个，而弱区域是 `17c1, 35c3, 135c6, 37c7, 157c9, 5n24, 28n5, 4n8` 这 17 个，所以也是零秩结构。

## 例子 2：同时有行列强区域 <a href="#example-2" id="example-2"></a>

<figure><img src="../../.gitbook/assets/images_0657.png" alt="" width="375"><figcaption><p>例子 2</p></figcaption></figure>

如图所示。这个题的绿色是行强区域，橙色是列强区域。这个比较难看懂，所以我简单对强弱区域描述一下。

首先强区域是 `56789r3` 和 `1234c2347` 一共 21 个，而弱区域是 `12r49`、`34r68`、`3n23456789`、`2n34`、`57n2` 和 `1n7` 这 21 个，所以是零秩的。

这里要说的是 `r3c47` 这两个单元格。这两个单元格里都同时包含两种颜色，但这并不是在说它是强三元组或弱三元组，因为这两个单元格里用到两种配色是因为它是同时被行上的强区域和列上强区域经过的特殊节点。但是，行的强区域是 5、6、7、8、9，而列上的强区域则是 1、2、3、4，数字压根不会重叠，所以不存在任何的三元组。

## 例子 3：宫强区域 <a href="#example-3" id="example-3"></a>

<figure><img src="../../.gitbook/assets/images_0658.png" alt="" width="375"><figcaption><p>例子 3</p></figcaption></figure>

如图所示。这个题会用到两个（或者说 1 个也行）宫内的强区域：`9b1` 和 `9b9`。细数一下，强区域数量是 21 个，弱区域数量也是 21 个。数的方式和前文一样就不再重复了。

## 例子 4：行列宫强区域 <a href="#example-4" id="example-4"></a>

<figure><img src="../../.gitbook/assets/images_0661.png" alt="" width="375"><figcaption><p>例子 5</p></figcaption></figure>

如图所示。这个题看起来最为复杂，一共用了 21 个强区域和 21 个弱区域，其中 `8r3` 和 `8b9` 是两个非列弱区域需要单独纳入。
