---
description: Reasoning of Bivalue Oddagon
---

# 死环的基本推理

前文我们介绍了守护者的矛盾逻辑，以及规避矛盾而衍生出的推理过程。下面我们将其推广到两个数字之间的关系，看看它还能有什么新鲜用法。

## 死环类型 2（Bivalue Oddagon Type 2） <a href="#bivalue-oddagon-type-2" id="bivalue-oddagon-type-2"></a>

<figure><img src="../../.gitbook/assets/images_0593.png" alt="" width="375"><figcaption><p>死环类型 2</p></figcaption></figure>

如图所示，这结构就算没推理，看起来就很像是守护者。我们按单元格数一下这个环路的长度，很显然也是 7 个单元格。我们敏锐地发现到，这 7 个单元格除了 `r6c37` 和 `r7c2` 里包含数字 1 以外，剩下的 4 个单元格全都只有 4 和 7。

如果我们假设这三个 1 不存在的话，也就意味着这 7 个单元格里只剩下 4 和 7 两种候选数。按照守护者的推理方式，单个数字显然只能构成填和不填的交替状态；而单元格内只剩下两个数则会构成填 4 和填 7 的交替状态。

但是，由于结构是奇数长度，所以必然最终会在某一个行列宫里出现两个相同数字，这必然是矛盾的。所以，数字 1 不能全部去掉，进而我们知道，这三个 1 里至少有一个 1 是为真的。

显然，`r6c2(1)` 是三个 1 都看得见的地方，所以 `r6c2 <> 1` 便是这个题目的结论。

我们将一个数推广到了两个数字。而这种需要用两个数字交替填写数字的环路被称为**死环**（Bivalue Oddagon）技巧。因为死环的本体（环路）的相貌非常像是在用唯一环，虽然推理逻辑完全不同，但是它这个推理方式非常像是在用类型 2 的思维，所以我们也象征性将死环也归纳出一个类型 2 来。

可能你会有点疑惑。既然我都明确说了是类型 2 的思维方式，那为啥不先讲类型 1？这一点的话，我们暂且不表。我们先来看类型 3。

## 死环类型 3（Bivalue Oddagon Type 3） <a href="#bivalue-oddagon-type-3" id="bivalue-oddagon-type-3"></a>

<figure><img src="../../.gitbook/assets/images_0594.png" alt="" width="375"><figcaption><p>死环类型 3</p></figcaption></figure>

如图所示。这是一个比较简单的、只用 5 个单元格的死环结构。因为它仍然是奇数长度，所以它最终肯定也能形成矛盾。不过这个题并非像是类型 2 一样多出来的是相同的数字，这个题里的守护者是 `r1c3(9)` 和 `r1c5(5)` 这两个。

显然，它俩不同假（否则奇数长度的环出现矛盾），所以它俩至少有一个为真。结合唯一环等的类型 3 的思路，它俩在这个结构里也不能同为真，否则 `r1c8` 则无法填上任何数字。所以，这两个守护者必须只有一个为真。

不论哪一个为真，`r1c8` 都会和它凑成一个关于 5 和 9 的、在 `r1` 上的显性数对结构。所以，`r1` 的其余单元格都不能填 5 或 9，所以这个题的删数是 `r1c1 <> 9` 和 `r1c4 <> 5` 这两个。

这便是类型 3 的推理方式，搭配一个显性数对进行推理的过程。

<figure><img src="../../.gitbook/assets/images_0595.png" alt="" width="375"><figcaption><p>死环类型 3，另一个例子</p></figcaption></figure>

如图所示。这个例子搭配了一个比较大规格的数组结构。不过你也可以看 7 和 8 的隐性数对来解决大规格数组难理解的问题，毕竟在类型 3 逻辑里，显隐性数组也是互补的，在这里也一样。

## 死环有类型 4 吗？ <a href="#is-there-any-bivalue-oddagon-type-4" id="is-there-any-bivalue-oddagon-type-4"></a>

