---
description: Reasoning of Mutant Fish
---

# 交叉鱼的基本推理

## 交叉鱼（Mutant Fish） <a href="#mutant-fish" id="mutant-fish"></a>

<figure><img src="../../.gitbook/assets/images_0557.png" alt="" width="375"><figcaption><p>交叉鱼</p></figcaption></figure>

如图所示。本题的强区域是 `5r26` 和 `5c6`，弱区域是 `5c3` 和 `5b25`。强弱区域都有 3 个，而且每一个数都是一个强弱区域覆盖的，所以套用秩的算法得到这个结构的秩为 0，所有弱区域均可用于删数。

不过稍显遗憾的是，这个题就只能删 1 个数。

我们把这个结构称为**交叉鱼**（Mutant Fish），专门指代强弱区域同时有行列同时出现的组合的特殊鱼情况。

## 交叉鱼的结构起源 <a href="#origin-of-mutant-fish" id="origin-of-mutant-fish"></a>

和前面一样，我们也来看看交叉鱼都是怎么来的。

基于宫内鱼的例子，我们从宫内鱼的结构开始说起。

<figure><img src="../../.gitbook/assets/images_0558.png" alt="" width="361"><figcaption><p>交叉鱼的结构起源-1</p></figcaption></figure>

如图所示。这是一个标准的宫内鱼，三阶的。

变为交叉鱼其实非常简单，只需要一步就可完成。我们只需要把强区域 `b6` 改成 `c7` 就行。改成 `c7` 之后，`r1278c7` 这四个单元格会纳入到强区域之中；取而代之的是，因为改成了 `c7`，所以 `b6` 不再被覆盖的部分全部去掉，于是结构就改成了这样：

<figure><img src="../../.gitbook/assets/images_0559.png" alt="" width="361"><figcaption><p>交叉鱼的结构起源-2</p></figcaption></figure>

如图所示。这样就是一个合格的交叉鱼了。

这种改造也是合理的，这是因为我们不再拘束于观察宫内的情况。我们保留了弱区域可以删除的单元格，转为 `b39` 之后因为弱区域也可用宫代替列覆盖，所以所有数字仍然可以保证完全不变。

所以，交叉鱼在一定程度上是可以被宫内鱼替代掉的，这取决于结构是否都具有这种等价的转化机制；不过，交叉鱼在定义上会显得更为灵活，而对于结构归类和定义分类我们会在稍后进行说明。

## 二阶交叉鱼（Mutant X-Wing）

和前面宫内鱼一样，二阶交叉鱼也没有任何存在的意义和价值。甚至比宫内鱼更加直接，**二阶交叉鱼**（Mutant X-Wing）其实就是拆开的一个行区块和一个列区块，两个区块技巧被并到一起的形态。

<figure><img src="../../.gitbook/assets/images_0565.png" alt="" width="361"><figcaption><p>二阶交叉鱼就是一个行区块和一个列区块</p></figcaption></figure>

如图所示，这其实就是宫内鱼里想提到的那一点。而且，因为刚才说过，宫内鱼和交叉鱼在一定程度上可以转化（转化的地方就是这个宫到列上去），所以这使得交叉鱼是行列区块的等价更为明显。

## 一些例子 <a href="#some-examples" id="some-examples"></a>

### 三阶交叉鱼（Mutant Swordfish） <a href="#mutant-swordfish" id="mutant-swordfish"></a>

<figure><img src="../../.gitbook/assets/images_0560.png" alt="" width="375"><figcaption><p>三阶交叉鱼</p></figcaption></figure>

如图所示。这是一个**三阶交叉鱼**（Mutant Swordfish）。强区域是 `9b3`、`9r7` 和 `9c3`，刚好把行列宫全给用了；弱区域也是 3 个，`9b7`、`9r3` 和 `9c7`。

<figure><img src="../../.gitbook/assets/images_0561.png" alt="" width="375"><figcaption><p>三阶交叉鱼，另一个例子</p></figcaption></figure>

如图所示。这是另外一个例子。

### 四阶交叉鱼（Mutant Jellyfish）

<figure><img src="../../.gitbook/assets/images_0562.png" alt="" width="375"><figcaption><p>四阶交叉鱼</p></figcaption></figure>

如图所示。这是一个**四阶交叉鱼**（Mutant Jellyfish）。强区域是 `3r238` 和 `3c7`，弱区域是 `3b39` 和 `3c25`。

<figure><img src="../../.gitbook/assets/images_0563.png" alt="" width="375"><figcaption><p>四阶交叉鱼，另一个例子</p></figcaption></figure>

如图所示。这也是另外一个例子。
