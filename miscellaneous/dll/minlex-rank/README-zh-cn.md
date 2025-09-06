# 介绍

这个文件夹里存的是一些关于最小字典序和其全局排序的 DLL 文件。

你可以使用一些在这几个 DLL 里预定义好的函数，用于开发。

## 可交互性

先定义出一个可以用于交互的类型：

```c#
struct VCDESC
{
	public CUInt64 rank; // ulong
	public CInt i416, i9992, i660k1, ir1, i660k2, ir2; // int
	public G g;

	public struct G
	{
		public fixed CChar b1[27]; // sbyte
		public fixed CChar r4[9]; // sbyte
		public fixed CChar rx[45]; // sbyte
	}
}
```

然后调用 `SetMode` 方法，传入参数用作数据交互的缓冲区。

```c#
VCDESC* pvcdesc;
if (SetMode(1, &pvcdesc) != 0) // 0, 1 or 2.
{
	Console.WriteLine("set mode failed");
	return;
}
```

然后你就可以随便用其他的转换功能了。

## 链接

这些 DLL 文件是我从[这个仓库](https://github.com/GPenet/Virtual-calatog)拷贝下来的。感谢老哥的实现！我确实有打算研究一下最小字典序和全局排序的东西，但是我实在是没有时间。