可以从前文里看出，我们压根就没有想提及类型 4 的欲望。那么，死环有类型 4 吗？

至于类型 4 的话，这个说起来会稍微麻烦一些。

<figure><img src="../../.gitbook/assets/images_0597.png" alt="" width="375"><figcaption><p>死环类型 4 草图</p></figcaption></figure>

如图所示。这是类型 4 的一个示意图。我们需要有奇数长度的环路，外带一个共轭对，才能形成类型 4 的推理架构。

<figure><img src="../../.gitbook/assets/images_0598.png" alt="" width="375"><figcaption><p>死环类型 4，代数，第一步</p></figcaption></figure>

如图所示。这样我们可以优先得到这样一些位置的填数情况，这三个单元格很方便假设，因为里面不包含其他数字。下面我们要借用一下 `b2` 的信息了。注意这里有 6 的共轭对，所以我们需要讨论 6 的位置。

<figure><img src="../../.gitbook/assets/images_0599.png" alt=""><figcaption><p>死环类型 4，代数，产生的两种填法</p></figcaption></figure>

如图所示，这样我们可以得到这两种情况。其中，红色的是可以形成数对的删数的位置，橘色的单元格则是形成跨区或不跨区的数对的位置。很显然，两种情况我们取交集就可以得到这样的结果：

<figure><img src="../../.gitbook/assets/images_0600.png" alt="" width="375"><figcaption><p>死环类型 4，代数，可产生的删数结论</p></figcaption></figure>

如图所示，这样我们就可以得到这样的删数位置是两种情况均可产生的结论，也就是说，`{r12c4,r2c6} <> 24` 是这个示意图里可以产生的结论。

## 死环类型 4（Bivalue Oddagon Type 4） <a href="#bivalue-oddagon-type-4" id="bivalue-oddagon-type-4"></a>

下面我们来看一下类型 4 的实际例子。

<figure><img src="../../.gitbook/assets/images_0610.png" alt="" width="375"><figcaption><p>死环类型 4</p></figcaption></figure>

如图所示。我们需要假设两头 `r1c3` 和 `r5c5` 的填数（比如这里显然都是同一个数，我们这里用 $$a$$ 表示）。为什么非得是同一个数呢？因为是奇数长度的环路，共轭对占用的两个格子一定相邻出现，所以余下的单元格满足两点特征：

1. 余下的单元格数量也是个奇数，因为奇数 - 2 还是个奇数；
2. 因为是串联起来的 $$\text{r1c3} \rightarrow A \rightarrow B \rightarrow \text{r5c5}$$，而 $$A$$ 和 $$B$$（即图中的 `r1c4` 和 `r2c5`）显然相邻，所以它俩填的数一定有一个 $$b$$ 和一个 4（4 是共轭对，所以必须填一个），故 `r1c3` 和 `r5c5` 填的肯定是相同的数字 $$a$$。

这么组起来，我们会得到两个两头 `r1c3` 和 `r5c5` 结合共轭对的单元格构成关于 1 和 2 的显性数对，于是两种情况取交集可得到结论一定是 `r1c6 <> 12`。

## 死环类型 1 = 远程数对 <a href="#bivalue-oddagon-type-1-is-equivalent-to-remote-pair" id="bivalue-oddagon-type-1-is-equivalent-to-remote-pair"></a>

那么，死环有类型 1 吗？

对于我们想象的死环类型 1，它理应是一个奇数长度的环，然后某一个单元格除了这两个必需的候选数外，还带有额外的数字，这就和类型 1 匹配上了。不过，我们其实可以不看这个多出来的单元格。它其实是远程数对。

<figure><img src="../../.gitbook/assets/images_0596.png" alt="" width="375"><figcaption><p>死环类型 1</p></figcaption></figure>

如图所示。这便是类型 1 的一个例子。不过很明显，我们完全可以不算上删数单元格，这样看的话其实就是简单的远程数对技巧，因为余下的单元格长度一定是偶数个（奇数个单元格减去 1），所以链路的头尾一定是两个不同的填数。
