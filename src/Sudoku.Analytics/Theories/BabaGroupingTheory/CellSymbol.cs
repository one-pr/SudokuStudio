namespace Sudoku.Theories.BabaGroupingTheory;

/// <summary>
/// Defines a symbol that represents a cell supposed, representing a meaning of a fuzzy assignment like <c>r3c2 = <i>x</i></c>.
/// </summary>
/// <param name="mask">The mask.</param>
public readonly struct CellSymbol(int mask) : IEquatable<CellSymbol>, IEqualityOperators<CellSymbol, CellSymbol, bool>
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
	public CellSymbol(Cell cell, ReadOnlySpan<CellSymbolValue> values) :
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
	public ReadOnlySpan<CellSymbolValue> Values
	{
		get
		{
			var result = new List<CellSymbolValue>();
			foreach (var index in _mask & (1 << 18) - 1)
			{
				var type = index / 9;
				var value = index % 9;
				result.Add(new((CellSymbolType)type, value));
			}
			return result.AsSpan();
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is CellSymbol comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(CellSymbol other) => _mask == other._mask;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public override string ToString()
		=> ToString(
			null,
			SR.IsEnglish(CultureInfo.CurrentUICulture)
				? BabaGroupInitialLetter.EnglishLetter_X
				: BabaGroupInitialLetter.EnglishLetter_A,
			BabaGroupLetterCase.Lower
		);

	/// <inheritdoc cref="ToString(IFormatProvider?, BabaGroupInitialLetter, BabaGroupLetterCase)"/>
	public string ToString(BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
		=> ToString(null, initialLetter, @case);

	/// <summary>
	/// Returns a string that represents the current instance.
	/// </summary>
	/// <param name="formatProvider">The format provider.</param>
	/// <param name="initialLetter">The initial letter.</param>
	/// <param name="case">The letter case.</param>
	/// <returns>A string that represents the current instance.</returns>
	public string ToString(IFormatProvider? formatProvider, BabaGroupInitialLetter initialLetter, BabaGroupLetterCase @case)
	{
		var converter = CoordinateConverter.GetInstance(formatProvider);
		var assumedValuesString = string.Join('|', from value in Values select value.ToString(initialLetter, @case));
		return $"{converter.CellConverter(Cell.AsCellMap())} = {assumedValuesString}";
	}


	/// <inheritdoc/>
	public static bool operator ==(CellSymbol left, CellSymbol right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(CellSymbol left, CellSymbol right) => !(left == right);
}
