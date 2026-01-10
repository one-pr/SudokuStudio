namespace Sudoku.Concepts;

/// <summary>
/// Represents a chute (a band or a tower, representing 27 cells in 3 aligned blocks).
/// </summary>
/// <param name="Index">Index of the chute. The value is between 0 and 6.</param>
/// <param name="IsRow">Indicates whether the chute is in a mega-row.</param>
/// <param name="HousesMask">Indicates the houses used.</param>
public readonly record struct Chute(int Index, bool IsRow, HouseMask HousesMask) : IEqualityOperators<Chute, Chute, bool>
{
	/// <summary>
	/// Indicates the minimum chute index.
	/// </summary>
	public const int MinChuteIndex = 0;

	/// <summary>
	/// Indicates the maximum chute index.
	/// </summary>
	public const int MaxChuteIndex = 6;


	/// <summary>
	/// Backing field of <see cref="ChuteMaps"/>.
	/// </summary>
	private static readonly CellMap[] ChuteMapsBackingField;

	/// <summary>
	/// Backing field of <see cref="Chutes"/>.
	/// </summary>
	private static readonly Chute[] ChutesBackingField;


	/// <include file='../../global-doc-comments.xml' path='g/static-constructor' />
	static Chute()
	{
		//
		// ChuteMaps
		//
		ChuteMapsBackingField = new CellMap[6];
		for (var chute = 0; chute < 3; chute++)
		{
			var ((r1, r2, r3), (c1, c2, c3)) = (ChuteHouses[chute], ChuteHouses[chute + 3]);
			(ChuteMapsBackingField[chute], ChuteMapsBackingField[chute + 3]) = (HousesMap[r1] | HousesMap[r2] | HousesMap[r3], HousesMap[c1] | HousesMap[c2] | HousesMap[c3]);
		}

		//
		// Chutes
		//
		ChutesBackingField = new Chute[6];
		for (var chute = 0; chute < 3; chute++)
		{
			var ((r1, r2, r3), (c1, c2, c3)) = (ChuteHouses[chute], ChuteHouses[chute + 3]);
			(ChutesBackingField[chute], ChutesBackingField[chute + 3]) = (
				new(chute, true, 1 << r1 | 1 << r2 | 1 << r3),
				new(chute + 3, false, 1 << c1 | 1 << c2 | 1 << c3)
			);
		}
	}


	/// <summary>
	/// Indicates the cells in this chute.
	/// </summary>
	public ref readonly CellMap Cells => ref ChuteMaps[Index];


	/// <summary>
	/// Indicates the chute maps.
	/// </summary>
	public static ReadOnlySpan<CellMap> ChuteMaps => ChuteMapsBackingField;

	/// <summary>
	/// Indicates a list of <see cref="Chute"/> instances representing chutes.
	/// </summary>
	public static ReadOnlySpan<Chute> Chutes => ChutesBackingField;

	/// <summary>
	/// Indicates chute houses.
	/// </summary>
	private static ReadOnlySpan<(House, House, House)> ChuteHouses
		=> ((House, House, House)[])[(9, 10, 11), (12, 13, 14), (15, 16, 17), (18, 19, 20), (21, 22, 23), (24, 25, 26)];


	/// <inheritdoc cref="ToString(CoordinateConverter)"/>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter) => converter.ChuteConverter([this]);


	/// <summary>
	/// Try to parse the current string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The instance parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, out Chute result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the current string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The instance parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, out Chute result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <summary>
	/// Try to parse the current string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The instance parsed.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, out Chute result)
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
	/// Parse the string into the target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The instance.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Chute Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parse the string into the target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The instance.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Chute Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));

	/// <summary>
	/// Parse the string into the target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The instance.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static Chute Parse(string s, CoordinateParser converter) => converter.ChuteParser(s)[0];
}
