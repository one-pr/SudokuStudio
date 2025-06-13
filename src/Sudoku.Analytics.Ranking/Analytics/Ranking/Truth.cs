namespace Sudoku.Analytics.Ranking;

/// <summary>
/// Represents a truth. A truth is a set of candidates (generally one digit in a whole house, or a cell),
/// which requires the set can only fill one digit into it.
/// Both no digits filled or greater than 2 digits filled are disallowed.
/// </summary>
public abstract class Truth : RankSet;
