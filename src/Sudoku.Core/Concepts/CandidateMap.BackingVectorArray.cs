namespace Sudoku.Concepts;

public partial struct CandidateMap
{
	/// <summary>
	/// Indicates the backing vector array type as field of containing type <see cref="CandidateMap"/>.
	/// </summary>
	/// <remarks>
	/// Do not inline the field to <see cref="CandidateMap"/>
	/// because it will conflict with predefined rules of C# 12 feature "Inline Array".
	/// </remarks>
	[InlineArray(12)]
	private struct BackingVectorArray :
		IEquatable<BackingVectorArray>,
		IEqualityOperators<BackingVectorArray, BackingVectorArray, bool>,
		IInlineArray<BackingVectorArray, ulong>
	{
		/// <summary>
		/// Indicates the first element of the whole buffer.
		/// </summary>
		private ulong _firstElement;


		/// <summary>
		/// Returns a sequence of <see cref="Vector256{T}"/> of <see cref="ulong"/> values that can be used in SIMD scenarios.
		/// </summary>
		public readonly ReadOnlySpan<Vector256<ulong>> Vectors
			=> (Vector256<ulong>[])[Vector256.Create(this[..4]), Vector256.Create(this[4..8]), Vector256.Create(this[8..])];

		/// <inheritdoc/>
		[UnscopedRef]
		readonly ReadOnlySpan<ulong> IInlineArray<BackingVectorArray, ulong>.Elements => this[..];


		/// <inheritdoc/>
		static int IInlineArray<BackingVectorArray, ulong>.InlineArrayLength => 12;


		/// <inheritdoc/>
		[UnscopedRef]
		ref ulong IInlineArray<BackingVectorArray, ulong>.this[int index] => ref this[index];


		/// <inheritdoc cref="object.ToString"/>
		public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is BackingVectorArray comparer && Equals(comparer);

		/// <inheritdoc cref="IEquatable{T}.Equals(T)"/>
		public readonly bool Equals(in BackingVectorArray other)
		{
			var thisVectors = Vectors;
			var otherVectors = other.Vectors;
			return thisVectors[0] == otherVectors[0] && thisVectors[1] == otherVectors[1] && thisVectors[2] == otherVectors[2];
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			var hashCode = default(HashCode);
			for (var i = 0; i < InlineArrayLength; i++)
			{
				hashCode.Add(this[i]);
			}
			return hashCode.ToHashCode();
		}

		/// <inheritdoc/>
		readonly bool IEquatable<BackingVectorArray>.Equals(BackingVectorArray other) => Equals(other);

		/// <inheritdoc/>
		[UnscopedRef]
		readonly ReadOnlySpan<ulong> IInlineArray<BackingVectorArray, ulong>.AsReadOnlySpan() => this;

		/// <inheritdoc/>
		[UnscopedRef]
		ref ulong IInlineArray<BackingVectorArray, ulong>.GetPinnableReference() => ref this[0];

		/// <inheritdoc/>
		[UnscopedRef]
		Span<ulong> IInlineArray<BackingVectorArray, ulong>.AsSpan() => this;


		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
		public static bool operator ==(in BackingVectorArray left, in BackingVectorArray right) => left.Equals(right);

		/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
		public static bool operator !=(in BackingVectorArray left, in BackingVectorArray right) => !(left == right);

		/// <inheritdoc/>
		static bool IEqualityOperators<BackingVectorArray, BackingVectorArray, bool>.operator ==(BackingVectorArray left, BackingVectorArray right)
			=> left == right;

		/// <inheritdoc/>
		static bool IEqualityOperators<BackingVectorArray, BackingVectorArray, bool>.operator !=(BackingVectorArray left, BackingVectorArray right)
			=> left != right;
	}
}
