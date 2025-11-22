namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a group of <see cref="CellSymbol"/> instances.
/// </summary>
/// <param name="symbols">The symbols.</param>
/// <seealso cref="CellSymbol"/>
public sealed class CellSymbolGroup(params IEnumerable<CellSymbol> symbols) :
	SortedSet<CellSymbol>(symbols),
	IComparable<CellSymbolGroup>,
	IComparisonOperators<CellSymbolGroup, CellSymbolGroup, bool>,
	IEquatable<CellSymbolGroup>,
	IEqualityOperators<CellSymbolGroup, CellSymbolGroup, bool>,
	IFormattable,
	IParsable<CellSymbolGroup>
{
	/// <summary>
	/// Indicates cells used.
	/// </summary>
	public CellMap Cells
	{
		get
		{
			var result = CellMap.Empty;
			foreach (var element in this)
			{
				result += element.Cell;
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates all values used.
	/// </summary>
	public CellSymbolValueGroup Values
	{
		get
		{
			var result = CellSymbolValueGroup.Empty;
			foreach (var element in this)
			{
				result.AddRange(element.Values);
			}
			return result;
		}
	}


	/// <summary>
	/// Represents an empty instance.
	/// </summary>
	public static CellSymbolGroup Empty => [];

	/// <summary>
	/// Indicates the set comparer instance.
	/// </summary>
	private static IEqualityComparer<SortedSet<CellSymbol>> EqualityComparer => field ??= CreateSetComparer();


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => Equals(obj as CellSymbolGroup);

	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] CellSymbolGroup? other) => EqualityComparer.Equals(this, other);

	/// <inheritdoc/>
	public override int GetHashCode() => EqualityComparer.GetHashCode(this);

	/// <summary>
	/// Add a list of values into the current collection.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>The number of elements added.</returns>
	public int AddRange(params CellSymbolGroup values)
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
	public int CompareTo(CellSymbolGroup? other)
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
	public override string ToString() => ToString(null);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider)
		=> ToString(
			formatProvider,
			SR.IsEnglish(CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			BabaGroupLetterCase.Lower
		);

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		// Create a coordinate converter.
		var converter = CoordinateConverter.GetInstance(formatProvider);

		// Group them up by digit.
		var digitDictionary = new SortedDictionary<CellSymbolValueGroup, CellSymbolGroup>();
		foreach (var element in this)
		{
			var values = element.Values;
			if (!digitDictionary.TryAdd(values, [element]))
			{
				digitDictionary[values].Add(element);
			}
		}

		return string.Join(
			", ",
			from value in digitDictionary.Values
			select $"{value.Cells.ToString(converter)} = {value.Values.First.ToString(initialLetter, @case)}"
		);
	}

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(null);


	/// <inheritdoc cref="CellSymbol.TryParse(string?, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbol)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out CellSymbolGroup? result)
		=> TryParse(s, null, out result);

	/// <inheritdoc cref="CellSymbol.TryParse(string?, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbol)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [NotNullWhen(true)] out CellSymbolGroup? result)
		=> TryParse(s, provider, BabaGroupLetterCase.Lower, out result);

	/// <inheritdoc cref="CellSymbol.TryParse(string?, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbol)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, BabaGroupLetterCase @case, [NotNullWhen(true)] out CellSymbolGroup? result)
		=> TryParse(
			s,
			provider,
			SR.IsEnglish(CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			@case,
			out result
		);

	/// <inheritdoc cref="CellSymbol.TryParse(string?, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbol)"/>
	public static bool TryParse(
		[NotNullWhen(true)] string? s,
		BabaGroupInitialLetter initialLetter,
		BabaGroupLetterCase @case,
		[NotNullWhen(true)] out CellSymbolGroup? result
	) => TryParse(s, null, initialLetter, @case, out result);

	/// <inheritdoc cref="CellSymbol.TryParse(string?, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase, out CellSymbol)"/>
	public static bool TryParse(
		[NotNullWhen(true)] string? s,
		IFormatProvider? provider,
		BabaGroupInitialLetter initialLetter,
		BabaGroupLetterCase @case,
		[NotNullWhen(true)] out CellSymbolGroup? result
	)
	{
		try
		{
			if (s is null)
			{
				result = null;
				return false;
			}

			result = Parse(s, provider, initialLetter, @case);
			return true;
		}
		catch (FormatException)
		{
			result = null;
			return false;
		}
	}

	/// <inheritdoc cref="CellSymbol.Parse(string, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolGroup Parse(string s) => Parse(s, null);

	/// <inheritdoc cref="CellSymbol.Parse(string, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolGroup Parse(string s, IFormatProvider? provider) => Parse(s, provider, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="CellSymbol.Parse(string, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolGroup Parse(string s, IFormatProvider? provider, BabaGroupLetterCase @case)
		=> Parse(
			s,
			provider,
			SR.IsEnglish(CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			@case
		);

	/// <inheritdoc cref="CellSymbol.Parse(string, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolGroup Parse(string s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> Parse(s, null, initialLetter, @case);

	/// <inheritdoc cref="CellSymbol.Parse(string, IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbolGroup Parse(string s, IFormatProvider? provider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		// The equation sequences should be split with ',' token, but coordinates may use commas.
		// We should locate each equation by '=' token, and find for the next ',' after an equality operator token '='.
		var values = parseExpressions(s);

		var result = Empty;
		var parser = CoordinateParser.GetInstance(provider);

		// Iterate variables.
		foreach (var (cellsString, cellSymbolValuesString) in values)
		{
			var cells = parser.CellParser(cellsString);
			var letters = CellSymbolValueGroup.Parse(cellSymbolValuesString, initialLetter, @case);
			foreach (var cell in cells)
			{
				result.Add(new(cell, letters));
			}
		}
		return result;


		static ReadOnlySpan<(string Left, string Right)> parseExpressions(ReadOnlySpan<char> input)
		{
			if (input.Length == 0)
			{
				return [];
			}

			var result = new List<(string, string)>();
			for (int pos = 0, end; pos < input.Length; pos = end + 1)
			{
				// Find for the first equation token.
				if (input[pos..].IndexOf('=') is not (var equationTokenIndex and not -1))
				{
					throw new FormatException();
				}

				// Find for the first appearance of comma after equation token '='.
				var commaAfterEquationTokenIndex = input[(equationTokenIndex + 1)..].IndexOf(',');
				end = commaAfterEquationTokenIndex == -1 ? input.Length : commaAfterEquationTokenIndex;

				// Add the split result.
				var left = input[pos..equationTokenIndex].Trim();
				var right = input[(equationTokenIndex + 1)..end].Trim();
				result.Add((left.ToString(), right.ToString()));
			}
			return result.AsSpan();
		}
	}


	/// <inheritdoc/>
	public static bool operator ==(CellSymbolGroup? left, CellSymbolGroup? right)
		=> (left, right) switch { (null, null) => true, (not null, not null) => left.Equals(right), _ => false };

	/// <inheritdoc/>
	public static bool operator !=(CellSymbolGroup? left, CellSymbolGroup? right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(CellSymbolGroup left, CellSymbolGroup right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(CellSymbolGroup left, CellSymbolGroup right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(CellSymbolGroup left, CellSymbolGroup right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(CellSymbolGroup left, CellSymbolGroup right) => left.CompareTo(right) <= 0;
}
