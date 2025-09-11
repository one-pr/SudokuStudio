namespace Sudoku.Linq;

/// <summary>
/// Provides with a list of LINQ methods used by <see cref="Conclusion"/>.
/// </summary>
/// <seealso cref="Conclusion"/>
public static class ConclusionsEnumerable
{
	/// <summary>
	/// Provides extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="Conclusion"/>.
	/// </summary>
	extension(ReadOnlySpan<Conclusion> @this)
	{
		/// <summary>
		/// Projects a list of <see cref="Conclusion"/> instances, converted each instances into a <see cref="Cell"/> value,
		/// and merge them into a <see cref="CellMap"/> and return it.
		/// </summary>
		/// <param name="selector">The selector to project the values.</param>
		/// <returns>A <see cref="CellMap"/> result.</returns>
		public CellMap Select(Func<Conclusion, Cell> selector)
		{
			var result = CellMap.Empty;
			foreach (var element in @this)
			{
				result.Add(selector(element));
			}
			return result;
		}
	}
}
