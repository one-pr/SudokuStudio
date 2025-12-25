namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Represents an option provider for coordinates.
/// </summary>
/// <param name="DefaultSeparator">
/// <para>Indicates the default separator. The value will be inserted into two non-digit-kind instances.</para>
/// <para>The value is <c>", "</c> by default.</para>
/// </param>
/// <param name="DigitsSeparator">
/// <para>Indicates the digits separator.</para>
/// <para>The value is <see langword="null"/> by default, meaning no separators will be inserted between 2 digits.</para>
/// </param>
/// <param name="AssignmentToken">
/// <para>Indicates the token that describes an assignment conclusion, connected with cell and digit.</para>
/// <para>The value is <c>" = "</c> by default.</para>
/// </param>
/// <param name="EliminationToken">
/// <para>Indicates the token that describes an elimination conclusion, connected with cell and digit.</para>
/// <para>The value is <c>"<![CDATA[ <> ]]>"</c> by default.</para>
/// </param>
/// <param name="NotationBracket">
/// <para>Indicates the bracket surrounding the multiple coordinate parts (especially for cell groups and candidate groups).</para>
/// <para>The value is <see cref="NotationBracket.None"/> by default.</para>
/// </param>
/// <param name="CurrentCulture">
/// <para>Indicates the current culture.</para>
/// <para>The value is <see langword="null"/> by default, meaning the converter uses invariant culture to output some string text.</para>
/// </param>
/// <remarks>
/// You can use types <see cref="RxCyConverter"/>, <seealso cref="K9Converter"/>, <see cref="LiteralCoordinateConverter"/>
/// and <see cref="ExcelCoordinateConverter"/>.
/// They are the derived types of the current type.
/// </remarks>
/// <seealso cref="RxCyConverter"/>
/// <seealso cref="K9Converter"/>
/// <seealso cref="LiteralCoordinateConverter"/>
/// <seealso cref="ExcelCoordinateConverter"/>
public abstract record CoordinateConverter(
	string DefaultSeparator = ", ",
	string? DigitsSeparator = null,
	string AssignmentToken = " = ",
	string EliminationToken = " <> ",
	NotationBracket NotationBracket = NotationBracket.None,
	CultureInfo? CurrentCulture = null
) :
	ICoordinateProvider<CoordinateConverter>,
	ICellConvertible,
	ICandidateConvertible,
	IHouseConvertible,
	IConclusionConvertible,
	IDigitConvertible,
	IIntersectionConvertible,
	IChuteConvertible,
	IConjugatePairConvertible
{
	/// <inheritdoc/>
	public abstract CellMapFormatter CellConverter { get; }

	/// <inheritdoc/>
	public abstract CandidateMapFormatter CandidateConverter { get; }

	/// <inheritdoc/>
	public abstract Func<HouseMask, string> HouseConverter { get; }

	/// <inheritdoc/>
	public abstract Func<ReadOnlySpan<Conclusion>, string> ConclusionConverter { get; }

	/// <inheritdoc/>
	public abstract Func<Mask, string> DigitConverter { get; }

	/// <inheritdoc/>
	public abstract Func<ReadOnlySpan<Miniline>, string> IntersectionConverter { get; }

	/// <inheritdoc/>
	public abstract Func<ReadOnlySpan<Chute>, string> ChuteConverter { get; }

	/// <inheritdoc/>
	public abstract Func<ReadOnlySpan<Conjugate>, string> ConjugateConverter { get; }

	/// <summary>
	/// Indicates the target culture.
	/// </summary>
	private protected CultureInfo TargetCurrentCulture => CurrentCulture ?? CultureInfo.CurrentUICulture;


	/// <inheritdoc/>
	public static CoordinateConverter InvariantCulture => new RxCyConverter();


	/// <inheritdoc/>
	public static CoordinateConverter GetInstance(CultureInfo? culture)
		=> culture switch
		{
			{ IsChinese: true } => new K9Converter(true, CurrentCulture: culture),
			{ IsEnglish: true } => new RxCyConverter(true, true, CurrentCulture: culture),
			_ => InvariantCulture
		};
}
