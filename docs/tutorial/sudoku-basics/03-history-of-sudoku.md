---
description: History of Sudoku
---

# 数独历史

数独演变到如今，中间确实存在不少响当当的人物在让数独更加大众化。下面我们来看看数独究竟是如何演变至今的。

## 规则的由来 <a href="#origin-of-constraints" id="origin-of-constraints"></a>

下面给各位展示数独宫和不重复约束规则的由来。

### 宫规则：法国Le Siècle日报 <a href="#block-rule-growing" id="block-rule-growing"></a>

早在 19 世纪末期，法国的 Le Siècle 日报就发布了一篇填数字的游戏。这种填数字游戏看起来似乎跟目前的数独差不多，但区别也非常大。

<figure><img src="../.gitbook/assets/images_0125.png" alt=""><figcaption><p>该日报上的题</p></figcaption></figure>

> 图源：[https://en.wikipedia.org/wiki/Sudoku](https://en.wikipedia.org/wiki/Sudoku)

如图所示，该题目并不包含宫的约束条件，并且它的规则也不同：它要求你在空格里填入 1 到 9，使得每一行、每一列以及两条对角线的填数之和相同。

该规则并没有任何限制额外的填数限制，但你可以看到提示数所在行列似乎填数本身就不重复。因此，这一点给后续产生的数独提供了一些创意。

很显然，从规则求和来看，它来自于一个叫做**幻方**（Magic Square）的阵列。幻方是一种类似的东西，它就刚好要求填数求和一致。但和前文提及的这种填数游戏不同，幻方是一整个盘面内没有要求重复性，而一整个盘面里数字之间不相同，也就是说填数的范围是从 1 到 $$n^2$$。如 6 阶的幻方填入的数字是 1 - 36；3 阶的幻方则是填入 1 到 9。

由于该日报发布的题目规则不同于幻方，因此这并不属于幻方。官方将其称为 Carré Magique Diabolique，英语翻译大概为 Diabolical Magic Square，直译过来是“恶魔般的幻方”。

> 这里 diabolical 有多种理解方式。它的意思是“恶魔一般的”，但也可以翻译为“特别困难的”，即类似于中文的“极难”；也可以翻译成“糟糕透了的”。如何翻译取决于你自己对这种填数游戏的理解。

而它的这个名字则是于 1895 年 7 月 6 号才被固定下来。

早在 1892 年 11 月 19 日，该日报就发布了一则带宫的版本：

<figure><img src="../.gitbook/assets/images_0149.png" alt=""><figcaption><p>带宫的版本</p></figcaption></figure>

> 图源：[https://web.archive.org/web/20061210103525/http:/cboyer.club.fr/multimagie/SupplAncetresSudoku.pdf](https://web.archive.org/web/20061210103525/http:/cboyer.club.fr/multimagie/SupplAncetresSudoku.pdf)

但这看起来更不像数独了。因为它填入的数字并不是 1 到 9，而是一个两位数，但规则仍然是求和一致。

唯一和数独有联系的，可能只有这一个宫的约束性质。不过，这一点，确实为数独提供了一个基本的轮廓。这个填数游戏持续了好一阵子，不过大概在第一次世界大战发生的时间前后，就突然基本消失了。

这便是数独带有宫的、可通过文献找到的最早的记录。

### 不重复规则：欧拉的拉丁方和崔锡鼎的九数略 <a href="#no-dupe-rule-growing" id="no-dupe-rule-growing"></a>

另一种说法是来自于**拉丁方**（Latin Square）。也有翻译成拉丁方块和拉丁方阵的。这个东西于 18 世纪就已经出现。

按照一般说法，拉丁方起源自一个叫做**欧拉**（Leonhard Euler）的数学家。实际上，他也不一定非得是数学家，他在别的领域也有研究。不过以本人来看，他创造的拉丁方是以字母作为填空的元素，而非数字。

然后经考证，似乎这个东西的发明另有其人。一位韩国的数学家**崔锡鼎**（Choi Seok-jeong） 似乎早于欧拉至少 67 年就制作了拉丁方的相似内容。这个东西当时被称为**九数略**（Gusuryak）。

<figure><img src="../.gitbook/assets/images_0172.png" alt=""><figcaption><p>九数略</p></figcaption></figure>

> 图源：[https://web.archive.org/web/20180814135318/https://kyudb.snu.ac.kr/contents/content\_detail.do?code=A00074\&a\_code=A01\&b\_code=B02\&c\_code=C24\&num=74](https://web.archive.org/web/20180814135318/https://kyudb.snu.ac.kr/contents/content_detail.do?code=A00074\&a_code=A01\&b_code=B02\&c_code=C24\&num=74)

请注意图片的右半部分，它并未标记宫的界限，也就是说这也是一个没有宫的图案，但它却保证了每一个单元格填入的这个两位数字，上面的数字在整个盘面里是一个完整的 9 阶拉丁方规则，而下面也是一样。

另外，图片里提及的“洛书”则实指中国的河图洛书。长下面这样。

<figure><img src="../.gitbook/assets/images_0193.png" alt="" width="224"><figcaption><p>河图洛书</p></figcaption></figure>

> 图源：[https://zh.wikipedia.org/zh-hans/%E6%B2%B3%E5%9C%96%E6%B4%9B%E6%9B%B8](https://zh.wikipedia.org/zh-hans/%E6%B2%B3%E5%9C%96%E6%B4%9B%E6%9B%B8)

从图形上来看，中国早已存在对幻方的研究。该图里线条连接的圆点数排布恰为一个 3 阶的幻方：

$$
\begin{matrix}
& & 4 & 9 & 2 & \rightarrow & 15\\
& & 3 & 5 & 7 & \rightarrow & 15\\
& & 8 & 1 & 6 & \rightarrow & 15\\
& \swarrow & \downarrow & \downarrow & \downarrow & \searrow \\
15 & & 15 & 15 & 15 & & 15
\end{matrix}
$$

这是两个广泛被认为是数独的历史文字。

也就是说，崔锡鼎的九数略来自于中国古代的河图洛书，但稍加改良将求和的规则改成了一种不重复的填充规则；而若干年后，欧拉也独立创造出了不重复规则的填充规则。他们互相是没有关系的。

总而言之，数独的宫规则和不重复规则来自于这样两种不同的填数谜题，最终归并到了一起。

## 时间线 <a href="#timeline" id="timeline"></a>

下面为各位总结一下由来的完整时间线。括号里写的来源请参考本文末尾的超链接查看。

{% stepper %}
{% step %}
### 公元前 2200 年——中国 河图洛书 <a href="#in-2200-bc" id="in-2200-bc"></a>

最早形式可追溯到公元前 2200 年，可能来自中国的“河图洛书”。内容请参考前面介绍的部分（来源 1）

之所以说“可能”，是因为它和数独完全不像。唯一一样的地方是洛书转为数字符号时，只用了 1 到 9。这一点作为数独历史由来仍有小范围的争议。
{% endstep %}

{% step %}
### 17 世纪初——朝鲜王朝 崔锡鼎的九数略 <a href="#early-17th-century" id="early-17th-century"></a>

17 世纪由朝鲜时期数学家崔锡鼎出版《九数略》，其填入不重复项的规则是走这个内容演变来的，甚至比欧拉的拉丁方早 67 年出现，详情请参考前文的部分（来源 6）

这一条据说已被证实（来源 7）；但具体出版时间只是提到 1700 年左右。因为广泛认为大概是 1715 年左右出版，所以一般认为是早 67 年（1782 年欧拉发表拉丁方，参考下面这一部分的内容）。
{% endstep %}

{% step %}
### 17 世纪末——瑞士 欧拉的拉丁方 <a href="#late-17th-century" id="late-17th-century"></a>

在 1782 年，瑞士的数学家欧拉提出了拉丁方的概念，即在 $$n \times n$$ 的矩阵里填入 $$n$$ 个符号（这里可以是数字，其实也可以是其他的内容），总之使得每一种符号在任意行和列上都恰好只会出现一次（来源 2）
{% endstep %}

{% step %}
### 19 世纪——法国 报纸计算型数独雏形 <a href="#in-19th-century" id="in-19th-century"></a>

1895 年，法国报纸刊登了近似现代数独的魔方（幻方阵）的变体规则，此时已经可见包含宫的基本规则了（来源 3）
{% endstep %}

{% step %}
### 20 世纪 70 年代——美国 建筑师刊登的数独雏形 <a href="#in-1970s" id="in-1970s"></a>

1979 年，由美国印地安洲的建筑师 Howard Garns 为戴尔杂志创作了 Number Place 的谜题玩法，即现代数独的雏形。这位建筑师的名字也出现在了杂志里，填补部分数字，规则已经和现代的数独标准一样了（来源 2）
{% endstep %}

{% step %}
### 20 世纪 80 年代——日本 引入和命名 <a href="#in-1980s" id="in-1980s"></a>

1984 年，日本 Nikoli 公司出版由**鍜治真起**（Kaji Maki）命名“数独”，来自于句子“数字は独身に限る”，译为“数字唯一”；这位大师不仅推广，还制定了规范化了数独的规则（来源 1）

顺带一说，这位大师因为其推广和投身数独的贡献，被奉为“数独之父”。
{% endstep %}

{% step %}
### 1997 年和 2004 年——日本 新西兰法官推广 <a href="#year-1997-and-2004" id="year-1997-and-2004"></a>

1997 年，新西兰籍的法官**高乐德**（Wayne Gould）在香港担任法官的工作。在他回老家期间，在日本转机的时候发现了数独，于是对此感兴趣，开发了电脑版本的数独出题的程序。

2004 年，他本人说服《泰晤士报》引入数独，随后该游戏风靡英国和欧美报刊（来源 4）
{% endstep %}

{% step %}
### 2006 年——国际化赛事 <a href="#year-2006" id="year-2006"></a>

2006 年，数独游戏开始举办第一场**世界数独锦标赛**（World Sudoku Championship，WSC），吸引了来自多个国家的选手参与，称为全球智力运动的赛事（来源 3）
{% endstep %}

{% step %}
### 现代数独 <a href="#modern-sudoku" id="modern-sudoku"></a>

当今数独以纸笔、app、在线平台等形式多路普及，拥有诸多玩法的变体形式（如杀手数独等）。国际数独日定于每年的 9 月 9 日（来源 5）
{% endstep %}

{% step %}
### 2021 年——鍜治真起去世 <a href="#year-2021" id="year-2021"></a>

2021 年 8 月 10 日，数独之父鍜治真起因胆管癌去世，享年 69 岁。
{% endstep %}
{% endstepper %}

## 参考来源 <a href="#references" id="references"></a>

1. [https://escape-sudoku.com/blog/history-of-sudoku-game](https://escape-sudoku.com/blog/history-of-sudoku-game?)
2. [https://www.sudokuconquest.com/blog/a-brief-history-of-sudoku](https://www.sudokuconquest.com/blog/a-brief-history-of-sudoku?)
3. [https://en.wikipedia.org/wiki/Sudoku](https://en.wikipedia.org/wiki/Sudoku)
4. [https://www.gamesver.com/sudoku-the-history-all-you-need-to-know/](https://www.gamesver.com/sudoku-the-history-all-you-need-to-know/)
5. [https://nationaltoday.com/international-sudoku-day/](https://nationaltoday.com/international-sudoku-day/)
6. [https://web.archive.org/web/20180814135318/https://kyudb.snu.ac.kr/contents/content\_detail.do?code=A00074\&a\_code=A01\&b\_code=B02\&c\_code=C24\&num=74](https://web.archive.org/web/20180814135318/https://kyudb.snu.ac.kr/contents/content_detail.do?code=A00074\&a_code=A01\&b_code=B02\&c_code=C24\&num=74)
7. [https://math-physics-problems.fandom.com/wiki/Mathematical\_Puzzles\_from\_the\_Gusuryak](https://math-physics-problems.fandom.com/wiki/Mathematical_Puzzles_from_the_Gusuryak)
