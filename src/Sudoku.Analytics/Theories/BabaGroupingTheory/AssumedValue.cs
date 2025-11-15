namespace Sudoku.Theories.BabaGroupingTheory;

/// <summary>
/// Represents an assumed value.
/// </summary>
/// <param name="mask"><inheritdoc cref="_mask" path="/summary"/></param>
public readonly struct AssumedValue(byte mask) : IEquatable<AssumedValue>, IEqualityOperators<AssumedValue, AssumedValue, bool>
{
	/// <summary>
	/// Indicates the mask.
	/// </summary>
	private readonly byte _mask = mask;


	/// <summary>
	/// Initializes an <see cref="AssumedValue"/> instance via the specified type and value.
	/// </summary>
	/// <param name="type">The type.</param>
	/// <param name="value">The value.</param>
	public AssumedValue(AssumedValueType type, Digit value) : this((byte)((int)type << 4 | value))
	{
	}


	/// <summary>
	/// Indicates the assumed type.
	/// </summary>
	public AssumedValueType Type => (AssumedValueType)(_mask >> 4 & 1);

	/// <summary>
	/// Indicates the index.
	/// </summary>
	public Digit Index => _mask & 15;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is AssumedValue comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(AssumedValue other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(BabaGroupInitialLetter.EnglishLetter_X, BabaGroupLetterCase.Lower);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> initialLetter.GetSequence(@case)[Index].ToString();


	/// <inheritdoc/>
	public static bool operator ==(AssumedValue left, AssumedValue right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(AssumedValue left, AssumedValue right) => !(left == right);
}
