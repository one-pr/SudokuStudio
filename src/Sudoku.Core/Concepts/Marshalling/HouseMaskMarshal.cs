namespace Sudoku.Concepts.Marshalling;

/// <summary>
/// Represents a concept "House Mask". A house mask is a list of house indices represented as a 27-bit integer value.
/// </summary>
public static class HouseMaskMarshal
{
	/// <summary>
	/// Indicates the mask that means all blocks.
	/// </summary>
	public const HouseMask AllBlocksMask = Grid.MaxCandidatesMask;

	/// <summary>
	/// Indicates the mask that means all rows.
	/// </summary>
	public const HouseMask AllRowsMask = Grid.MaxCandidatesMask << 9;

	/// <summary>
	/// Indicates the mask that means all columns.
	/// </summary>
	public const HouseMask AllColumnsMask = Grid.MaxCandidatesMask << 18;

	/// <summary>
	/// Indicates the mask that means all houses.
	/// </summary>
	public const HouseMask AllHousesMask = (1 << 27) - 1;


	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(HouseMask @this)
	{
		/// <summary>
		/// Indicates the triplet of masks that describes the blocks, rows and columns.
		/// </summary>
		public (Mask Block, Mask Row, Mask Column) SplitMask
		{
			get
			{
				var blockMask = (Mask)(@this & Grid.MaxCandidatesMask);
				var rowMask = (Mask)(@this >> 9 & Grid.MaxCandidatesMask);
				var columnMask = (Mask)(@this >> 18 & Grid.MaxCandidatesMask);
				return (blockMask, rowMask, columnMask);
			}
		}


		/// <summary>
		/// Creates for a <see cref="HouseMask"/> instance via the specified houses.
		/// </summary>
		/// <param name="houses">The houses.</param>
		/// <returns>A <see cref="HouseMask"/> instance.</returns>
		public static HouseMask Create(ReadOnlySpan<House> houses)
		{
			var result = 0;
			foreach (var house in houses)
			{
				result |= 1 << house;
			}
			return result;
		}
	}
}
