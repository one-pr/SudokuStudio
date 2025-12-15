namespace Sudoku.Descriptors;

/// <summary>
/// Defines a type that is a chunk.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <typeparam name="TElement">The type of each element.</typeparam>
public interface IChunkCreator<out TSelf, TElement>
	where TSelf : IChunkCreator<TSelf, TElement>
	where TElement : notnull
{
	/// <summary>
	/// Create an instance via a single element.
	/// </summary>
	/// <param name="element">The element.</param>
	/// <returns>The instance.</returns>
	static abstract TSelf Create(TElement element);

	/// <summary>
	/// Create an instance via an array.
	/// </summary>
	/// <param name="array">The array.</param>
	/// <returns>The instance.</returns>
	static abstract TSelf Create(TElement[] array);

	/// <summary>
	/// Create an instance via a list.
	/// </summary>
	/// <param name="list">The list.</param>
	/// <returns>The instance.</returns>
	static abstract TSelf Create(List<TElement> list);

	/// <summary>
	/// Create an instance via a hash set.
	/// </summary>
	/// <param name="hashSet">The hash set.</param>
	/// <returns>The instance.</returns>
	static abstract TSelf Create(HashSet<TElement> hashSet);
}
