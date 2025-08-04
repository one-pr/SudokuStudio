namespace Sudoku.Behaviors.Ittoryu;

/// <summary>
/// Indicates the target digit path.
/// </summary>
/// <param name="Digits">The digits path.</param>
[CollectionBuilder(typeof(DisorderedIttoryuDigitPath), nameof(Create))]
public readonly record struct DisorderedIttoryuDigitPath(Digit[] Digits) :
	IComparable<DisorderedIttoryuDigitPath>,
	IComparisonOperators<DisorderedIttoryuDigitPath, DisorderedIttoryuDigitPath, bool>,
	IEnumerable<Digit>
{
	/// <summary>
	/// Indicates whether the pattern is complete.
	/// </summary>
	public bool IsComplete => Digits.Length == 9;

	/// <summary>
	/// Indicates hte digits string.
	/// </summary>
	private string[] DigitsString => from digit in Digits select (digit + 1).ToString();


	/// <inheritdoc/>
	public int CompareTo(DisorderedIttoryuDigitPath other) => GetHashCode().CompareTo(other.GetHashCode());

	/// <inheritdoc/>
	public bool Equals(DisorderedIttoryuDigitPath other) => Digits.Length == other.Digits.Length && GetHashCode() == other.GetHashCode();

	/// <inheritdoc/>
	public override int GetHashCode()
	{
		var (result, multiplicativeIdentity) = (0, 1);
		foreach (var digit in Digits.AsReadOnlySpan().EnumerateReversely())
		{
			result += digit * multiplicativeIdentity;
			multiplicativeIdentity *= 10;
		}

		return result;
	}

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString("->");

	/// <inheritdoc cref="ToString()"/>
	public string ToString(string? separator)
		=> separator switch { null or [] => string.Concat(DigitsString), _ => string.Join(separator, DigitsString) };

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<Digit> GetEnumerator() => new(Digits);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Digits.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<Digit> IEnumerable<Digit>.GetEnumerator() => Digits.AsEnumerable().GetEnumerator();


	/// <summary>
	/// Creates a <see cref="DisorderedIttoryuDigitPath"/> instance via collection expression.
	/// </summary>
	/// <param name="digits">A list of digits to be initialized.</param>
	/// <returns>A <see cref="DisorderedIttoryuDigitPath"/> instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static DisorderedIttoryuDigitPath Create(ReadOnlySpan<Digit> digits) => new([.. digits]);


	/// <inheritdoc/>
	public static bool operator >(DisorderedIttoryuDigitPath left, DisorderedIttoryuDigitPath right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(DisorderedIttoryuDigitPath left, DisorderedIttoryuDigitPath right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(DisorderedIttoryuDigitPath left, DisorderedIttoryuDigitPath right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(DisorderedIttoryuDigitPath left, DisorderedIttoryuDigitPath right) => left.CompareTo(right) <= 0;


	/// <summary>
	/// Implicit cast from a <see cref="Digit"/> sequence into a <see cref="DisorderedIttoryuDigitPath"/>.
	/// </summary>
	/// <param name="digitSequence">A digit sequence. Please note that the value can be <see langword="null"/>.</param>
	public static implicit operator DisorderedIttoryuDigitPath(Digit[]? digitSequence) => new(digitSequence is null ? [] : digitSequence);
}
