namespace Sudoku.Algorithms.UniquenessTest;

/// <summary>
/// Represents a pattern trial node.
/// </summary>
/// <param name="Assigned">Indicates the assigned candidate.</param>
/// <param name="Parent">Indicates the parent node.</param>
public sealed record PatternTrialNode(Candidate Assigned, PatternTrialNode? Parent) :
	IEqualityOperators<PatternTrialNode, PatternTrialNode, bool>
{
	/// <summary>
	/// Indicates all assigned candidates.
	/// </summary>
	public CandidateMap AssignedCandidates
	{
		get
		{
			var result = CandidateMap.Empty;
			for (var node = this; node is not null; node = node.Parent)
			{
				result.Add(node.Assigned);
			}
			return result;
		}
	}


	/// <inheritdoc/>
	public bool Equals([NotNullWhen(true)] PatternTrialNode? other)
		=> other is not null && AssignedCandidates == other.AssignedCandidates;

	/// <inheritdoc/>
	public override int GetHashCode() => AssignedCandidates.GetHashCode();

	/// <summary>
	/// Apply all assignments to the target grid.
	/// </summary>
	/// <param name="grid">The grid.</param>
	public void ApplyTo(ref Grid grid)
	{
		foreach (var assigned in AssignedCandidates)
		{
			grid.SetDigit(assigned / 9, assigned % 9);
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp9/feature[@name='records']/target[@name='method' and @cref='PrintMembers']"/>
	private bool PrintMembers(StringBuilder builder)
	{
		var cell = Assigned / 9;
		var digit = Assigned % 9;

		builder.Append($"{nameof(Assigned)} = ");
		builder.Append($"r{cell / 9 + 1}c{cell % 9 + 1}({digit + 1})");
		builder.Append($", {nameof(Parent)} = ");
		if (Parent is not null)
		{
			var parentAssigned = Parent.Assigned;
			var parentCell = parentAssigned / 9;
			var parentDigit = parentAssigned % 9;
			builder.Append($"r{parentCell / 9 + 1}c{parentCell % 9 + 1}({parentDigit + 1})");
		}
		else
		{
			builder.Append("<null>");
		}
		return true;
	}
}
