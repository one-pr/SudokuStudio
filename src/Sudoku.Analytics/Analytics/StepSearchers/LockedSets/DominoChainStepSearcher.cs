namespace Sudoku.Analytics.StepSearchers;

/// <summary>
/// Provides with a <b>Domino Chain</b> step searcher.
/// The step searcher will include the following techniques:
/// <list type="bullet">
/// <item>Domino Chain</item>
/// </list>
/// </summary>
[StepSearcher("StepSearcherName_DominoChainStepSearcher", Technique.DominoChain)]
public sealed partial class DominoChainStepSearcher : StepSearcher
{
	/// <inheritdoc/>
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Search for Sue de Coq chain.
		// A valid Sue de Coq chain has a property that they starts with a line (row or column),
		// and alternate line types, inserting blocks as interim bridge.
		// For example, the following puzzle shows the houses that a chain used:
		//
		//                           2369 + 14
		// .-------------------.---------------------.--------------------.
		// | 137   367    368  | 2689  (1269)  5     | 2368  23789  4     |
		// | 47    4567   4568 | 3     (2469)  2689  | 2568  1      25789 |
		// | 134   2      9    |(468)   7     (168)  |(3568)(358)  (58)   | 3568 + 14
		// :-------------------+---------------------+--------------------:
		// | 5     39     1    | 2689  (2369)  4     | 7     289    289   |
		// | 6     8      2    | 79     5      79    | 1     4      3     |
		// | 349   349    7    | 1     (239)   289   | 258   2589   6     |
		// :-------------------+---------------------+--------------------:
		// |(2347)(3457) (345) |(247)   8     (127)  | 9     6      1257  | 27 + 3456
		// | 2479  1     (456) | 24679 (2469)  3     | 2458  2578   2578  |
		// | 8     34679 (346) | 5     (12469) 12679 | 234   237    127   |
		// '-------------------'---------------------'--------------------'
		//
		// Puzzle:
		//   .....5..4...3...1..29.7....5.1..47..682.5.143..71....6....8.96..1...3...8..5.....:218 318 818 625 527 229 529 829 944 945 965 966 267 481 287 392 492 393 695 297

		return null;
	}
}
