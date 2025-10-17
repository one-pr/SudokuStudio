namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a forcing chains component.
/// </summary>
public interface IForcingChains : IChainOrForcingChains, IFormattable
{
	/// <summary>
	/// Indicates the complexity of the whole pattern.
	/// </summary>
	int Complexity { get; }

	/// <summary>
	/// Indicates the digits used in this pattern.
	/// </summary>
	Mask DigitsMask { get; }

	/// <summary>
	/// Indicates the conclusions of the pattern.
	/// </summary>
	ReadOnlyMemory<Conclusion> Conclusions { get; }

	/// <summary>
	/// Indicates the complexity of each branch.
	/// </summary>
	ReadOnlySpan<int> BranchedComplexity { get; }

	/// <summary>
	/// Indicates all possible branches.
	/// </summary>
	ReadOnlySpan<UnnamedChain> Branches { get; }


	/// <summary>
	/// Collect views for the current chain.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="newConclusions">The conclusions.</param>
	/// <param name="supportedRules">The supported rules.</param>
	/// <returns>The views.</returns>
	sealed View[] GetViews(in Grid grid, Conclusion[] newConclusions, ChainingRuleCollection supportedRules)
	{
		var viewNodes = GetViewsCore(grid, supportedRules, newConclusions);
		var result = new View[viewNodes.Length];
		for (var i = 0; i < viewNodes.Length; i++)
		{
			var elimMap = (from conclusion in newConclusions select conclusion.Candidate).AsCandidateMap();
			result[i] = [
				..
				from node in viewNodes[i]
				where node is not CandidateViewNode { Candidate: var c } || !elimMap.Contains(c)
				select node
			];
		}

		var viewIndex = 1;
		var processedViewNodesMap = new ProcessedViewNodeMap();
		foreach (var branch in Branches)
		{
			foreach (var supportedRule in supportedRules)
			{
				supportedRule.GetViewNodes(grid, branch, result[viewIndex], processedViewNodesMap, out var producedViewNodes);
				result[0].AddRange(producedViewNodes);
			}
			viewIndex++;
		}
		return result;
	}

	/// <summary>
	/// Represents a method that creates a list of views.
	/// </summary>
	/// <param name="grid">The target grid.</param>
	/// <param name="rules">The rules used.</param>
	/// <param name="newConclusions">The conclusions used.</param>
	/// <returns>A list of nodes.</returns>
	protected ReadOnlySpan<ViewNode[]> GetViewsCore(in Grid grid, ChainingRuleCollection rules, Conclusion[] newConclusions);

	/// <inheritdoc/>
	string IFormattable.ToString(string? format, IFormatProvider? formatProvider) => ToString(formatProvider);
}
