namespace Sudoku.Concepts;

/// <summary>
/// Represents a <see href="http://sudopedia.enjoysudoku.com/Conjugate_pair.html">conjugate pair</see>.
/// </summary>
/// <remarks>
/// A <b>Conjugate pair</b> is a pair of two candidates, in the same house,
/// where all cells has only two position can fill this candidate.
/// </remarks>
/// <param name="_mask">Indicates the target mask.</param>
public readonly struct Conjugate(int _mask) : IEquatable<Conjugate>, IEqualityOperators<Conjugate, Conjugate, bool>
{
	/// <summary>
	/// Initializes a <see cref="Conjugate"/> instance with from and to cell offset and a digit.
	/// </summary>
	/// <param name="from">The from cell.</param>
	/// <param name="to">The to cell.</param>
	/// <param name="digit">The digit.</param>
	public Conjugate(Cell from, Cell to, Digit digit) : this(digit << 20 | from << 10 | to)
	{
	}

	/// <summary>
	/// Initializes a <see cref="Conjugate"/> instance with the map and the digit.
	/// The map should contains two cells, the first one is the start one, and the second one is the end one.
	/// </summary>
	/// <param name="map">The map.</param>
	/// <param name="digit">The digit.</param>
	public Conjugate(in CellMap map, Digit digit) : this(map[0], map[1], digit)
	{
	}


	/// <summary>
	/// Indicates the "from" cell, i.e. the base cell that starts the conjugate pair.
	/// </summary>
	public Cell From => _mask & 1023;

	/// <summary>
	/// Indicates the "to" cell, i.e. the target cell that ends the conjugate pair.
	/// </summary>
	public Cell To => _mask >> 10 & 1023;

	/// <summary>
	/// Indicates the digit used.
	/// </summary>
	public Digit Digit => _mask >> 20 & 15;

	/// <summary>
	/// Indicates the target line of the two cells lie in.
	/// </summary>
	public House Line => Map.SharedLine;

	/// <summary>
	/// Indicates the house that the current conjugate pair lies in.
	/// </summary>
	public HouseMask Houses => Map.SharedHouses;

	/// <summary>
	/// Indicates the cells (the "from" cell and "to" cell).
	/// </summary>
	public CellMap Map => [From, To];


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out Candidate fromCand, out Candidate toCand) => (fromCand, toCand) = (From * 9 + Digit, To * 9 + Digit);

	/// <inheritdoc cref="object.Equals(object?)"/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is Conjugate comparer && Equals(comparer);

	/// <inheritdoc/>
	public bool Equals(Conjugate other) => Digit == other.Digit && Map == other.Map;

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(Digit, Map);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation, using the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter) => converter.ConjugateConverter([this]);


	/// <summary>
	/// Try to parse the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, out Conjugate result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the specified string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, out Conjugate result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <summary>
	/// Try to parse the specified string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out Conjugate result)
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
	/// Parses the string into target instance.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <returns>The result.</returns>
	public static Conjugate Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the string into target instance via the specified culture.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The result.</returns>
	public static Conjugate Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));

	/// <summary>
	/// Parses the string into target instance via the specified converter.
	/// </summary>
	/// <param name="s">The string to parse.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The result.</returns>
	public static Conjugate Parse(string s, CoordinateParser converter) => converter.ConjugateParser(s)[0];


	/// <inheritdoc/>
	public static bool operator ==(Conjugate left, Conjugate right) => left.Equals(right);

	/// <inheritdoc/>
	public static bool operator !=(Conjugate left, Conjugate right) => !(left == right);
}
