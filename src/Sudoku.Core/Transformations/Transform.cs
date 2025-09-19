namespace Sudoku.Transformations;

/// <summary>
/// Represents a generic transform that includes the ranks of morphing types.
/// </summary>
/// <param name="TransposeRank">Indicates the rank of transpose.</param>
/// <param name="RelabeledRowsRank">Indicates the rank of relabeled rows.</param>
/// <param name="RelabeledColumnsRank">Indicates the rank of relabeled columns.</param>
/// <param name="RelabeledDigitsRank">Indicates the rank of relabeled digits.</param>
/// <completionlist cref="Transform"/>
public readonly partial record struct Transform(
	int TransposeRank,
	int RelabeledRowsRank,
	int RelabeledColumnsRank,
	int RelabeledDigitsRank
) :
	IBitwiseOperators<Transform, Transform, Transform>,
	IComparable<Transform>,
	IComparisonOperators<Transform, Transform, bool>,
	IEqualityOperators<Transform, Transform, bool>,
	ITransformable<Transform>
{
	/// <summary>
	/// Represents equivalent transform (no transform).
	/// </summary>
	public static readonly Transform Equivalent = new(0, 0, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 90 degrees clockwise.
	/// </summary>
	public static readonly Transform ClockwiseRotate90Degrees = new(1, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 180 degrees clockwise.
	/// </summary>
	public static readonly Transform ClockwiseRotate180Degrees = new(0, ^1, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 270 degrees clockwise.
	/// </summary>
	public static readonly Transform ClockwiseRotate270Degrees = new(1, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 90 degrees counterclockwise.
	/// </summary>
	public static readonly Transform CounterclockwiseRotate90Degrees = new(1, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that rotates 180 degrees counterclockwise.
	/// </summary>
	public static readonly Transform CounterclockwiseRotate180Degrees = new(0, ^1, ^1, 0);

	/// <summary>
	/// Represents a transform that rotates 270 degrees counterclockwise.
	/// </summary>
	public static readonly Transform CounterclockwiseRotate270Degrees = new(1, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that mirrors left-right side.
	/// </summary>
	public static readonly Transform MirrorLeftRight = new(0, ^1, 0, 0);

	/// <summary>
	/// Represents a transform that mirrors top-bottom side.
	/// </summary>
	public static readonly Transform MirrorTopBottom = new(0, 0, ^1, 0);

	/// <summary>
	/// Represents a transform that mirrors diagonal.
	/// </summary>
	public static readonly Transform MirrorDiagonal = new(1, 0, 0, 0);

	/// <summary>
	/// Represents a transform that mirrors anti-diagonal.
	/// </summary>
	public static readonly Transform MirrorAntidiagonal = new(1, ^1, ^1, 0);


	/// <summary>
	/// Initializes a <see cref="Transform"/> instance.
	/// </summary>
	/// <param name="transposeRank">
	/// <inheritdoc cref="Transform(int, int, int, int)" path="/param[@name='TransposeRank']"/>
	/// </param>
	/// <param name="relabeledRowsRank">
	/// <inheritdoc cref="Transform(int, int, int, int)" path="/param[@name='RelabeledRowsRank']"/>
	/// </param>
	/// <param name="relabeledColumnsRank">
	/// <inheritdoc cref="Transform(int, int, int, int)" path="/param[@name='RelabeledColumnsRank']"/>
	/// </param>
	/// <param name="relabeledDigitsRank">
	/// <inheritdoc cref="Transform(int, int, int, int)" path="/param[@name='RelabeledDigitsRank']"/>
	/// </param>
	public Transform(int transposeRank, Index relabeledRowsRank, Index relabeledColumnsRank, Index relabeledDigitsRank) :
		this(
			transposeRank,
			relabeledRowsRank.GetOffset((int)GridIdentifier.RelabelLinesPermutationsCount),
			relabeledColumnsRank.GetOffset((int)GridIdentifier.RelabelLinesPermutationsCount),
			relabeledDigitsRank.GetOffset((int)GridIdentifier.RelabelDigitsPermutationsCount)
		)
	{
	}

	/// <summary>
	/// Initializes a <see cref="Transform"/> via the base-mixed rank.
	/// </summary>
	/// <param name="rank">The base-mixed rank.</param>
	public Transform(long rank) :
		this(
			(int)(
				rank
					/ GridIdentifier.RelabelDigitsPermutationsCount
					/ GridIdentifier.RelabelLinesPermutationsCount
					/ GridIdentifier.RelabelLinesPermutationsCount
					% GridIdentifier.TransposePermutationsCount
			),
			(int)(
				rank
					/ GridIdentifier.RelabelDigitsPermutationsCount
					/ GridIdentifier.RelabelLinesPermutationsCount
					% GridIdentifier.RelabelLinesPermutationsCount
			),
			(int)(
				rank
					/ GridIdentifier.RelabelDigitsPermutationsCount
					% GridIdentifier.RelabelLinesPermutationsCount
			),
			(int)(rank % GridIdentifier.RelabelDigitsPermutationsCount)
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
	public long Rank
		=> TransposeRank
			* GridIdentifier.RelabelLinesPermutationsCount
			* GridIdentifier.RelabelLinesPermutationsCount
			* GridIdentifier.RelabelDigitsPermutationsCount
			+ RelabeledRowsRank
			* GridIdentifier.RelabelLinesPermutationsCount
			* GridIdentifier.RelabelDigitsPermutationsCount
			+ RelabeledColumnsRank
			* GridIdentifier.RelabelDigitsPermutationsCount
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
	public ReadOnlySpan<Digit> DigitsRelabeled => CantorExpansion.UnrankRelabeledDigits(RelabeledDigitsRank);


	/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
	[OverloadResolutionPriority(1)]
	public bool Equals(in Transform other) => Rank == other.Rank;

	/// <inheritdoc cref="IComparable{T}.CompareTo(T)"/>
	public int CompareTo(in Transform other) => Rank.CompareTo(other.Rank);

	/// <inheritdoc/>
	int IComparable<Transform>.CompareTo(Transform other) => CompareTo(other);

	/// <inheritdoc/>
	Transform ITransformable<Transform>.RotateClockwise() => this | ClockwiseRotate90Degrees;

	/// <inheritdoc/>
	Transform ITransformable<Transform>.MirrorLeftRight() => this | MirrorLeftRight;

	/// <inheritdoc/>
	Transform ITransformable<Transform>.MirrorTopBottom() => this | MirrorTopBottom;

	/// <inheritdoc/>
	Transform ITransformable<Transform>.MirrorDiagonal() => this | MirrorDiagonal;

	/// <inheritdoc/>
	Transform ITransformable<Transform>.MirrorAntidiagonal() => this | MirrorAntidiagonal;

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		builder.Append($"{nameof(TransposeRank)} = {TransposeRank} ({TransposeRank != 0}), ");
		builder.Append($"{nameof(RelabeledRowsRank)} = {RelabeledRowsRank}");
		builder.Append($" ({+(from r in RowIndicesRelabeled select (r + 1).ToString())}), ");
		builder.Append($"{nameof(RelabeledColumnsRank)} = {RelabeledColumnsRank}");
		builder.Append($" ({+(from c in ColumnIndicesRelabeled select (c + 1).ToString())}), ");
		builder.Append($"{nameof(RelabeledDigitsRank)} = {RelabeledDigitsRank}");
		builder.Append($" ({+(from d in DigitsRelabeled select (d + 1).ToString())})");
		return true;
	}


	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_OnesComplement(TSelf)"/>
	public static Transform operator ~(in Transform value)
		=> new(
			Math.UnsignedMod(~value.TransposeRank, (int)GridIdentifier.TransposePermutationsCount),
			Math.UnsignedMod(~value.RelabeledRowsRank, (int)GridIdentifier.RelabelLinesPermutationsCount),
			Math.UnsignedMod(~value.RelabeledColumnsRank, (int)GridIdentifier.RelabelLinesPermutationsCount),
			Math.UnsignedMod(~value.RelabeledDigitsRank, (int)GridIdentifier.RelabelDigitsPermutationsCount)
		);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	[OverloadResolutionPriority(1)]
	public static bool operator ==(in Transform left, in Transform right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	[OverloadResolutionPriority(1)]
	public static bool operator !=(in Transform left, in Transform right) => !(left == right);

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThan(TSelf, TOther)"/>
	public static bool operator >(in Transform left, in Transform right) => left.CompareTo(right) > 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThan(TSelf, TOther)"/>
	public static bool operator <(in Transform left, in Transform right) => left.CompareTo(right) < 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_GreaterThanOrEqual(TSelf, TOther)"/>
	public static bool operator >=(in Transform left, in Transform right) => left.CompareTo(right) >= 0;

	/// <inheritdoc cref="IComparisonOperators{TSelf, TOther, TResult}.op_LessThanOrEqual(TSelf, TOther)"/>
	public static bool operator <=(in Transform left, in Transform right) => left.CompareTo(right) <= 0;

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseAnd(TSelf, TOther)"/>
	public static Transform operator &(in Transform left, in Transform right)
		=> new(
			left.TransposeRank & right.TransposeRank,
			left.RelabeledRowsRank & right.RelabeledRowsRank,
			left.RelabeledColumnsRank & right.RelabeledColumnsRank,
			left.RelabeledDigitsRank & right.RelabeledDigitsRank
		);

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_BitwiseOr(TSelf, TOther)"/>
	public static Transform operator |(in Transform left, in Transform right)
		=> new(
			left.TransposeRank | right.TransposeRank,
			left.RelabeledRowsRank | right.RelabeledRowsRank,
			left.RelabeledColumnsRank | right.RelabeledColumnsRank,
			left.RelabeledDigitsRank | right.RelabeledDigitsRank
		);

	/// <inheritdoc cref="IBitwiseOperators{TSelf, TOther, TResult}.op_ExclusiveOr(TSelf, TOther)"/>
	public static Transform operator ^(in Transform left, in Transform right)
		=> new(
			left.TransposeRank ^ right.TransposeRank,
			left.RelabeledRowsRank ^ right.RelabeledRowsRank,
			left.RelabeledColumnsRank ^ right.RelabeledColumnsRank,
			left.RelabeledDigitsRank ^ right.RelabeledDigitsRank
		);

	/// <inheritdoc/>
	static bool IEqualityOperators<Transform, Transform, bool>.operator ==(Transform left, Transform right)
		=> left == right;

	/// <inheritdoc/>
	static bool IEqualityOperators<Transform, Transform, bool>.operator !=(Transform left, Transform right)
		=> left != right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Transform, Transform, bool>.operator >(Transform left, Transform right)
		=> left > right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Transform, Transform, bool>.operator <(Transform left, Transform right)
		=> left < right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Transform, Transform, bool>.operator >=(Transform left, Transform right)
		=> left >= right;

	/// <inheritdoc/>
	static bool IComparisonOperators<Transform, Transform, bool>.operator <=(Transform left, Transform right)
		=> left <= right;

	/// <inheritdoc/>
	static Transform IBitwiseOperators<Transform, Transform, Transform>.operator &(Transform left, Transform right)
		=> left & right;

	/// <inheritdoc/>
	static Transform IBitwiseOperators<Transform, Transform, Transform>.operator |(Transform left, Transform right)
		=> left | right;

	/// <inheritdoc/>
	static Transform IBitwiseOperators<Transform, Transform, Transform>.operator ^(Transform left, Transform right)
		=> left ^ right;

	/// <inheritdoc/>
	static Transform IBitwiseOperators<Transform, Transform, Transform>.operator ~(Transform value)
		=> ~value;


	/// <summary>
	/// Explicit cast from <see cref="long"/> to <see cref="Transform"/>.
	/// </summary>
	/// <param name="baseMixedRank">The base-mixed rank.</param>
	public static explicit operator Transform(long baseMixedRank)
		=> new(Math.UnsignedMod(baseMixedRank, GridIdentifier.AllPermutationsCount));

	/// <summary>
	/// Explicit cast from <see cref="long"/> to <see cref="Transform"/>, with range check.
	/// </summary>
	/// <param name="baseMixedRank">The base-mixed rank.</param>
	/// <exception cref="OverflowException">Throws when <paramref name="baseMixedRank"/> is invalid.</exception>
	public static explicit operator checked Transform(long baseMixedRank)
		=> new(
			baseMixedRank is >= 0 and < GridIdentifier.AllPermutationsCount
				? baseMixedRank
				: throw new OverflowException()
		);

	/// <summary>
	/// Implicit cast from <see cref="Transform"/> to <see cref="long"/>.
	/// </summary>
	/// <param name="transform">The transform.</param>
	public static implicit operator long(Transform transform) => transform.Rank;
}
