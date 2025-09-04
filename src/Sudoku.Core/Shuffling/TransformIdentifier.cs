namespace Sudoku.Shuffling;

/// <summary>
/// Represents an identifier that describes the transforming cases.
/// </summary>
/// <remarks>
/// <para><include file="../../global-doc-comments.xml" path="/g/developer-notes"/></para>
/// <para>
/// This data type is used to represent an encoding of a transformation.
/// It stores a global code derived from a Sudoku puzzle through a minimum lexicographic order (min-lex) mapping.
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
/// <item><b>Row remapping</b> - relabel rows 1–9 to target positions</item>
/// <item><b>Column remapping</b> - relabel columns 1–9 to target positions</item>
/// <item><b>Digit remapping</b> - relabel digits 1–9 to target positions</item>
/// </list>
/// </para>
/// <para>
/// All equivalent transformations of Sudoku puzzles can be achieved using the above four types,
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
public readonly struct TransformIdentifier :
	IComparable<TransformIdentifier>,
	IComparisonOperators<TransformIdentifier, TransformIdentifier, bool>,
	IEquatable<TransformIdentifier>,
	IEqualityOperators<TransformIdentifier, TransformIdentifier, bool>
{
	/// <summary>
	/// Indicates the numebr of all transformations permutation cases.
	/// </summary>
	public const long AllTransformationsPermutationsCount = (long)TransposePermutationsCount
		* RemapRowsPermutationsCount
		* RemapColumnsPermutationsCount
		* RelabelDigitsPermutationsCount;

	/// <summary>
	/// Indicates the number of all inequivalent solutions.
	/// </summary>
	public const long InequivalentSolutionsCount = 5_472_730_538L;

	/// <summary>
	/// Indicates the number of remapping row cases.
	/// </summary>
	internal const int RemapRowsPermutationsCount = 1_296;

	/// <summary>
	/// Indicates the number of remapping column cases.
	/// </summary>
	internal const int RemapColumnsPermutationsCount = RemapRowsPermutationsCount;

	/// <summary>
	/// Indicates the number of required bits.
	/// </summary>
	private const int RequiredBitsCount = 155;

	/// <summary>
	/// Indicates the number of transposing cases.
	/// </summary>
	private const int TransposePermutationsCount = 2;

	/// <summary>
	/// Indicates the number of relabelling digit cases.
	/// </summary>
	private const int RelabelDigitsPermutationsCount = 362_880;

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
	/// Indicates the backing elements.
	/// </summary>
	private readonly InlineArray5<int> _elements;


	/// <summary>
	/// Indicates whether to transpose.
	/// </summary>
	public bool ShouldTranspose => TransformationMasks.ShouldTranspose;

	/// <summary>
	/// Indicates the number of global min-lex index.
	/// </summary>
	public long GlobalMinlexIndex
	{
		get
		{
			var bits = GetSlice(MinlexPartShiftAmount, MinlexPartBitsCount);
			var result = 0L;
			for (var i = 0; i < MinlexPartBitsCount; i++)
			{
				if (bits[i])
				{
					result |= 1L << i;
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
					result.Add(i);
				}
			}
			return result;
		}
	}

	/// <summary>
	/// Represents a value that displays relabeled row indices.
	/// </summary>
	public ReadOnlySpan<RowIndex> RowIndicesRelabeled => CantorExpansion.UnrankLine(TransformationMasks.RelabeledRows);

	/// <summary>
	/// Represents a value that displays relabeled column indices.
	/// </summary>
	public ReadOnlySpan<ColumnIndex> ColumnIndicesRelabeled => CantorExpansion.UnrankLine(TransformationMasks.RelabeledColumns);

	/// <summary>
	/// Represents a value that displayes relabeled digits.
	/// </summary>
	public ReadOnlySpan<Digit> DigitsRelabeled
		=> CantorExpansion.UnrankDigit(TransformationMasks.RelabeledDigits, [0, 1, 2, 3, 4, 5, 6, 7, 8]);

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
	/// Indicates the quadruple of tranformation masks.
	/// </summary>
	private (bool ShouldTranspose, int RelabeledRows, int RelabeledColumns, int RelabeledDigits) TransformationMasks
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

			var relabelDigits = mask % RelabelDigitsPermutationsCount;
			var remapColumns = mask / RelabelDigitsPermutationsCount % RemapColumnsPermutationsCount;
			var remapRows = mask / RelabelDigitsPermutationsCount / RemapColumnsPermutationsCount % RemapRowsPermutationsCount;
			var shouldTranspose = mask / RelabelDigitsPermutationsCount / RemapColumnsPermutationsCount / RemapRowsPermutationsCount % 2;
			return (shouldTranspose != 0, (int)remapRows, (int)remapColumns, (int)relabelDigits);
		}
	}


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] object? obj) => obj is TransformIdentifier comparer && Equals(comparer);

	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	public bool Equals(in TransformIdentifier other) => _elements[..].SequenceEqual(other._elements);

	/// <inheritdoc/>
	public override int GetHashCode() => IdentifierValue.GetHashCode();

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public int CompareTo(in TransformIdentifier other) => IdentifierValue.CompareTo(other.IdentifierValue);

	/// <inheritdoc/>
	bool IEquatable<TransformIdentifier>.Equals(TransformIdentifier other) => Equals(other);

	/// <inheritdoc/>
	int IComparable<TransformIdentifier>.CompareTo(TransformIdentifier other) => CompareTo(other);

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


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in TransformIdentifier left, in TransformIdentifier right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in TransformIdentifier left, in TransformIdentifier right) => !(left == right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in TransformIdentifier left, in TransformIdentifier right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in TransformIdentifier left, in TransformIdentifier right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in TransformIdentifier left, in TransformIdentifier right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in TransformIdentifier left, in TransformIdentifier right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	static bool IEqualityOperators<TransformIdentifier, TransformIdentifier, bool>.operator ==(TransformIdentifier left, TransformIdentifier right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<TransformIdentifier, TransformIdentifier, bool>.operator !=(TransformIdentifier left, TransformIdentifier right)
		=> left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TransformIdentifier, TransformIdentifier, bool>.operator >(TransformIdentifier left, TransformIdentifier right)
		=> left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TransformIdentifier, TransformIdentifier, bool>.operator <(TransformIdentifier left, TransformIdentifier right)
		=> left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TransformIdentifier, TransformIdentifier, bool>.operator >=(TransformIdentifier left, TransformIdentifier right)
		=> left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<TransformIdentifier, TransformIdentifier, bool>.operator <=(TransformIdentifier left, TransformIdentifier right)
		=> left <= right;
}
