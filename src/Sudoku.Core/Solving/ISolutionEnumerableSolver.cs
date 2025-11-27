namespace Sudoku.Solving;

/// <summary>
/// Represents a solver that can find all possible solutions with detection on every operation of new solution found.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
public interface ISolutionEnumerableSolver<TSelf> : ISolver where TSelf : ISolutionEnumerableSolver<TSelf>, allows ref struct
{
	/// <summary>
	/// Provide a way to detect event to be triggered when a solution is found;
	/// no matter whether the puzzle has a unique solution or not (multiple solutions).
	/// </summary>
	event EventHandler<TSelf, SolverSolutionFoundEventArgs>? SolutionFound;


	/// <summary>
	/// Try to perform enumeration on solutions.
	/// </summary>
	/// <param name="grid">The grid to be solved.</param>
	/// <param name="cancellationToken">The cancellation token that can cancel the current operation.</param>
	void EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken);
}
