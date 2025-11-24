namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a group of <see cref="CellSymbolValue"/> instances.
/// </summary>
/// <param name="values">The values.</param>
/// <seealso cref="CellSymbolValue"/>
/// <completionlist cref="CellSymbolValue"/>
public sealed class CellSymbolValues(params IEnumerable<CellSymbolValue> values) :
	SortedSet<CellSymbolValue>(values),
	IComparable<CellSymbolValues>,
	IComparisonOperators<CellSymbolValues, CellSymbolValues, bool>,
	IEquatable<CellSymbolValues>,
	IEqualityOperators<CellSymbolValues, CellSymbolValues, bool>,
	IFormattable,
	IParsable<CellSymbolValues>
{
	/// <summary>
	/// Indicates the first element in this collection.
	/// </summary>
	/// <exception cref="InvalidOperationException">Throws when the current collection is empty.</exception>
	public CellSymbolValue First
	{
		get
		{
			using var enumerator = GetEnumerator();
			return enumerator.MoveNext()
				? enumerator.Current
				: throw new InvalidOperationException("The sequence has no elements.");
		}
	}


	/// <summary>
	/// Indicates an empty instance.
	/// </summary>
	public static CellSymbolValues Empty => [];

	/// <summary>
	/// Indicates the set comparer instance.
	/// </summary>
	private static IEqualityComparer<SortedSet<CellSymbolValue>> EqualityComparer => field ??= CreateSetComparer();


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as CellSymbolValues);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] CellSymbolValues? other) => EqualityComparer.Equals(this, other);

	/// <inheritdoc/>
	public override int GetHashCode() => EqualityComparer.GetHashCode(this);

	/// <summary>
	/// Add a list of values into the current collection.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>The number of elements added.</returns>
	public int AddRange(params CellSymbolValues values)
	{
		var result = 0;
		foreach (var value in values)
		{
			if (Add(value))
			{
				result++;
			}
		}
		return result;
	}

	/// <inheritdoc/>
	public int CompareTo(CellSymbolValues? other)
	{
		if (other is null)
		{
			return 1;
		}

		using var e1 = GetEnumerator();
		using var e2 = other.GetEnumerator();
		while (true)
		{
			var b1 = e1.MoveNext();
			var b2 = e2.MoveNext();
			if (!b1 || !b2)
			{
				return b1 == b2 ? 0 : b1 ? 1 : -1;
			}
			if (e1.Current.CompareTo(e2.Current) is var result and not 0)
			{
				return result;
			}
		}
	}

	/// <inheritdoc/>
	public override string ToString() => ToString(BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	/// <exception cref="InvalidOperationException">Throws when the collection contains invalid data.</exception>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var sb = new StringBuilder();
		foreach (var item in this)
		{
			sb.Append(
				item == CellSymbolValue.Invalid
					? throw new InvalidOperationException("Cannot perform to-string operation because of invalid data encountered.")
					: initialLetter.GetSequence(@case)[item.Index]
			);
		}
		return sb.ToString();
	}

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupLetterCase)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(formatProvider, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="CellSymbolValue.ToString(IFormatProvider?, BabaGroupLetterCase)"/>
	public string ToString(IFormatProvider? formatProvider, BabaGroupLetterCase @case)
		=> ToString(
			SR.IsEnglish(formatProvider as CultureInfo ?? CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			@case
		);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);


	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out CellSymbolValues? result)
		=> TryParse(s, null, BabaGroupLetterCase.Lower, out result);

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case, [NotNullWhen(true)] out CellSymbolValues? result)
	{
		try
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			result = Parse(s, initialLetter, @case);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out CellSymbolValues? result)
		=> TryParse(s, provider, BabaGroupLetterCase.Lower, out result);

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, BabaGroupLetterCase, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, BabaGroupLetterCase @case, [NotNullWhen(true)] out CellSymbolValues? result)
		=> TryParse(s, BabaGroupInitialLetter.GetInstance(provider), @case, out result);

	/// <inheritdoc cref="IParsable{TSelf}.Parse(string, IFormatProvider?)"/>
	public static CellSymbolValues Parse(string s) => Parse(s, null, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="CellSymbolValue.Parse(string, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolValues Parse(string s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var result = Empty;
		foreach (var str in from element in s.Trim() select element.ToString())
		{
			result.Add(CellSymbolValue.Parse(str, initialLetter, @case));
		}
		return result;
	}

	/// <inheritdoc/>
	public static CellSymbolValues Parse(string s, IFormatProvider? provider) => Parse(s, provider, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="CellSymbolValue.Parse(string, IFormatProvider?, BabaGroupLetterCase)"/>
	public static CellSymbolValues Parse(string s, IFormatProvider? provider, BabaGroupLetterCase @case)
		=> Parse(s, BabaGroupInitialLetter.GetInstance(provider), @case);


	/// <inheritdoc/>
	public static bool operator ==(CellSymbolValues? left, CellSymbolValues? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(CellSymbolValues? left, CellSymbolValues? right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(CellSymbolValues left, CellSymbolValues right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(CellSymbolValues left, CellSymbolValues right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(CellSymbolValues left, CellSymbolValues right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(CellSymbolValues left, CellSymbolValues right) => left.CompareTo(right) <= 0;
}
