namespace Sudoku.Theories.DeadlyPatternTheory;

/// <summary>
/// Indicates the result value after <see cref="DeadlyPatternChecker.CheckWhetherFormsDeadlyPattern"/> called.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <seealso cref="DeadlyPatternChecker.CheckWhetherFormsDeadlyPattern"/>
public readonly ref struct DeadlyPatternCheckingResult(ref readonly Grid grid)
{
	/// <summary>
	/// Indicates the grid used.
	/// </summary>
	public readonly ref readonly Grid Grid = ref grid;


	/// <summary>
	/// Indicates the pattern is a real deadly pattern.
	/// </summary>
	public required bool IsDeadlyPattern { get; init; }

	/// <summary>
	/// Indicates the number of permutations.
	/// </summary>
	public required int PermutationsCount { get; init; }

	/// <summary>
	/// Indicates the failed reason.
	/// </summary>
	public DeadlyPatternResultFailedReason FailedReason { get; init; }

	/// <summary>
	/// Indicates all possible failed cases. The value can be an empty sequence if the pattern is a real deadly pattern,
	/// or not a deadly pattern but containing obvious invalid candidates (like containing given or modifiable cells).
	/// </summary>
	public ReadOnlySpan<Grid> FailedCases { get; init; }

	/// <summary>
	/// Indicates the candidates the pattern used.
	/// </summary>
	public CandidateMap PatternCandidates { get; init; }


	/// <inheritdoc cref="object.ToString"/>
	public override string ToString()
		=> $"{nameof(PermutationsCount)} = {PermutationsCount}, {nameof(IsDeadlyPattern)} = {IsDeadlyPattern}";
}
