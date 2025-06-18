namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a queue node that is used in calculating assignment combinations.
/// </summary>
/// <param name="State">Indicates the state.</param>
/// <param name="RemainingTruthIndices">Indicates the remaining truth indices.</param>
internal sealed record CombinationQueueNode(in CandidateMap State, int[] RemainingTruthIndices);
