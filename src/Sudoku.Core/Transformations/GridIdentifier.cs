namespace Sudoku.Transformations;

/// <summary>
/// Represents an identifier that describes the transforming cases.
/// </summary>
/// <remarks>
/// <para><include file="../../global-doc-comments.xml" path="/g/developer-notes"/></para>
/// <para>
/// This data type is used to represent an encoding of a transformation.
/// It stores a global mask derived from a Sudoku puzzle through a minimum lexicographic order (min-lex) mapping.
/// The backing identifier is <b>globally unique</b>: identical identifiers represent identical grids,
/// while different identifiers represent different grids.
/// </para>
/// <para>
/// We summarize the data structure into three parts:
/// <list type="number">
/// <item>The global identifier of the minimum lexicographic (min-lex) form</item>
/// <item>The transformation form</item>
/// <item>The list of empty cell positions</item>
/// </list>
/// </para>
/// <para>
/// There are approximately <b>5.4e9</b> inequivalent (essentially different, ED) Sudoku solutions in total.
/// By listing them in min-lex order, we can perform both sorting and deduplication. This part can be stored using 33 bits.
/// </para>
/// <para>
/// For the transformation form, we encode the following four types of transformations:
/// <list type="number">
/// <item><b>Transpose</b> - swap row and column indices (i.e., <c>(x, y) -> (y, x)</c>)</item>
/// <item><b>Row relabelling</b> - relabel rows 1-9 to target positions</item>
/// <item><b>Column relabelling</b> - relabel columns 1-9 to target positions</item>
/// <item><b>Digit relabelling</b> - relabel digits 1-9 to target positions</item>
/// </list>
/// </para>
/// <para>
/// All equivalent transformations of puzzles can be achieved using the above four types,
/// so this scheme can represent all transformation cases.
/// </para>
/// <para>
/// The total number of cases for these four transformations is 2, 1296, 1296, and 362880, respectively.
/// Multiplying them together gives the total number of possible cases, which is approximately <b>1.219e12</b>.
/// This value covers all possible transformation cases.
/// By storing the four transformation modes in bit form, the result can be represented by a single number, requiring 41 bits.
/// </para>
/// <para>
/// Finally, we need the list of empty cell positions. We only need 0 to represent an empty cell and 1 to represent a digit.
/// This part requires 81 bits.
/// </para>
/// <para>
/// Therefore, the entire data structure theoretically requires only <b>33 + 41 + 81 = 155</b> bits.
/// However, to align with bit size boundaries, this data structure uses the inline array feature and stores the data in 5 integers,
/// which can theoretically hold 160 bits.
/// </para>
/// </remarks>
public readonly struct GridIdentifier :
	IComparable<GridIdentifier>,
	IComparisonOperators<GridIdentifier, GridIdentifier, bool>,
	IEquatable<GridIdentifier>,
	IEqualityOperators<GridIdentifier, GridIdentifier, bool>
{
	/// <summary>
	/// Indicates the numebr of all transformations permutation cases.
	/// </summary>
	public const long AllPermutationsCount = GeometryPermutationsCount * RelabelDigitsPermutationsCount;

	/// <summary>
	/// Indicates the number of all inequivalent permutations (i.e. solutions).
	/// </summary>
	public const long InequivalentSolutionsCount = 5_472_730_538L;

	/// <summary>
	/// Indicates the number of deficiency permutations.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The reason that there is a deficiency of grids is due to the fact
	/// that some of the permutations create automorphic grids. Automorphic grids are the permutations which have duplicates.
	/// </para>
	/// <para>
	/// The total number of sudoku permutations is equal to <b>6,671,248,172,291,458,990,080</b> (approximately <b>6.67e21</b>),
	/// which is <b>5,472,730,538 * (2 * 1,296 * 1,296 * 362,880) - 344,420,270,386,053,120</b>,
	/// where all constants in this expression can be found in this type.
	/// </para>
	/// </remarks>
	public const long DeficiencyPermutationsCount = 344_420_270_386_053_120L;

	/// <summary>
	/// Indicates the number of geometry permutations.
	/// </summary>
	public const long GeometryPermutationsCount = RelabelLinesPermutationsCount * RelabelLinesPermutationsCount * TransposePermutationsCount;

	/// <summary>
	/// Indicates the number of relabelling row cases.
	/// </summary>
	public const long RelabelLinesPermutationsCount = 1_296;

	/// <summary>
	/// Indicates the number of transposing cases.
	/// </summary>
	public const long TransposePermutationsCount = 2;

	/// <summary>
	/// Indicates the number of relabelling digit cases.
	/// </summary>
	public const long RelabelDigitsPermutationsCount = 362_880;

	/// <summary>
	/// Indicates the number of required bits.
	/// </summary>
	private const int RequiredBitsCount = 155;

	/// <summary>
	/// Indicates the number of bits used in min-lex part.
	/// </summary>
	private const int MinlexPartBitsCount = 33;

	/// <summary>
	/// Indicates the number of bits used in transformation part.
	/// </summary>
	private const int TransformationPartBitsCount = 41;

	/// <summary>
	/// Indicates the number of bits used in cell states part.
	/// </summary>
	private const int CellStatePartBitsCount = 81;

	/// <summary>
	/// Indicates the shift amount of min-lex part.
	/// </summary>
	private const int MinlexPartShiftAmount = TransformationPartBitsCount + CellStatePartBitsCount;

	/// <summary>
	/// Indicates the shift amount of transformation part.
	/// </summary>
	private const int TransformationPartShiftAmount = CellStatePartBitsCount;

	/// <summary>
	/// Indicates the fixed serialization length.
	/// </summary>
	private const int FixedLength = 26;

	/// <summary>
	/// Indicates characters.
	/// </summary>
	private const string Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz{}";


	/// <summary>
	/// Indicates the backing elements.
	/// </summary>
	private readonly InlineArray5<int> _elements;


	/// <summary>
	/// Creates a <see cref="GridIdentifier"/> via a grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <exception cref="ArgumentException">Throws when the grid is invalid (not unique).</exception>
	public GridIdentifier(in Grid grid) :
		this(
			grid.SolutionGrid is { IsUndefined: false } solution
				? (BigInteger)MinlexRanker.GetRank(solution.ToString("0"), out var minlexGrid, out var transform) << MinlexPartShiftAmount
					| (BigInteger)(long)transform << TransformationPartShiftAmount
					| grid.GivenCells.Aggregate(BigInteger.Zero, static (interim, next) => interim | BigInteger.One << next)
				: throw new ArgumentException(SR.ExceptionMessage("GridInvalid"), nameof(grid))
		)
	{
	}

	/// <summary>
	/// Initializes a <see cref="GridIdentifier"/> instance.
	/// </summary>
	/// <param name="value">The value.</param>
	private GridIdentifier(BigInteger value)
	{
		for (var i = 0; i < RequiredBitsCount; i++)
		{
			var index = i >> 5;
			var position = i & (1 << 5) - 1;
			if ((value >>> i & 1) != 0)
			{
				_elements[index] |= 1 << position;
			}
		}
	}


	/// <summary>
	/// Indicates the number of global index.
	/// </summary>
	public ulong Index
	{
		get
		{
			var bits = GetSlice(MinlexPartShiftAmount, MinlexPartBitsCount);
			var result = 0UL;
			for (var i = 0; i < MinlexPartBitsCount; i++)
			{
				if (bits[i])
				{
					result |= 1UL << i;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates given cells.
	/// </summary>
	public CellMap GivenCells
	{
		get
		{
			var bits = GetSlice(0, CellStatePartBitsCount);
			var result = CellMap.Empty;
			for (var i = 0; i < CellStatePartBitsCount; i++)
			{
				if (bits[i])
				{
					result += i;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates target tranform.
	/// </summary>
	public Transform Transform
	{
		get
		{
			var bits = GetSlice(TransformationPartShiftAmount, TransformationPartBitsCount);
			var mask = BigInteger.Zero;
			var one = BigInteger.One;
			for (var i = 0; i < TransformationPartBitsCount; i++)
			{
				if (bits[i])
				{
					mask |= one << i;
				}
			}
			return new((long)mask);
		}
	}

	/// <summary>
	/// Represents identifier value.
	/// </summary>
	public BigInteger IdentifierValue
	{
		get
		{
			var result = BigInteger.Zero;
			var one = BigInteger.One;
			for (var i = 0; i < RequiredBitsCount; i++)
			{
				var index = i >> 5;
				var position = i & (1 << 5) - 1;
				if ((_elements[index] >>> position & 1) != 0)
				{
					result |= one << i;
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates the original grid.
	/// </summary>
	public Grid OriginalGrid => Grid.Parse(MinlexRanker.GetGrid(Index));

	/// <summary>
	/// Indicates the target grid applied such transformations.
	/// </summary>
	public Grid TargetGrid
	{
		get
		{
			var g = OriginalGrid;
			g.Apply(Transform);
			return g.Preserve(GivenCells);
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is GridIdentifier comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in GridIdentifier other) => _elements[..].SequenceEqual(other._elements);

	/// <inheritdoc/>
	public override int GetHashCode() => IdentifierValue.GetHashCode();

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public int CompareTo(in GridIdentifier other) => IdentifierValue.CompareTo(other.IdentifierValue);

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
	{
		var value = IdentifierValue;
		if (value == BigInteger.Zero)
		{
			return new(Characters[0], FixedLength);
		}

		var sb = new StringBuilder();
		while (value != BigInteger.Zero)
		{
			value = BigInteger.DivRem(value, 64, out var remainder);
			sb.Insert(0, Characters[(int)remainder]);
		}

		// Pad with leading zeros to ensure fixed length.
		return sb.ToString().PadLeft(FixedLength, Characters[0]);
	}

	/// <inheritdoc/>
	bool IEquatable<GridIdentifier>.Equals(GridIdentifier other) => Equals(other);

	/// <inheritdoc/>
	int IComparable<GridIdentifier>.CompareTo(GridIdentifier other) => CompareTo(other);

	/// <summary>
	/// Gets a <see cref="BitArray"/> instance that displays the slice.
	/// </summary>
	/// <param name="startIndex">The start index.</param>
	/// <param name="count">The number of bits.</param>
	/// <returns>A <see cref="BitArray"/> instance.</returns>
	private BitArray GetSlice(int startIndex, int count)
	{
		var result = new BitArray(count);
		for (var (i, j) = (startIndex, 0); j < count; i++, j++)
		{
			var index = i >> 5;
			var position = i & (1 << 5) - 1;
			result[j] = (_elements[index] >>> position & 1) != 0;
		}
		return result;
	}


	/// <summary>
	/// Try to parse the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result.</returns>
	public static bool TryParse([NotNullWhen(true)] string? s, out GridIdentifier result)
	{
		try
		{
			if (s is null)
			{
				result = default;
				return false;
			}

			result = Parse(s);
			return true;
		}
		catch (FormatException)
		{
			result = default;
			return false;
		}
	}

	/// <summary>
	/// Try to parse the specified string into target instance.
	/// </summary>
	/// <param name="s">The string.</param>
	/// <returns>The result.</returns>
	/// <exception cref="FormatException">Throws when invalid characters encountered.</exception>
	public static GridIdentifier Parse(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			throw new ArgumentException("Input string cannot be null or empty.");
		}
		if (s.Length != FixedLength)
		{
			throw new ArgumentException($"Input string must be exactly {FixedLength} characters.");
		}

		var result = BigInteger.Zero;
		foreach (var ch in s)
		{
			var index = Characters.IndexOf(ch);
			if (index < 0)
			{
				throw new ArgumentException($"Invalid character '{ch}' in input string.");
			}
			result = (result << 6) + index;
		}
		return new(result);
	}


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in GridIdentifier left, in GridIdentifier right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in GridIdentifier left, in GridIdentifier right) => !(left == right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in GridIdentifier left, in GridIdentifier right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in GridIdentifier left, in GridIdentifier right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in GridIdentifier left, in GridIdentifier right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in GridIdentifier left, in GridIdentifier right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	static bool IEqualityOperators<GridIdentifier, GridIdentifier, bool>.operator ==(GridIdentifier left, GridIdentifier right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<GridIdentifier, GridIdentifier, bool>.operator !=(GridIdentifier left, GridIdentifier right)
		=> left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GridIdentifier, GridIdentifier, bool>.operator >(GridIdentifier left, GridIdentifier right)
		=> left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GridIdentifier, GridIdentifier, bool>.operator <(GridIdentifier left, GridIdentifier right)
		=> left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GridIdentifier, GridIdentifier, bool>.operator >=(GridIdentifier left, GridIdentifier right)
		=> left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GridIdentifier, GridIdentifier, bool>.operator <=(GridIdentifier left, GridIdentifier right)
		=> left <= right;


	/// <summary>
	/// Explicit cast from <see cref="BigInteger"/> to <see cref="GridIdentifier"/>.
	/// </summary>
	/// <param name="identifierValue">The identifier value.</param>
	public static explicit operator GridIdentifier(BigInteger identifierValue)
		=> new(Math.UnsignedMod(identifierValue, (BigInteger)AllPermutationsCount * InequivalentSolutionsCount));

	/// <summary>
	/// Explicit cast from <see cref="BigInteger"/> to <see cref="Transformations.Transform"/>, with range check.
	/// </summary>
	/// <param name="identifierValue">The identifier value.</param>
	/// <exception cref="OverflowException">Throws when <paramref name="identifierValue"/> is invalid.</exception>
	public static explicit operator checked GridIdentifier(BigInteger identifierValue)
		=> new(
			identifierValue >= 0 && identifierValue < (BigInteger)AllPermutationsCount * InequivalentSolutionsCount
				? identifierValue
				: throw new OverflowException()
		);

	/// <summary>
	/// Implicit cast from <see cref="GridIdentifier"/> to <see cref="BigInteger"/>.
	/// </summary>
	/// <param name="transform">The transform.</param>
	public static implicit operator BigInteger(GridIdentifier transform) => transform.IdentifierValue;
}
