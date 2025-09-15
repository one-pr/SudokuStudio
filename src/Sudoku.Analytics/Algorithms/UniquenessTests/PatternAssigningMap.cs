namespace Sudoku.Algorithms.UniquenessTests;

/// <summary>
/// Represents a map of cell and mask pairs, indicating the specified cells should only use specified digits.
/// </summary>
/// <remarks>
/// <para>
/// You can treat this type as a read-only dictionary of (cell, digits) pairs. You can construct a map using a collection expression:
/// <code><![CDATA[
/// PatternAssigningMap map = [
///     KeyValuePair.Create(11, (Mask)(1 << 0 | 1 << 8)), // r2c3(19)
///     KeyValuePair.Create(12, (Mask)(1 << 0 | 1 << 1)), // r2c4(12)
///     KeyValuePair.Create(16, (Mask)(1 << 1 | 1 << 8)), // r2c8(29)
///     KeyValuePair.Create(20, (Mask)(1 << 0 | 1 << 8)), // r3c3(19)
///     KeyValuePair.Create(21, (Mask)(1 << 0 | 1 << 1)), // r3c4(12)
///     KeyValuePair.Create(25, (Mask)(1 << 1 | 1 << 8)) // r3c8(29)
/// ];
/// ]]></code>
/// </para>
/// <para>
/// Instances of this type can be used as arguments from method <see cref="UniquenessChecker.CanLeadToDeadlyPatternContradiction"/>,
/// representing the digits can be checked and used for the specified cells.
/// For example, if we want to restrict cell <c>r2c3</c> only checks for digits 1 and 9,
/// we can construct a <see cref="KeyValuePair{TKey, TValue}"/> of cell <c>r2c3</c> (index 11) and digit 1 and 9
/// using method <see cref="KeyValuePair.Create"/>:
/// <code><![CDATA[
/// var kvp = KeyValuePair.Create(11, (Mask)(1 << 0 | 1 << 8)); // r2c3(19)
/// var kvp2 = new KeyValuePair<Cell, Mask>(11, (Mask)(1 << 0 | 1 << 8)); // also okay
/// ]]></code>
/// In this way, method <see cref="UniquenessChecker"/>.TryAssign will limit digits to be checked to only digits 1 and 9,
/// to optimize checking experience by reducing complexity.
/// </para>
/// </remarks>
/// <seealso cref="UniquenessChecker.CanLeadToDeadlyPatternContradiction(Candidate, PatternAssigningMap?, in Grid, in CellMap, out PatternTrialNode?)"/>
/// <seealso cref="KeyValuePair{TKey, TValue}"/>
/// <seealso cref="KeyValuePair.Create{TKey, TValue}(TKey, TValue)"/>
[CollectionBuilder(typeof(PatternAssigningMap), nameof(Create))]
public sealed partial class PatternAssigningMap : IEnumerable<KeyValuePair<Cell, Mask>>, IFormattable
{
	/// <summary>
	/// Indicates the backing mask table.
	/// </summary>
	private readonly Dictionary<Cell, Mask> _maskTable = [];


	/// <summary>
	/// Initializes a <see cref="PatternAssigningMap"/> instance.
	/// </summary>
	private PatternAssigningMap()
	{
	}


	/// <summary>
	/// Indicates the number of values.
	/// </summary>
	public int Count => _maskTable.Count;


	/// <summary>
	/// Lookups the current collection to get digits limited of the specified cell.
	/// </summary>
	/// <param name="cell">The cell specified.</param>
	/// <returns>The mask of digits.</returns>
	public Mask this[Cell cell] => _maskTable[cell];

	/// <summary>
	/// Determines whether the specified cell and digit exist in the current collection.
	/// </summary>
	/// <param name="cell">The cell.</param>
	/// <param name="digit">The digit.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public bool this[Cell cell, Digit digit] => _maskTable.TryGetValue(cell, out var mask) && (mask >> digit & 1) != 0;


	/// <inheritdoc/>
	public override string ToString() => ToString(null);

	/// <summary>
	/// Converts the current instance into string representation, using the specified coordinate converter instance.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string representation.</returns>
	public string ToString(CoordinateConverter? converter)
	{
		converter ??= new RxCyConverter();

		var parts = new List<string>();
		foreach (var (cell, digits) in from kvp in _maskTable orderby kvp.Key select kvp)
		{
			var cellString = converter.CellConverter([cell]);
			var digitsString = converter.DigitConverter(digits);
			parts.Add($"{cellString}: {digitsString}");
		}
		return $"[{string.Join(", ", parts)}]";
	}

	/// <inheritdoc cref="IFormattable.ToString(string?, IFormatProvider?)"/>
	public string ToString(IFormatProvider? formatProvider) => ToString(CoordinateConverter.GetInstance(formatProvider));

	/// <summary>
	/// Converts the current instance into a <see cref="CandidateMap"/> instance.
	/// </summary>
	/// <returns>The instance of type <see cref="CandidateMap"/>.</returns>
	public CandidateMap AsCandidateMap()
	{
		var result = CandidateMap.Empty;
		foreach (var (cell, mask) in _maskTable)
		{
			foreach (var digit in mask)
			{
				result += cell * 9 + digit;
			}
		}
		return result;
	}

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public Enumerator GetEnumerator() => new(_maskTable);

	/// <summary>
	/// Converts the current instance into a <see cref="FrozenDictionary{TKey, TValue}"/> of cell and mask pairs.
	/// </summary>
	/// <returns>The instance of type <see cref="FrozenDictionary{TKey, TValue}"/>.</returns>
	public FrozenDictionary<Cell, Mask> ToFrozenDictionary() => _maskTable.ToFrozenDictionary();

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => _maskTable.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<KeyValuePair<Cell, Mask>> IEnumerable<KeyValuePair<Cell, Mask>>.GetEnumerator() => _maskTable.GetEnumerator();


	/// <summary>
	/// Creates a <see cref="PatternAssigningMap"/> instance.
	/// </summary>
	/// <param name="values">The values.</param>
	/// <returns>The instance.</returns>
	[EditorBrowsable(EditorBrowsableState.Never)]
	public static PatternAssigningMap Create(params ReadOnlySpan<KeyValuePair<Cell, Mask>> values)
	{
		var result = new PatternAssigningMap();
		foreach (var (cell, digit) in values)
		{
			result._maskTable.Add(cell, digit);
		}
		return result;
	}
}
