namespace Sudoku.Analytics.Bottlenecks;

/// <summary>
/// Represents a bottleneck filter consumed by method <see cref="AnalysisResultExtensions.GetBottlenecks(AnalysisResult, ReadOnlySpan{BottleneckFilter})"/>.
/// </summary>
/// <param name="TechniqueType">Indicates the technique type.</param>
/// <param name="BottleneckType">Indicates the bottleneck type.</param>
/// <seealso cref="AnalysisResultExtensions.GetBottlenecks(AnalysisResult, ReadOnlySpan{BottleneckFilter})"/>
public record struct BottleneckFilter(TechniqueType TechniqueType, BottleneckType BottleneckType);
