namespace System.Numerics;

/// <summary>
/// Enumerates the next k-combination represented as a bitmask using Gosper's hack.
/// </summary>
/// <typeparam name="TInteger">The type of the target integer value.</typeparam>
/// <param name="_bitCount">The number of bits.</param>
/// <param name="_oneCount">The number of <see langword="true"/> bits.</param>
/// <remarks>
/// <para>
/// Given a bitmask <c>x</c> that has exactly <c>k</c> bits set (ones), this routine computes
/// the bitmask that represents the next combination (in the usual bitmask ordering
/// where less-significant positions change first) while preserving the number of set bits.
/// The transformation is done using a few constant-time bit operations:
/// </para>
/// <para><b>Algorithm (high level)</b>:</para>
/// <list type="number">
/// <item><c>smallest = x &amp; -x</c> — isolate the least-significant set bit of <c>x</c>.</item>
/// <item>
/// <c>ripple = x + smallest</c> — add that isolated bit back to <c>x</c>.<br/>
/// This produces a carry that clears a trailing block of ones and sets the next higher zero bit (if any),
/// effectively "pushing" the low block of ones upward.
/// </item>
/// <item>
/// <c>ones = x ^ ripple</c> — this captures the bits that changed:
/// the cleared trailing ones together with the new carry bit.<br/>
/// Shifting and scaling those bits produces a right-aligned block of ones
/// that should appear at the least-significant positions after the ripple.
/// </item>
/// <item>
/// <c>ones = (ones >> 2) / smallest</c> — right-shift twice, then divide by the power-of-two <c>smallest</c><br/>
/// This expression moves that block of ones down to the lowest bits,
/// producing a mask with the correct number of trailing ones that remain after the higher bit was promoted.
/// </item>
/// <item>
/// <c>x = ripple | ones</c> — composes the promoted higher bit(s)
/// with the right-aligned trailing ones to form the next k-combination.
/// </item>
/// </list>
/// <para><b>Why this is correct (intuition)</b>:</para>
/// <para>
/// Let <c>r</c> be the number of consecutive ones that were cleared by the addition
/// (i.e., the length of the trailing one-block involved in the carry).
/// The addition sets a single higher bit (the carry destination) and clears those <c>r</c> lower ones.
/// The XOR with the original <c>x</c> yields a word containing
/// exactly those <c>r</c> cleared ones plus the single new carry bit (so popcount = <c>r</c> + 1).
/// Shifting the XOR result right by two removes the carry bit and one zero that separates
/// where the carry landed from the cleared block;
/// dividing by <c>smallest</c> (a power of two) shifts the remaining ones down so they
/// become a right-aligned block of <c>r</c> ones.
/// OR-ing with <c>ripple</c> (which holds the newly promoted higher bit) yields the next bitmask
/// that has the same number of ones, but with the leftmost moved-up one and the remaining ones placed as far right as possible.
/// </para>
/// <para>
/// <b>Complexity</b>: O(1) — only a handful of machine-word bit-operations (AND, ADD, XOR, shifts, divide by power of two).
/// </para>
/// <para>
/// <b>Auto play</b> (the first 3 steps, with values <paramref name="_bitCount"/> = 8, <paramref name="_oneCount"/> = 5):
/// <code><![CDATA[
/// .------.----------------------------.------------------------------------.----------.   .-----------.-----------.
/// | Step | Variable                   | Expression                         | Value    |   | _bitCount | _oneCount |
/// :------+----------------------------+------------------------------------+----------|   |-----------+-----------:
/// |    1 | current (before)           | current                            | 00011111 |   | 8         | 5         |
/// |      | _mask                      | (1 << (_bitCount - _oneCount)) - 1 | 00000111 |   '-----------'-----------'
/// |      | current & -current         | current & -current                 | 00000001 |
/// |      | current & -current & _mask | current & -current & _mask         | 00000001 |
/// |      | smallest                   | current & -current                 | 00000001 |
/// |      | ripple                     | current + smallest                 | 00100000 |
/// |      | ones (before shift)        | current ^ ripple                   | 00111111 |
/// |      | ones after                 | (ones >> 2) / smallest             | 00001111 |
/// |      | current (after)            | ripple | ones                      | 00101111 |
/// :------+----------------------------+------------------------------------+----------:
/// |    2 | current (before)           | current                            | 00101111 |
/// |      | _mask                      | (1 << (_bitCount - _oneCount)) - 1 | 00000111 |
/// |      | current & -current         | current & -current                 | 00000001 |
/// |      | current & -current & _mask | current & -current & _mask         | 00000001 |
/// |      | smallest                   | current & -current                 | 00000001 |
/// |      | ripple                     | current + smallest                 | 00110000 |
/// |      | ones (before shift)        | current ^ ripple                   | 00011111 |
/// |      | ones after                 | (ones >> 2) / smallest             | 00000111 |
/// |      | current (after)            | ripple | ones                      | 00110111 |
/// :------+----------------------------+------------------------------------+----------+
/// |    3 | current (before)           | current                            | 00110111 |
/// |      | _mask                      | (1 << (_bitCount - _oneCount)) - 1 | 00000111 |
/// |      | current & -current         | current & -current                 | 00000001 |
/// |      | current & -current & _mask | current & -current & _mask         | 00000001 |
/// |      | smallest                   | current & -current                 | 00000001 |
/// |      | ripple                     | current + smallest                 | 00111000 |
/// |      | ones (before shift)        | current ^ ripple                   | 00001111 |
/// |      | ones after                 | (ones >> 2) / smallest             | 00000011 |
/// |      | current (after)            | ripple | ones                      | 00111011 |
/// '------'----------------------------'------------------------------------'----------'
/// ]]></code>
/// </para>
/// </remarks>
[Obsolete($"Use '{nameof(SequenceExtensions)}.operator &({nameof(ReadOnlySpan)}<T>, int)' to construct combinations instead.", false)]
public ref struct BitCombinationEnumerator<TInteger>(int _bitCount, int _oneCount) : IEnumerator<TInteger>
	where TInteger : IBinaryInteger<TInteger>
{
	/// <summary>
	/// The mask.
	/// </summary>
	private readonly TInteger _mask = (TInteger.MultiplicativeIdentity << _bitCount - _oneCount) - TInteger.MultiplicativeIdentity;

	/// <summary>
	/// Indicates whether that the value is the last one.
	/// </summary>
	private bool _isLast = _bitCount == 0;


	/// <inheritdoc cref="IEnumerator.Current"/>
	public TInteger Current { get; private set; } = (TInteger.MultiplicativeIdentity << _oneCount) - TInteger.MultiplicativeIdentity;

	/// <inheritdoc/>
	readonly object IEnumerator.Current => Current;


	/// <inheritdoc cref="IEnumerator.MoveNext"/>
	public bool MoveNext()
	{
		// Check whether another combination is available.
		var result = HasNext();

		if (result && !_isLast)
		{
			// Step 1: Find the lowest set bit (the rightmost '1'),
			// e.g., if Current = 0b10100, then -Current = 0b01100 (two's complement), and smallest = 0b00100.
			var smallest = Current & -Current;

			// Step 2: Add smallest to Current to "ripple" the lowest set bit to the left,
			// e.g., 0b10100 + 0b00100 = 0b11000.
			var ripple = Current + smallest;

			// Step 3: Find the bits that changed between Current and ripple i.e., where the bits got reset or flipped.
			var ones = Current ^ ripple;

			// Step 4: Right-shift by 2 to remove the flipped bit and one more,
			// then divide by smallest to reposition the remaining set bits back to the lowest possible positions.
			ones = (ones >> 2) / smallest;

			// Step 5: Combine the rippled result and repositioned ones
			// to produce the next valid combination with the same number of 1's.
			Current = ripple | ones;
		}

		return result;
	}

	/// <inheritdoc/>
	[DoesNotReturn]
	readonly void IEnumerator.Reset() => throw new NotImplementedException();

	/// <inheritdoc/>
	readonly void IDisposable.Dispose()
	{
	}

	/// <summary>
	/// Changes the state of the fields, and check whether the bit has another available possibility to be iterated.
	/// </summary>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	private bool HasNext()
	{
		var result = !_isLast;

		// Check whether the current value is the last possible combination.
		// Extract the lowest 1-bit (Current & -Current), and check if it's still within the valid bit range (using _mask).
		// If there's no more movable '1' in the legal mask range, we've reached the last combination.
		_isLast = (Current & -Current & _mask) == TInteger.AdditiveIdentity;

		return result;
	}
}
