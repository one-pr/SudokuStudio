namespace Sudoku.Drawing.Nodes;

/// <summary>
/// Defines a view node that highlights for a candidate.
/// </summary>
/// <param name="identifier"><inheritdoc/></param>
/// <param name="candidate"><inheritdoc cref="Candidate" path="/summary"/></param>
[method: JsonConstructor]
public sealed class CandidateViewNode(ColorDescriptor identifier, Candidate candidate) : BasicViewNode(identifier)
{
	/// <summary>
	/// Indicates the candidate highlighted.
	/// </summary>
	public Candidate Candidate { get; } = candidate;

	/// <summary>
	/// Indicates the target cell.
	/// </summary>
	public Cell Cell => Candidate / 9;


	/// <inheritdoc/>
	public override bool Equals([NotNullWhen(true)] ViewNode? other)
		=> base.Equals(other) && other is CandidateViewNode comparer && Candidate == comparer.Candidate;

	/// <inheritdoc cref="object.GetHashCode"/>
	public override int GetHashCode() => HashCode.Combine(Candidate, TypeIdentifier);

	/// <inheritdoc/>
	public override string ToString()
	{
		var candidateString = Candidate.ToCandidateString(Candidate, CoordinateConverter.InvariantCulture);
		return $"{nameof(CandidateViewNode)} {{ Candidate = {candidateString}, {nameof(Identifier)} = {Identifier} }}";
	}

	/// <inheritdoc/>
	public override CandidateViewNode Clone() => new(Identifier, Candidate);
}
