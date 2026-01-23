namespace Sudoku.Linq;

/// <summary>
/// Provides with a list of LINQ methods used by <see cref="Conclusion"/>.
/// </summary>
/// <seealso cref="Conclusion"/>
public static class ConclusionsEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
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
				result += selector(element);
			}
			return result;
		}
	}
}
