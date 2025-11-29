namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Represents a Boolean formula in Conjunctive Normal Form (CNF).
/// Stores a list of clauses, where each clause is an array of <see cref="int"/> literals.
/// Using positive integers to represents a variable assigned <see langword="true"/>; for negative integers, <see langword="false"/>.
/// </summary>
/// <param name="VariablesCount">Indicates total number of variables.</param>
public sealed record CnfExpression(int VariablesCount) : IEnumerable<ReadOnlyMemory<int>>
{
	/// <summary>
	/// Indicates the number of clauses.
	/// </summary>
	public int ClauseCount => Clauses.Count;

	/// <summary>
	/// Indicates the list of clauses.
	/// </summary>
	internal List<ReadOnlyMemory<int>> Clauses { get; } = [];


	/// <summary>
	/// Get clause at the specified index.
	/// </summary>
	/// <param name="index">The desired index.</param>
	/// <returns>The clause.</returns>
	public ReadOnlyMemory<int> this[int index] => Clauses[index];


	/// <summary>
	/// Add a new clause (disjunction of literals) to the expression.
	/// </summary>
	/// <param name="literals">The literals.</param>
	public void AddClause(ReadOnlyMemory<int> literals) => Clauses.Add(literals);

	/// <inheritdoc cref="IEnumerable{T}.GetEnumerator"/>
	public AnonymousSpanEnumerator<ReadOnlyMemory<int>> GetEnumerator() => new(Clauses.AsSpan());

	/// <inheritdoc/>
	IEnumerator IEnumerable.GetEnumerator() => Clauses.GetEnumerator();

	/// <inheritdoc/>
	IEnumerator<ReadOnlyMemory<int>> IEnumerable<ReadOnlyMemory<int>>.GetEnumerator() => Clauses.GetEnumerator();
}
