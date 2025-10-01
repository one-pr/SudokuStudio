---
description: Entailment
---

# 逻辑蕴含

在之前（例如牺牲）的内容里我们遇到过蕴含式的用法，下面我们对蕴含进行一个完整的阐述。

## 基本规则 <a href="#rule" id="rule"></a>

蕴含，也叫蕴涵，指的是我们在平时使用“如果……就会……”、“如果……则……”之类说法的一个特殊表达。这种说法本来只表示一种承前启后的关系，但是因为在逻辑学中，它被定义为了一个表达式，所以它成为了一种连接两个命题的运算，记作 $$P \to Q$$。

这个箭头暗示着我们的过程是从前面往后面推导。而这个运算还有一个特殊结果 $$\lnot P \lor Q$$。

## 结果推算 <a href="#result-calculation" id="result-calculation"></a>

不知道你有没有发现前面蕴含的表达式很诡异。首先是蕴含用的箭头推出，暗示的是前提部分和结论部分的这种推出关系。但这玩意儿怎么还有真假性判断的？而且，$$P \to Q$$ 这玩意儿怎么还可以等价转换成 $$\lnot P \lor Q$$ 的？

这个问题解释起来有些复杂，我们把他具象化为一个实际例子给各位解释一下。

假设我说两个命题 $$P$$ 和 $$Q$$ 分别代指的是“考到驾照了”和“带你去兜风”。也就是说 $$P \to Q$$ 指的是“如果考到驾照了，就带你兜风”。

在这个说法下，我们需要尝试罗列 $$P$$ 和 $$Q$$ 的所有真假情况进行一一排列，于是就会有四种。

* $$P, Q$$：考到驾照了、带你去兜风；（合理）
* $$P, \lnot Q$$：考到驾照了、不带你去兜风；（**不合理**）
* $$\lnot P, Q$$：没有考到驾照、带你去兜风；（合理）
* $$\lnot P, \lnot Q$$：没有考到驾照、不带你去兜风。（合理）

在你尝试把这些信息进行组合的时候，你会发现一个很神奇的点：只有第二种组合 $$P, \lnot Q$$ 不合理。第一种合理是废话；第三种和第四种也合理看起来有点别扭。不过你要这么想：考到驾照才带你去兜风；但是“没考到驾照”并不在“考到驾照”的前提条件下，所以最终兜风与否其实是**不一定**的。不确定是否发生问题其实并不大，而更大的问题其实只在于第二个和假设直接相悖的说法。换言之，蕴含最终表示的是这三种组合的全部情况的联立状态。

所以四个组合里只有第二种明确不符合题意。那么我们尝试把这个三个式子联立起来，即

$$
P \to Q \equiv (P \land Q) \lor (\lnot P \land Q) \lor (\lnot P \land \lnot Q)
$$

我们使用前面 [01-brief-introduction-to-logic.md](01-brief-introduction-to-logic.md "mention") 里所学到的公式进行化简：

$$
\begin{align*}
  P \to Q &\equiv (P \land Q) \lor (\lnot P \land Q) \lor (\lnot P \land \lnot Q)\\
  &\equiv (P \land Q) \lor ((\lnot P \land Q) \lor (\lnot P \land \lnot Q)) &\text{结合律}\\
  &\equiv (P \land Q) \lor (\lnot P \land (Q \lor \lnot Q)) &\text{分配律} \\
  &\equiv (P \land Q) \lor \lnot P &\text{排中律} \\
  &\equiv \lnot P \lor (P \land Q) &\text{交换次序} \\
  &\equiv (\lnot P \lor P) \land (\lnot P \lor Q) &\text{分配律}\\
  &\equiv \lnot P \lor Q &\text{排中律}
\end{align*}
$$

这样我们就有了最后的这个结果。

所以，因为我们有 $$P \to Q \equiv \lnot P \lor Q$$，所以在逻辑学里，蕴含式可以用于真假性判断（虽然这并不是很常见），这种转化关系将蕴含式以命题拆解的另外一个说法表述了出来，变成了实在的、可以判断真假的命题，这是这个式子的意义。

## 为什么互为逆否的命题等价 <a href="#why-contrapositive-and-original-proposition-are-equivalent" id="why-contrapositive-and-original-proposition-are-equivalent"></a>

之前我是要求各位按结论记住的。而对于这一点而言，我们可以尝试使用蕴含来证明得到为什么是等价的，而且很简单。

我们将四种命题从蕴含的箭头改为逻辑或运算：

* 原命题 $$P \to Q \equiv \lnot P \lor Q$$；
* 逆命题 $$Q \to P \equiv \lnot Q \lor P$$；
* 否命题 $$\lnot P \to \lnot Q \equiv \lnot \lnot P \lor \lnot Q \equiv P \lor \lnot Q$$；
* 逆否命题 $$\lnot Q \to \lnot P \equiv \lnot \lnot Q \lor \lnot P \equiv Q \lor \lnot P$$。

可以看出，逆命题和否命题均等于 $$Q \lor \lnot P$$，原命题和逆否命题均等于 $$P \lor \lnot Q$$，这便是为什么互为逆否的两个命题等价的本质原因。
