namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Defines a symbol that represents a cell supposed, representing a meaning of a fuzzy assignment like <c>r3c2 = <i>x</i></c>.
/// </summary>
/// <param name="mask">The mask.</param>
public readonly struct CellSymbol(int mask) :
	IComparable<CellSymbol>,
	IComparisonOperators<CellSymbol, CellSymbol, bool>,
	IEquatable<CellSymbol>,
	IEqualityOperators<CellSymbol, CellSymbol, bool>,
	IFormattable,
	IParsable<CellSymbol>
{
	/// <summary>
	/// <para>
	/// Indicates the mask. The mask uses 25 of 32 bits:
	/// <code>
	/// .--------.---------------------------.-----------------------------------------------------------------------.
	/// |  31-25 | 24| 23| 22| 21| 20| 19| 18| 17| 16| 15| 14| 13| 12| 11| 10| 9 | 8 | 7 | 6 | 5 | 4 | 3 | 2 | 1 | 0 |
	/// :--------+---------------------------+-----------------------------------------------------------------------:
	/// | Unused |         Cell zone         |                           Assumed value zone                          |
	/// '--------'---------------------------'-----------------------------------------------------------------------'
	/// </code>
	/// where assumed value zone stores 18-bit data representing the letter or digit chosen.
	/// </para>
	/// <para>
	/// In the lower 9 bits of assumed value zone, they represents the accurate digits stored;
	/// while higher 9 bits represents the fuzzy digits
	/// (named variables like <c><i>x</i></c>, <c><i>y</i></c> and <c><i>z</i></c>).
	/// </para>
	/// </summary>
	private readonly int _mask = mask;


	/// <summary>
	/// Initializes a <see cref="CellSymbol"/> instance via the cell and values.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="values">The values.</param>
	public CellSymbol(Cell cell, params CellSymbolValues values) :
		this(cell << 18 | values.Aggregate(0, static (interim, next) => interim | 1 << (int)next.Type * 9 + next.Index))
	{
	}


	/// <summary>
	/// Indicates the cell used.
	/// </summary>
	public Cell Cell => _mask >>> 18;

	/// <summary>
	/// Indicates the first value.
	/// </summary>
	public CellSymbolValue FirstValue
		=> BitOperations.TrailingZeroCount(_mask & (1 << 18) - 1) is var r and not FallbackConstants.@int
		&& (r / 9, r % 9) is var (type, value)
			? new((CellSymbolType)type, value)
			: CellSymbolValue.Invalid;

	/// <summary>
	/// Indicates values.
	/// </summary>
	public CellSymbolValues Values
	{
		get
		{
			var result = new SortedSet<CellSymbolValue>();
			foreach (var index in _mask & (1 << 18) - 1)
			{
				var type = index / 9;
				var value = index % 9;
				result.Add(new((CellSymbolType)type, value));
			}
			return [.. result];
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is CellSymbol comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(CellSymbol other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc/>
	public int CompareTo(CellSymbol other)
		=> Cell.CompareTo(other.Cell) is var r1 and not 0
			? r1
			: Values.CompareTo(other.Values) is var r2 and not 0 ? r2 : 0;

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public override string ToString()
		=> ToString(null, BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> ToString(null, initialLetter, @case);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="formatProvider">The format provider for cell notation.</param>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(IFormatProvider? formatProvider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var assumedValuesString = string.Concat(from value in Values select value.ToString(initialLetter, @case));
		return $"{converter.CellConverter(Cell.AsCellMap())} = {assumedValuesString}";
	}

	/// <summary>
	/// Converts the current instance into <see cref="ComplexCellSymbol"/> instance.
	/// </summary>
	/// <returns>The current instance.</returns>
	public ComplexCellSymbol AsComplex() => [this];

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider)
		=> ToString(formatProvider, BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower);


	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, [MaybeNullWhen(false)] out CellSymbol result)
		=> TryParse(s, null, out result);

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out CellSymbol result)
		=> TryParse(s, provider, BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower, out result);

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, out CellSymbolValue)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case, out CellSymbol result)
		=> TryParse(s, null, initialLetter, @case, out result);

	/// <inheritdoc cref="CellSymbolValue.TryParse(string?, IFormatProvider?, out CellSymbolValue)"/>
	public static bool TryParse(
		[NotNullWhen(true)] string? s,
		IFormatProvider? provider,
		BabaGroupInitialLetter initialLetter,
		BabaGroupLetterCase @case,
		out CellSymbol result
	)
	{
		try
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			result = Parse(s, provider);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <inheritdoc cref="CellSymbolValue.Parse(string, IFormatProvider?)"/>
	public static CellSymbol Parse(string s) => Parse(s, null);

	/// <inheritdoc cref="CellSymbolValue.Parse(string, IFormatProvider?)"/>
	public static CellSymbol Parse(string s, IFormatProvider? provider)
		=> Parse(s, provider, BabaGroupInitialLetter.CurrentCultureInstance, BabaGroupLetterCase.Lower);

	/// <inheritdoc cref="CellSymbolValue.Parse(string, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbol Parse(string s, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> Parse(s, null, initialLetter, @case);

	/// <inheritdoc cref="CellSymbolValue.Parse(string, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public static CellSymbol Parse(string s, IFormatProvider? provider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var converter = CoordinateParser.GetInstance(provider);
		var split = s.Split('=', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
		if (split is not [var left, var right])
		{
			throw new FormatException();
		}
		if (converter.CellParser(left) is not [var cell])
		{
			throw new FormatException();
		}
		return new(cell, CellSymbolValue.Parse(right));
	}


	/// <inheritdoc/>
	public static bool operator ==(CellSymbol left, CellSymbol right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(CellSymbol left, CellSymbol right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(CellSymbol left, CellSymbol right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(CellSymbol left, CellSymbol right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(CellSymbol left, CellSymbol right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(CellSymbol left, CellSymbol right) => left.CompareTo(right) <= 0;
}
