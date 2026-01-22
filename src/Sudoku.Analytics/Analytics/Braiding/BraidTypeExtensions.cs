namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Provides extension methods on <see cref="BraidingType"/>.
/// </summary>
/// <seealso cref="BraidingType"/>
public static class BraidTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="BraidingType"/>.
	/// </summary>
	/// <param name="this">The current field.</param>
	extension(BraidingType @this)
	{
		/// <summary>
		/// Indicates whether the field can be defined as "rope".
		/// </summary>
		public bool IsRope => @this is BraidingType.NRope or BraidingType.ZRope;

		/// <summary>
		/// Indicates whether the field can be defined as "braid".
		/// </summary>
		public bool IsBraid => @this is BraidingType.NBraid or BraidingType.ZBraid;

		/// <summary>
		/// Indicates the number of N's.
		/// </summary>
		public int NCount
			=> @this switch
			{
				BraidingType.NRope => 3,
				BraidingType.NBraid => 2,
				BraidingType.ZBraid => 1,
				BraidingType.ZRope => 0,
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};

		/// <summary>
		/// Indicates the number of Z's.
		/// </summary>
		public int ZCount
			=> @this switch
			{
				BraidingType.NRope => 0,
				BraidingType.NBraid => 1,
				BraidingType.ZBraid => 2,
				BraidingType.ZRope => 3,
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};


		/// <summary>
		/// Converts the current field into the easy-to-read mode string of 3 characters,
		/// with each character either <c>'N'</c> or <c>'Z'</c>.
		/// </summary>
		/// <returns>A string of 3 characters, with each character either <c>'N'</c> or <c>'Z'</c>.</returns>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public string ToSimpleString()
			=> @this switch
			{
				BraidingType.Unknown => string.Empty,
				BraidingType.NRope => nameof(BraidingType.NNN),
				BraidingType.NBraid => nameof(BraidingType.NNZ),
				BraidingType.ZBraid => nameof(BraidingType.NZZ),
				BraidingType.ZRope => nameof(BraidingType.ZZZ),
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};


		/// <summary>
		/// Creates a <see cref="BraidingType"/> field via 3 rotation types.
		/// </summary>
		/// <param name="type1">The first rotation type.</param>
		/// <param name="type2">The second rotation type.</param>
		/// <param name="type3">The third rotation type.</param>
		/// <returns>The result braid type.</returns>
		public static BraidingType Create(StrandType type1, StrandType type2, StrandType type3)
			=> (type1, type2, type3) switch
			{
				(StrandType.Downside, StrandType.Downside, StrandType.Downside) => BraidingType.NRope,
				(StrandType.Downside, StrandType.Downside, StrandType.Upside) => BraidingType.NBraid,
				(StrandType.Downside, StrandType.Upside, StrandType.Downside) => BraidingType.NBraid,
				(StrandType.Upside, StrandType.Downside, StrandType.Downside) => BraidingType.NBraid,
				(StrandType.Downside, StrandType.Upside, StrandType.Upside) => BraidingType.ZBraid,
				(StrandType.Upside, StrandType.Downside, StrandType.Upside) => BraidingType.ZBraid,
				(StrandType.Upside, StrandType.Upside, StrandType.Downside) => BraidingType.ZBraid,
				(StrandType.Upside, StrandType.Upside, StrandType.Upside) => BraidingType.ZRope,
				_ => throw new ArgumentException($"Invalid type combination ({type1}, {type2}, {type3})")
			};

		/// <summary>
		/// Creates a <see cref="BraidingType"/> field via 3 rotation types.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <returns>The result braid type.</returns>
		public static BraidingType Create(params ReadOnlySpan<StrandType> types)
		{
			ArgumentException.Assert(types.Length == 3);
			return BraidingType.Create(types[0], types[1], types[2]);
		}
	}
}
