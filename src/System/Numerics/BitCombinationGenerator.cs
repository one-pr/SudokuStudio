namespace System.Numerics;

/// <summary>
/// Represents a combination generator that iterations each combination of bits for the specified number of bits, and how many 1's in it.
/// </summary>
/// <typeparam name="TInteger">The type of the target integer value.</typeparam>
/// <param name="_bitCount">Indicates the number of bits.</param>
/// <param name="_oneCount">Indicates the number of bits set <see langword="true"/>.</param>
[DebuggerStepThrough]
public readonly ref struct BitCombinationGenerator<TInteger>(int _bitCount, int _oneCount)
	where TInteger : IBinaryInteger<TInteger>
{
	/// <summary>
	/// Gets the enumerator of the current instance in order to use <see langword="foreach"/> loop.
	/// </summary>
	/// <returns>The enumerator instance.</returns>
	public BitCombinationEnumerator<TInteger> GetEnumerator() => new(_bitCount, _oneCount);
}
