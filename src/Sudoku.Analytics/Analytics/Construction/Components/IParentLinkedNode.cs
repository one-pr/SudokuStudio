namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a node that supports parent linking.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
internal interface IParentLinkedNode<TSelf> :
	IComponent,
	IEquatable<TSelf>,
	IEqualityOperators<TSelf, TSelf, bool>
	where TSelf : IParentLinkedNode<TSelf>
{
	/// <summary>
	/// Indicates the length of ancestors.
	/// </summary>
	sealed int AncestorsLength
	{
		get
		{
			var result = 0;
			for (var node = this; node is not null; node = node.Parent)
			{
				result++;
			}
			return result;
		}
	}

	/// <summary>
	/// Indicates all ancestor nodes of the current node.
	/// </summary>
	sealed ReadOnlySpan<TSelf> Ancestors
	{
		get
		{
			var (result, p) = (new List<TSelf> { (TSelf)this }, Parent);
			while (p is not null)
			{
				result.Add(p);
				p = p.Parent;
			}
			return result.AsSpan();
		}
	}

	/// <summary>
	/// Indicates the parent node.
	/// </summary>
	TSelf? Parent { get; }

	/// <summary>
	/// Indicates the root node.
	/// </summary>
	TSelf Root { get; }


	/// <summary>
	/// Determines whether the current node is an ancestor of the specified node. 
	/// </summary>
	/// <param name="childNode">The node to be checked.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	bool IsAncestorOf(TSelf childNode)
	{
		for (var node = childNode; node is not null; node = node.Parent)
		{
			if (Equals(node))
			{
				return true;
			}
		}
		return false;
	}

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string representation.</returns>
	string ToString(CultureInfo culture);

	/// <inheritdoc cref="ToString(ICandidateMapConverter, IFormatProvider?)"/>
	string ToString(CoordinateConverter converter);

	/// <inheritdoc cref="ToString(ICandidateMapConverter, IFormatProvider?)"/>
	string ToString(ICandidateMapConverter converter);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string representation.</returns>
	string ToString(ICandidateMapConverter converter, IFormatProvider? formatProvider);


	/// <summary>
	/// Creates a <see cref="WhipNode"/> instance with parent node.
	/// </summary>
	/// <param name="current">The current node.</param>
	/// <param name="parent">The parent node.</param>
	/// <returns>The new node created.</returns>
	static abstract TSelf Create(TSelf current, TSelf? parent);
}
