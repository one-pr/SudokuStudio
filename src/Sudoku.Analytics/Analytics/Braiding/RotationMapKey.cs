namespace Sudoku.Analytics.Braiding;

/// <summary>
/// Represents cells key of braiding, used by <see cref="BraidAnalysis.RotationMap"/>.
/// </summary>
/// <param name="ChuteIndex">Indicates chute index (0..6).</param>
/// <param name="SequenceIndex">Indicates sequence index (0..3).</param>
/// <param name="Type">Indicates type of rotation.</param>
/// <seealso cref="BraidAnalysis.RotationMap"/>
internal readonly record struct RotationMapKey(int ChuteIndex, Digit SequenceIndex, RotationType Type);
