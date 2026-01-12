namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Provides extension methods on <see cref="BraidType"/>.
/// </summary>
/// <seealso cref="BraidType"/>
public static class BraidTypeExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="BraidType"/>.
	/// </summary>
	extension(BraidType)
	{
		/// <summary>
		/// Creates a <see cref="BraidType"/> field via 3 rotation types.
		/// </summary>
		/// <param name="type1">The first rotation type.</param>
		/// <param name="type2">The second rotation type.</param>
		/// <param name="type3">The third rotation type.</param>
		/// <returns>The result braid type.</returns>
		public static BraidType Create(RotationType type1, RotationType type2, RotationType type3)
			=> (type1, type2, type3) switch
			{
				(RotationType.Downside, RotationType.Downside, RotationType.Downside) => BraidType.NRope,
				(RotationType.Downside, RotationType.Downside, RotationType.Upside) => BraidType.NBraid,
				(RotationType.Downside, RotationType.Upside, RotationType.Downside) => BraidType.NBraid,
				(RotationType.Upside, RotationType.Downside, RotationType.Downside) => BraidType.NBraid,
				(RotationType.Downside, RotationType.Upside, RotationType.Upside) => BraidType.ZBraid,
				(RotationType.Upside, RotationType.Downside, RotationType.Upside) => BraidType.ZBraid,
				(RotationType.Upside, RotationType.Upside, RotationType.Downside) => BraidType.ZBraid,
				(RotationType.Upside, RotationType.Upside, RotationType.Upside) => BraidType.ZRope,
				_ => throw new ArgumentException($"Invalid type combination ({type1}, {type2}, {type3})")
			};

		/// <summary>
		/// Creates a <see cref="BraidType"/> field via 3 rotation types.
		/// </summary>
		/// <param name="types">The types.</param>
		/// <returns>The result braid type.</returns>
		public static BraidType Create(params ReadOnlySpan<RotationType> types)
		{
			ArgumentException.Assert(types.Length == 3);
			return BraidType.Create(types[0], types[1], types[2]);
		}
	}
}
