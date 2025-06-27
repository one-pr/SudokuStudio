namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Represents truth view node.
/// </summary>
/// <param name="identifier"><inheritdoc cref="RankSetViewNode(ColorIdentifier, Space)" path="/param[@name='identifier']"/></param>
/// <param name="space"><inheritdoc cref="RankSetViewNode(ColorIdentifier, Space)" path="/param[@name='space']"/></param>
public sealed class TruthSpaceViewNode(ColorIdentifier identifier, Space space) : RankSetViewNode(identifier, space)
{
	/// <inheritdoc/>
	public override bool IsTruth => true;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is TruthSpaceViewNode comparer && Space == comparer.Space;

	/// <inheritdoc/>
	public override TruthSpaceViewNode Clone() => new(Identifier, Space);
}
