namespace Sudoku.Analytics.Construction.Patterns;

/// <summary>
/// Defines a data pattern that describes an ALS.
/// </summary>
/// <param name="digitsMask"><inheritdoc cref="DigitsMask" path="/summary"/></param>
/// <param name="cells"><inheritdoc cref="Cells" path="/summary"/></param>
/// <param name="possibleEliminationMap"><inheritdoc cref="PossibleEliminationMap" path="/summary"/></param>
/// <param name="eliminationMap"><inheritdoc cref="EliminationMap" path="/summary"/></param>
/// <remarks>
/// An <b>Almost Locked Set</b> is a sudoku concept, which describes a case that
/// <c>n</c> cells contains <c>(n + 1)</c> kinds of different digits.
/// The special case is a bi-value cell.
/// </remarks>
public sealed class AlmostLockedSetPattern(Mask digitsMask, in CellMap cells, in CellMap possibleEliminationMap, CellMap[] eliminationMap) :
	Pattern,
	IComparable<AlmostLockedSetPattern>,
	IComparisonOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>
{
	/// <summary>
	/// Indicates an array of the total number of the strong relations in an ALS of the different size.
	/// The field is only unused in the property <see cref="StrongLinks"/>.
	/// </summary>
	/// <seealso cref="StrongLinks"/>
	private static readonly int[] StrongRelationsCount = [0, 1, 3, 6, 10, 15, 21, 28, 36, 45];


	/// <summary>
	/// Indicates whether the ALS only uses a bi-value cell.
	/// </summary>
	public bool IsBivalueCell => Cells.Count == 1;

	/// <inheritdoc/>
	public override bool IsChainingCompatible => true;

	/// <inheritdoc/>
	public override PatternType Type => PatternType.AlmostLockedSet;

	/// <summary>
	/// Indicates the mask of digits used.
	/// </summary>
	public Mask DigitsMask { get; } = digitsMask;

	/// <summary>
	/// Indicates the cells used.
	/// </summary>
	public CellMap Cells { get; } = cells;

	/// <summary>
	/// Gets the possible cells that can store eliminations for the ALS.
	/// </summary>
	public CellMap PossibleEliminationMap { get; } = possibleEliminationMap;

	/// <summary>
	/// The cells that can be eliminated, grouped by digit. The former 9 elements of the array is the cells
	/// that can be eliminated for the corresponding digit, and the last element is the merged cells.
	/// </summary>
	public CellMap[] EliminationMap { get; } = eliminationMap;

	/// <summary>
	/// Indicates the house used.
	/// </summary>
	public House House => Cells.FirstSharedHouse;

	/// <summary>
	/// Indicates all strong links in this ALS.
	/// The result will be represented as a <see cref="Mask"/> of 9 bits indicating which bits used.
	/// </summary>
	public ReadOnlySpan<Mask> StrongLinks
	{
		get
		{
			var digits = DigitsMask.AllSets;
			var result = new Mask[StrongRelationsCount[digits.Length - 1]];
			for (var (i, x, l) = (0, 0, digits.Length); i < l - 1; i++)
			{
				for (var j = i + 1; j < l; j++)
				{
					result[x++] = (Mask)(1 << digits[i] | 1 << digits[j]);
				}
			}
			return result;
		}
	}


	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out Mask digitsMask, out CellMap cells) => (digitsMask, cells) = (DigitsMask, Cells);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out Mask digitsMask, out CellMap cells, out CellMap possibleEliminationMap)
		=> ((digitsMask, cells), possibleEliminationMap) = (this, PossibleEliminationMap);

	/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
	public void Deconstruct(out Mask digitsMask, out CellMap cells, out CellMap possibleEliminationMap, out CellMap[] eliminationMap)
		=> ((digitsMask, cells, possibleEliminationMap), eliminationMap) = (this, EliminationMap);

	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] Pattern? other)
		=> other is AlmostLockedSetPattern comparer && DigitsMask == comparer.DigitsMask && Cells == comparer.Cells;

	/// <inheritdoc/>
	/// <exception cref="ArgumentNullException">Throws when the argument <paramref name="other"/> is <see langword="null"/>.</exception>
	public int CompareTo(AlmostLockedSetPattern? other)
		=> other is null
			? throw new ArgumentNullException(nameof(other))
			: Cells.Count.CompareTo(other.Cells.Count) is var p and not 0 ? p : Cells.CompareTo(other.Cells);

	/// <inheritdoc/>
	public override int GetHashCode() => HashCode.Combine(DigitsMask, Cells);

	/// <inheritdoc/>
	public override string ToString() => ToString(CoordinateConverter.InvariantCulture);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	public string ToString(CultureInfo culture) => ToString(CoordinateConverter.GetInstance(culture));

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string.</returns>
	public string ToString(CoordinateConverter converter)
	{
		var digitsStr = converter.DigitConverter(DigitsMask);
		var houseStr = converter.HouseConverter(1 << House);
		var cellsStr = converter.CellConverter(Cells);
		return IsBivalueCell
			? $"{digitsStr}/{cellsStr}"
			: $"{digitsStr}/{cellsStr} {SR.Get("KeywordIn", converter.CurrentCulture ?? CultureInfo.CurrentUICulture)} {houseStr}";
	}

	/// <inheritdoc/>
	public override AlmostLockedSetPattern Clone() => new(DigitsMask, Cells, PossibleEliminationMap, EliminationMap);


	/// <summary>
	/// Try to parse the string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out AlmostLockedSetPattern? result)
		=> TryParse(s, CoordinateParser.InvariantCulture, out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CultureInfo culture, [NotNullWhen(true)] out AlmostLockedSetPattern? result)
		=> TryParse(s, CoordinateParser.GetInstance(culture), out result);

	/// <summary>
	/// Try to parse the string into target instance, using the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, CoordinateParser converter, [NotNullWhen(true)] out AlmostLockedSetPattern? result)
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
		result = null;
		return false;
	}

	/// <summary>
	/// Collects all possible <see cref="AlmostLockedSetPattern"/>s in the specified grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <returns>All possible found <see cref="AlmostLockedSetPattern"/> instances.</returns>
	[MemoryCached]
	public static ReadOnlySpan<AlmostLockedSetPattern> Collect(in Grid grid)
	{
		// VARIABLE_DECLARATION_BEGIN
		_ = grid is { EmptyCells: var __EmptyCells, BivalueCells: var __BivalueCells, CandidatesMap: var __CandidatesMap };
		// VARIABLE_DECLARATION_END

		// Get all bi-value-cell ALSes.
		var result = new List<AlmostLockedSetPattern>();
		foreach (var cell in __BivalueCells)
		{
			var eliminationMap = new CellMap[9];
			foreach (var digit in grid.GetCandidates(cell))
			{
				eliminationMap[digit] = Peer.PeersMap[cell] & __CandidatesMap[digit];
			}

			result.Add(new(grid.GetCandidates(cell), cell.AsCellMap(), Peer.PeersMap[cell] & __EmptyCells, eliminationMap));
		}

		// Get all non-bi-value-cell ALSes.
		for (var house = 0; house < 27; house++)
		{
			if ((HousesMap[house] & __EmptyCells) is not { Count: >= 3 } tempMap)
			{
				continue;
			}

			for (var size = 2; size <= tempMap.Count - 1; size++)
			{
				foreach (ref readonly var map in tempMap & size)
				{
					var blockMask = map.BlockMask;
					if (BitOperations.IsPow2(blockMask) && house >= 9)
					{
						// All ALS cells lying on a box-row or a box-column will be processed as a block ALS.
						continue;
					}

					// Get all candidates in these cells.
					var digitsMask = grid[map];
					if (BitOperations.PopCount((uint)digitsMask) - 1 != size)
					{
						continue;
					}

					var eliminationMap = new CellMap[9];
					foreach (var digit in digitsMask)
					{
						eliminationMap[digit] = map % __CandidatesMap[digit];
					}

					var coveredLine = map.SharedLine;
					result.Add(
						new(
							digitsMask,
							map,
							house < 9 && coveredLine is >= 9 and not FallbackConstants.@int
								? (HousesMap[house] | HousesMap[coveredLine]) & __EmptyCells & ~map
								: tempMap & ~map,
							eliminationMap
						)
					);
				}
			}
		}
		return result.AsSpan();
	}

	/// <summary>
	/// Parses the current string into a valid instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The instance.</returns>
	public static AlmostLockedSetPattern Parse(string s) => Parse(s, CoordinateParser.InvariantCulture);

	/// <summary>
	/// Parses the current string into a valid instance, via the specified culture.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="culture">The culture.</param>
	/// <returns>The instance.</returns>
	public static AlmostLockedSetPattern Parse(string s, CultureInfo culture) => Parse(s, CoordinateParser.GetInstance(culture));

	/// <summary>
	/// Parses the current string into a valid instance, via the specified converter.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="converter">The converter.</param>
	/// <returns>The instance.</returns>
	public static AlmostLockedSetPattern Parse(string s, CoordinateParser converter)
	{
		const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;
		return s.Split('/', options) is [var digitsStr, var cellsStrAndHouseStr]
			? cellsStrAndHouseStr.Split(' ', options) is [var cellsStr, _, _]
				? new(converter.DigitParser(digitsStr), converter.CellParser(cellsStr), [], [])
				: throw new FormatException(SR.ExceptionMessage("AlsMissingCellsInTargetHouse"))
			: throw new FormatException(SR.ExceptionMessage("AlsMissingSlash"));
	}


	/// <inheritdoc/>
	public static bool operator >(AlmostLockedSetPattern left, AlmostLockedSetPattern right) => left.CompareTo(right) > 0;

	/// <inheritdoc/>
	public static bool operator <(AlmostLockedSetPattern left, AlmostLockedSetPattern right) => left.CompareTo(right) < 0;

	/// <inheritdoc/>
	public static bool operator >=(AlmostLockedSetPattern left, AlmostLockedSetPattern right) => left.CompareTo(right) >= 0;

	/// <inheritdoc/>
	public static bool operator <=(AlmostLockedSetPattern left, AlmostLockedSetPattern right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>.operator ==(AlmostLockedSetPattern? left, AlmostLockedSetPattern? right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<AlmostLockedSetPattern, AlmostLockedSetPattern, bool>.operator !=(AlmostLockedSetPattern? left, AlmostLockedSetPattern? right)
		=> left != right;
}
