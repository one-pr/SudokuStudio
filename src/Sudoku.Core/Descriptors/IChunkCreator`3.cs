namespace Sudoku.Descriptors;

/// <summary>
/// Defines a type that is a chunk, which supports bit state map creation.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
/// <typeparam name="TMap">The type of map.</typeparam>
/// <typeparam name="TElement">The type of each element.</typeparam>
public interface IChunkCreator<out TSelf, TMap, TElement> : IChunkCreator<TSelf, TElement>
	where TSelf : IChunkCreator<TSelf, TElement>
	where TMap : unmanaged, IBitStateMap<TMap, TElement>
	where TElement : unmanaged, IBinaryInteger<TElement>
{
	/// <summary>
	/// Create an instance via a map.
	/// </summary>
	/// <param name="map">The map.</param>
	/// <returns>The instance.</returns>
	static abstract TSelf Create(in TMap map);
}
