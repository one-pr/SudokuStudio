namespace Sudoku.Analytics.Dependency.Contradictions;

/// <summary>
/// Represents a cause that describes which space (house or cell) becomes empty if the list of assignments are applied.
/// </summary>
/// <param name="Candidate">Indicates the start candidate.</param>
/// <param name="LastNode">Indicates the node that causes a conflict.</param>
/// <param name="EmptySpace">Indicates which space will become empty if assignments are applied.</param>
public readonly record struct Cause(Candidate Candidate, DependencyNode LastNode, Space EmptySpace);
