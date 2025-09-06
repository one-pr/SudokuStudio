# Introduction

This folder stores some DLL files that will be used in min-lex-rank conversions.

You can copy such files into your program calls the functions defined.

## Interoperability

Define a type like this:

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

Then calls the method `SetMode` to create exchangable buffer:

```c#
VCDESC* pvcdesc;
if (SetMode(1, &pvcdesc) != 0) // 0, 1 or 2.
{
	Console.WriteLine("set mode failed");
	return;
}
```

And then you can call other methods to get conversions.

## Link

Those DLL files are downloaded from [this repository](https://github.com/GPenet/Virtual-calatog). Thank you for your implementation! I do want to study on this, but I really don't have enough time to do researches on it.

