namespace Sudoku.Shuffling;

/// <summary>
/// Represents a generic transform that includes the ranks of morphing types.
/// </summary>
/// <param name="TransposeRank">Indicates the rank of transpose.</param>
/// <param name="RelabeledRowsRank">Indicates the rank of relabeled rows.</param>
/// <param name="RelabeledColumnsRank">Indicates the rank of relabeled columns.</param>
/// <param name="RelabeledDigitsRank">Indicates the rank of relabeled digits.</param>
public readonly record struct GenericTransform(int TransposeRank, int RelabeledRowsRank, int RelabeledColumnsRank, int RelabeledDigitsRank) :
	IComparable<GenericTransform>,
	IComparisonOperators<GenericTransform, GenericTransform, bool>,
	IEqualityOperators<GenericTransform, GenericTransform, bool>
{
	/// <summary>
	/// Represents equivalent transform.
	/// </summary>
	public static readonly GenericTransform Equivalent = new(0, 0, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 90 degrees clockwise.
	/// </summary>
	public static readonly GenericTransform ClockwiseRotate90Degrees = new(1, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 180 degrees clockwise.
	/// </summary>
	public static readonly GenericTransform ClockwiseRotate180Degrees = new(0, ^1, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 270 degrees clockwise.
	/// </summary>
	public static readonly GenericTransform ClockwiseRotate270Degrees = new(1, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 90 degrees counterclockwise.
	/// </summary>
	public static readonly GenericTransform CounterclockwiseRotate90Degrees = new(1, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 180 degrees counterclockwise.
	/// </summary>
	public static readonly GenericTransform CounterclockwiseRotate180Degrees = new(0, ^1, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 270 degrees counterclockwise.
	/// </summary>
	public static readonly GenericTransform CounterclockwiseRotate270Degrees = new(1, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that mirrors left-right side.
	/// </summary>
	public static readonly GenericTransform MirrorLeftRight = new(0, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that mirrors top-bottom side.
	/// </summary>
	public static readonly GenericTransform MirrorTopBottom = new(0, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that mirrors diagonal.
	/// </summary>
	public static readonly GenericTransform MirrorDiagonal = new(1, 0, 0, 0);

	/// <summary>
	/// Represents a transform that mirrors anti-diagonal.
	/// </summary>
	public static readonly GenericTransform MirrorAntidiagonal = new(1, ^1, ^1, 0);

	/// <summary>
	/// Represents a transpose transform.
	/// </summary>
	public static readonly GenericTransform Transpose = new(1, 0, 0, 0);


	/// <summary>
	/// Initializes a <see cref="GenericTransform"/> instance.
	/// </summary>
	/// <param name="transposeRank">
	/// <inheritdoc cref="GenericTransform(int, int, int, int)" path="/param[@name='TransposeRank']"/>
	/// </param>
	/// <param name="relabeledRowsRank">
	/// <inheritdoc cref="GenericTransform(int, int, int, int)" path="/param[@name='RelabeledRowsRank']"/>
	/// </param>
	/// <param name="relabeledColumnsRank">
	/// <inheritdoc cref="GenericTransform(int, int, int, int)" path="/param[@name='RelabeledColumnsRank']"/>
	/// </param>
	/// <param name="relabeledDigitsRank">
	/// <inheritdoc cref="GenericTransform(int, int, int, int)" path="/param[@name='RelabeledDigitsRank']"/>
	/// </param>
	public GenericTransform(int transposeRank, Index relabeledRowsRank, Index relabeledColumnsRank, Index relabeledDigitsRank) :
		this(
			transposeRank,
			relabeledRowsRank.GetOffset((int)GridTransformIdentifier.RelabelLinesPermutationsCount),
			relabeledColumnsRank.GetOffset((int)GridTransformIdentifier.RelabelLinesPermutationsCount),
			relabeledDigitsRank.GetOffset((int)GridTransformIdentifier.RelabelDigitsPermutationsCount)
		)
	{
	}

	/// <summary>
	/// Initializes a <see cref="GenericTransform"/> via the base-mixed rank.
	/// </summary>
	/// <param name="baseMixedRank">The base-mixed rank.</param>
	public GenericTransform(long baseMixedRank) :
		this(
			(int)(
				baseMixedRank
					/ GridTransformIdentifier.RelabelDigitsPermutationsCount
					/ GridTransformIdentifier.RelabelLinesPermutationsCount
					/ GridTransformIdentifier.RelabelLinesPermutationsCount
					% GridTransformIdentifier.TransposePermutationsCount
			),
			(int)(
				baseMixedRank
					/ GridTransformIdentifier.RelabelDigitsPermutationsCount
					/ GridTransformIdentifier.RelabelLinesPermutationsCount
					% GridTransformIdentifier.RelabelLinesPermutationsCount
			),
			(int)(
				baseMixedRank
					/ GridTransformIdentifier.RelabelDigitsPermutationsCount
					% GridTransformIdentifier.RelabelLinesPermutationsCount
			),
			(int)(baseMixedRank % GridTransformIdentifier.RelabelDigitsPermutationsCount)
		)
	{
	}


	/// <summary>
	/// Indicates whether to transpose.
	/// </summary>
	public bool ShouldTranspose => TransposeRank == 1;

	/// <summary>
	/// Indicates the base-mixed rank.
	/// </summary>
	public long BaseMixedRank
		=> TransposeRank * GridTransformIdentifier.RelabelLinesPermutationsCount * GridTransformIdentifier.RelabelLinesPermutationsCount * GridTransformIdentifier.RelabelDigitsPermutationsCount
		+ RelabeledRowsRank * GridTransformIdentifier.RelabelLinesPermutationsCount * GridTransformIdentifier.RelabelDigitsPermutationsCount
		+ RelabeledColumnsRank * GridTransformIdentifier.RelabelDigitsPermutationsCount
		+ RelabeledDigitsRank;

	/// <summary>
	/// Represents a value that displays relabeled row indices.
	/// </summary>
	public ReadOnlySpan<RowIndex> RowIndicesRelabeled => CantorExpansion.UnrankRelabeledLines(RelabeledRowsRank);

	/// <summary>
	/// Represents a value that displays relabeled column indices.
	/// </summary>
	public ReadOnlySpan<ColumnIndex> ColumnIndicesRelabeled => CantorExpansion.UnrankRelabeledLines(RelabeledColumnsRank);

	/// <summary>
	/// Represents a value that displays relabeled digits.
	/// </summary>
	public ReadOnlySpan<Digit> DigitsRelabeled => CantorExpansion.UnrankRelabeledDigits(RelabeledDigitsRank, SpanEnumerable.Range(9));


	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public int CompareTo(in GenericTransform other) => BaseMixedRank.CompareTo(other.BaseMixedRank);

	/// <inheritdoc/>
	int IComparable<GenericTransform>.CompareTo(GenericTransform other) => CompareTo(other);


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(in GenericTransform left, in GenericTransform right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(in GenericTransform left, in GenericTransform right) => !(left == right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in GenericTransform left, in GenericTransform right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in GenericTransform left, in GenericTransform right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in GenericTransform left, in GenericTransform right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in GenericTransform left, in GenericTransform right) => left.CompareTo(right) <= 0;

	/// <inheritdoc/>
	static bool IEqualityOperators<GenericTransform, GenericTransform, bool>.operator ==(GenericTransform left, GenericTransform right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<GenericTransform, GenericTransform, bool>.operator !=(GenericTransform left, GenericTransform right)
		=> left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GenericTransform, GenericTransform, bool>.operator >(GenericTransform left, GenericTransform right)
		=> left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GenericTransform, GenericTransform, bool>.operator <(GenericTransform left, GenericTransform right)
		=> left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GenericTransform, GenericTransform, bool>.operator >=(GenericTransform left, GenericTransform right)
		=> left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<GenericTransform, GenericTransform, bool>.operator <=(GenericTransform left, GenericTransform right)
		=> left <= right;


	/// <summary>
	/// Explicit cast from <see cref="long"/> to <see cref="GenericTransform"/>.
	/// </summary>
	/// <param name="baseMixedRank">The base-mixed rank.</param>
	public static explicit operator GenericTransform(long baseMixedRank)
		=> new(Math.Abs(baseMixedRank) % GridTransformIdentifier.AllPermutationsCount);

	/// <summary>
	/// Explicit cast from <see cref="long"/> to <see cref="GenericTransform"/>, with range check.
	/// </summary>
	/// <param name="baseMixedRank">The base-mixed rank.</param>
	/// <exception cref="OverflowException">Throws when <paramref name="baseMixedRank"/> is invalid.</exception>
	public static explicit operator checked GenericTransform(long baseMixedRank)
		=> new(
			baseMixedRank is >= 0 and < GridTransformIdentifier.AllPermutationsCount
				? baseMixedRank
				: throw new OverflowException()
		);

	/// <summary>
	/// Implicit cast from <see cref="GenericTransform"/> to <see cref="long"/>.
	/// </summary>
	/// <param name="transform">The transform.</param>
	public static implicit operator long(GenericTransform transform) => transform.BaseMixedRank;
}
