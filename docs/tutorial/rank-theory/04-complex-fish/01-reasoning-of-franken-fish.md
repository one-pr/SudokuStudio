---
description: Reasoning of Franken Fish
---

# 宫内鱼的基本推理

## 宫内鱼（Franken Fish） <a href="#franken-fish" id="franken-fish"></a>

<figure><img src="../../.gitbook/assets/images_0547.png" alt="" width="375"><figcaption><p>宫内鱼</p></figcaption></figure>

如图所示。本题有三个强区域和三个弱区域，且每一个数都是“精准”覆盖的，这意味着我们可以直接使用秩的公式得到这个例子的秩为 0。所以所有弱区域都可以用于删数。

我们把这个技巧称为**宫内鱼**（Franken Fish）。是第一种需要介绍的变种鱼结构。

## 宫内鱼的结构起源 <a href="#origin-of-franken-fish" id="origin-of-franken-fish"></a>

宫内鱼的结构来自于普通的鱼。之所以大家能发现到这种长相的鱼结构，是依靠结构变形得到的。

<figure><img src="../../.gitbook/assets/images_0548.png" alt="" width="361"><figcaption><p>宫内鱼的结构起源-1</p></figcaption></figure>

如图所示。这是一个普通的鱼。我们为了简述结构的样貌，直接使用斜杠表示这些位置不包含某个数字，而红色的 x 表示这个位置可以安放这个数，而蓝色的部分则是结构可以删除的部分（弱区域）。

我们试着将列移动一下。只要不在移动后改变结构的样子或者出现必然矛盾的结果，我们就随便改这个结构的长相，毕竟它只是个结构而已。

<figure><img src="../../.gitbook/assets/images_0549.png" alt="" width="361"><figcaption><p>宫内鱼的结构起源-2</p></figcaption></figure>

如图所示。我们在移动了之后，结构的 `c78` 拼在了一起。显然，它只是拼在一起了，但是强弱区域数量并未发生变化，也没有影响结构的成立。

接下来，我们稍微变离谱一些。我们把 `r2` 强区域迁移到 `b3` 里，然后去掉 `r2c2` 这个摆放位置；取而代之的是，将 `b3` 里尚未被弱区域覆盖的 `r123c9` 使用斜杠标记，表示它们不能填此数。

<figure><img src="../../.gitbook/assets/images_0550.png" alt="" width="361"><figcaption><p>宫内鱼的结构起源-3</p></figcaption></figure>

如图所示。这样变化了之后就是一个标准的宫内鱼了。这是怎么来的呢？

我们回到前一个图。我们这种安排无法利用到宫。我们能想到将宫用作鱼结构的效果，只能是挪动到 `b3` 时，因为弱区域本身就自带对 `r123c78` 这 6 个位置的覆盖效果，因此移动过去压根不需要我们做什么。

所以，代价是什么呢？代价就是 `r2c2` 在变位之后需要去掉。在它变为当前这一步用到宫的时候，我们只是单纯去掉了 `r2c2` 这一个位置。如果我们还留着它的话，那么就意味着我们必须为它也带有一个强区域的覆盖，但不论如何覆盖，都会造成结构要么不合法，要么很诡异。最自然的样子就只有图上这种长相。

最终，我们就通过这个变化形式，将鱼结构迁移到了宫内鱼的效果上来。可以看到，本题里，宫内鱼依赖了 `b3` 作为强区域的一部分，而强区域使用的 6 个单元格均位于删除域的两列 `c78` 里，所以理应随意去掉一些位置都是可以的。只要 `r123c7` 和 `r123c8` 这两组位置里，都能最终剩下至少一个位置就行。比如说下面这样：

<figure><img src="../../.gitbook/assets/images_0551.png" alt="" width="361"><figcaption><p>宫内鱼的结构起源-4</p></figcaption></figure>

如图所示。这样也不会影响结构成立。

## 二阶宫内鱼（Franken X-Wing） <a href="#franken-x-wing" id="franken-x-wing"></a>

如果我们按照此规则将结构进行变换，我们会得到下面这样的结构。

<figure><img src="../../.gitbook/assets/images_0552.png" alt="" width="361"><figcaption><p>二阶宫内鱼</p></figcaption></figure>

如图所示。这是**二阶宫内鱼**（Franken X-Wing）结构的基本长相。这虽然也可以用于删数，但是实际上不难发现，它其实等价于区块。严格来说是行列区块。

我们发现，`r8` 只剩余两处安放位置，但它按宫内鱼的变形规则来看，它俩必须在同一个宫，于是 `r8c78` 直接就构成了行列区块。于是，删数直接包含了 6 个位置：`r79c789`。在删除后，`r456c9` 直接在 `c9` 上形成了列区块。也就是说，二阶宫内鱼一定可以等价改成一个行区块和一个列区块。因此，二阶宫内鱼在做题期间是不存在的。

## 一些例子 <a href="#some-examples" id="some-examples"></a>

下面我们来看一些例子。

### 三阶宫内鱼（Franken Swordfish） <a href="#franken-swordfish" id="franken-swordfish"></a>

下面我们来看**三阶交叉鱼**（Franken Swordfish）的例子。

<figure><img src="../../.gitbook/assets/images_0553.png" alt="" width="375"><figcaption><p>三阶宫内鱼</p></figcaption></figure>

如图所示。本题的强区域是 `1r34` 和 `1b9`，弱区域是 `1c489`。

<figure><img src="../../.gitbook/assets/images_0554.png" alt="" width="375"><figcaption><p>三阶宫内鱼，另一个例子</p></figcaption></figure>

如图所示。这是另外一个例子，强区域是 `6r19` 和 `6b5`，弱区域是 `6c469`。

### 四阶宫内鱼（Franken Jellyfish） <a href="#franken-jellyfish" id="franken-jellyfish"></a>

然后我们再来看看**四阶宫内鱼**（Franken Jellyfish）的例子。

<figure><img src="../../.gitbook/assets/images_0555.png" alt="" width="375"><figcaption><p>四阶宫内鱼</p></figcaption></figure>

如图所示。本题的强区域是 `6r268` 和 `6b6` 一共 4 个，弱区域也是 4 个：`6c3478`。

<figure><img src="../../.gitbook/assets/images_0556.png" alt="" width="375"><figcaption><p>四阶宫内鱼，另一个例子</p></figcaption></figure>

如图所示，这题的强区域是 `8r125` 和 `8b7`，弱区域是 `8c1257`。
