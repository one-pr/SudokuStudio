namespace Sudoku.Concepts;

/// <summary>
/// Indicates a segment (an intersection of 3 cells, by a block and a line).
/// </summary>
/// <param name="mask">The mask.</param>
/// <remarks>
/// For more information please visit <see href="http://sudopedia.enjoysudoku.com/Intersection.html">this link</see>.
/// </remarks>
public readonly struct Segment(int mask) :
	IComparable<Segment>,
	IComparisonOperators<Segment, Segment, bool>,
	IEquatable<Segment>,
	IEqualityOperators<Segment, Segment, bool>
{
	/// <summary>
	/// <para>Indicates the backing mask.</para>
	/// <para>
	/// The value contains 2 parts (block index and line index, both are in raw values),
	/// and 10 of 16 bits used, with higher 5 bits represents row or column, lower 5 bits represents block.
	/// </para>
	/// <para>
	/// Although a block index can be compressed by 4 bits instead, I don't adjust the storage rule on this,
	/// in order to keep consistency of storage.
	/// </para>
	/// </summary>
	private readonly int _mask = mask;


	/// <summary>
	/// Initializes a <see cref="Segment"/> instance via the specified block index and line index.
	/// </summary>
	/// <param name="line">The line index.</param>
	/// <param name="block">The block index.</param>
	public Segment(House line, BlockIndex block) : this(line << 5 | block)
	{
	}


	/// <summary>
	/// Indicates whether the line is a row or not.
	/// </summary>
	public bool IsRow => Line < 18;

	/// <summary>
	/// Indicates the block.
	/// </summary>
	public House Block => _mask & 31;

	/// <summary>
	/// Indicates the line (row or column).
	/// </summary>
	public House Line => _mask >> 5 & 31;

	/// <summary>
	/// Indicates the cells used in the whole block.
	/// </summary>
	public ref readonly CellMap BlockMap => ref HousesMap[Block];

	/// <summary>
	/// Indicates the cells used in the whole line.
	/// </summary>
	public ref readonly CellMap LineMap => ref HousesMap[Line];

	/// <summary>
	/// Indicates 3 cells of the segment.
	/// </summary>
	public CellMap Cells => BlockMap & LineMap;


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out House line, out BlockIndex block) => (line, block) = (Line, Block);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Segment comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Segment other) => _mask == other._mask;

	/// <summary>
	/// Returns which one is greater. The method compares the absolute index in the whole segment set space.
	/// </summary>
	/// <param name="other">The other instance to compare.</param>
	/// <returns>
	/// An <see cref="int"/> value represents the result:
	/// <list type="table">
	/// <item>
	/// <term>-1</term>
	/// <description>The right is greater</description>
	/// </item>
	/// <item>
	/// <term>0</term>
	/// <description>They are same</description>
	/// </item>
	/// <item>
	/// <term>1</term>
	/// <description>The left is greater</description>
	/// </item>
	/// </list>
	/// </returns>
	public int CompareTo(Segment other) => ((int)this).CompareTo((int)other);

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => _mask;

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter) => converter.SegmentConverter([this]);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, via the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));


	/// <inheritdoc cref="TryParse(string?, CoordinateParser, out Segment)"/>
	public static bool TryParse([NotNullWhen(true)] string? s, out Segment result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the specified string, converting it into target instance via the specified converter.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out Segment result)
	{
		try
		{
			if (s is null)
			{
				goto ReturnFalse;
			}
			result = Parse(s, converter);
			return true;
		}
		catch (FormatException)
		{
		}

	ReturnFalse:
		result = default;
		return false;
	}

	/// <summary>
	/// Try to parse the specified string, converting it into target instance via the specified culture.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, out Segment result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <inheritdoc cref="Parse(string, CoordinateParser)"/>
	public static Segment Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the specified string, converting it into target instance via the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The result.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Segment Parse(string s, CoordinateParser converter) => converter.SegmentParser(s)[0];

	/// <summary>
	/// Parses the specified string, converting it into target instance via the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The result.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Segment Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));


	/// <inheritdoc/>
	public static bool operator ==(Segment left, Segment right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Segment left, Segment right) => !(left == right);

	/// <inheritdoc/>
	public static bool operator >(Segment left, Segment right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(Segment left, Segment right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(Segment left, Segment right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(Segment left, Segment right) => left.CompareTo(right) <= 0;


	/// <summary>
	/// Projects the value into the order of the whole segment set space.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <exception cref="OverflowException">Throws when the value is invalid.</exception>
	public static explicit operator int(Segment value)
	{
		var line = value.Line;
		var block = value.Block;
		return line < 18 ? line % 9 * 3 + block % 3 : 27 + line % 9 * 3 + block / 3;
	}

	/// <summary>
	/// Projects the order of whole segment set space into the target segment instance.
	/// </summary>
	/// <param name="value">The value.</param>
	/// <exception cref="OverflowException">Throws when the value is invalid.</exception>
	public static explicit operator Segment(int value)
	{
		if (value < 27)
		{
			// In row.
			var row = value / 3 + 9;
			var block = (row - 9) / 3 * 3 + value % 3;
			return new(row, block);
		}
		else
		{
			// In column.
			var column = (value - 27) / 3 + 18;
			var block = (column - 18) / 3 * 3 + (value - 27) / 3;
			return new(column, block);
		}
	}
}
