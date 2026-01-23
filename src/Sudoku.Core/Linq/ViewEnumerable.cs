namespace Sudoku.Linq;

/// <summary>
/// Represents with LINQ methods for <see cref="View"/> instances.
/// </summary>
/// <seealso cref="View"/>
public static class ViewEnumerable
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(View[] @this)
	{
		/// <inheritdoc cref="ArrayEnumerable.SelectMany{TSource, TCollection, TResult}(TSource[], Func{TSource, ReadOnlySpan{TCollection}}, Func{TSource, TCollection, TResult})"/>
		public ReadOnlySpan<ViewNode> SelectMany(Func<View, View> collectionSelector, Func<View, ViewNode, ViewNode> resultSelector)
		{
			var length = @this.Length;
			var result = new List<ViewNode>(length << 1);
			for (var i = 0; i < length; i++)
			{
				var element = @this[i];
				foreach (var subElement in collectionSelector(element))
				{
					result.Add(resultSelector(element, subElement));
				}
			}
			return result.AsSpan();
		}
	}
}
