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
	/// <summary>
	/// Indicates the row column first grouped by block.
	/// </summary>
	private static readonly (House Row, House Column)[] LineFirst = [(9, 18), (9, 21), (9, 24), (12, 18), (12, 21), (12, 24), (15, 18), (15, 21), (15, 24)];


	/// <inheritdoc/>
	[InterceptorMethodCaller]
	protected internal override Step? Collect(ref StepAnalysisContext context)
	{
		ref readonly var grid = ref context.Grid;

		// Collect all ALSes, and ignore all ALSes that are bi-value cells and in blocks.
		var alsesInLine = new Dictionary<House, HashSet<AlmostLockedSetPattern>>(18);
		var alsesInBlock = new Dictionary<House, HashSet<AlmostLockedSetPattern>>(9);
		foreach (var als in AlmostLockedSetPattern.Collect(grid))
		{
			if (!als.IsBivalueCell
				&& als.House is var house
				&& (house >= 9 ? alsesInLine : alsesInBlock) is var dic
				&& !dic.TryAdd(house, [als]))
			{
				dic[house].Add(als);
			}
		}

		// Collect start nodes.
		var queue = new Queue<QueueNode>();
		foreach (var startAlses in alsesInLine.Values)
		{
			foreach (var startAls in startAlses)
			{
				// Calculates for RCCs for start ALS.
				var rccs = CandidateMap.Empty;
				var tempBlocksMask = (Mask)0;
				foreach (var digit in startAls.DigitsMask)
				{
					var availableCells = startAls.Cells & CandidatesMap[digit];
					var blockMask = availableCells.BlockMask;
					if (!IsPow2((uint)blockMask))
					{
						// This digit uses 2 or more blocks.
						continue;
					}

					if (availableCells.Count == 3)
					{
						// This digit cannot be used as a valid RCC.
						continue;
					}

					tempBlocksMask |= blockMask;
					foreach (var cell in availableCells)
					{
						rccs.Add(cell * 9 + digit);
					}
				}
				if (!IsPow2(tempBlocksMask) || rccs.Cells.Count == 3)
				{
					// Skip for two cases:
					//   1) There's at least 2 separated blocks are used as different RCCs
					//      because we cannot construct a valid pattern on such case.
					//   2) We cannot find for the next ALSes because no lines available.
					continue;
				}

				// Add the ALS as root node.
				queue.Enqueue(new(startAls, rccs, null));
			}
		}

		// Then we should choose a row or column as the start, in BFS.
		while (queue.Count != 0)
		{
			// Dequeue a node.
			var currentNode = queue.Dequeue();

			// Check data.
			var currentAls = currentNode.Pattern;
			var currentRcc = currentNode.RestrictedCommon;
			var currentRccDigitsMask = currentRcc.Digits;
			var currentRccCells = currentRcc.Cells;
			var currentAlsSharedBlock = currentRccCells.SharedBlock;
			var (nextHouseStartRow, nextHouseStartColumn) = LineFirst[currentAlsSharedBlock];
			var nextHouseStart = currentAls.House.HouseType == HouseType.Row ? nextHouseStartColumn : nextHouseStartRow;
			for (var nextHouse = nextHouseStart; nextHouse < nextHouseStart + 3; nextHouse++)
			{
				if (HousesMap[nextHouse] & currentRccCells)
				{
					// Cannot select the intersected one.
					continue;
				}

				// Check ALSes in the next house.
				if (!alsesInLine.TryGetValue(nextHouse, out var nextAlses))
				{
					continue;
				}

				// Iterate ALSes in the next house.
				foreach (var nextAls in nextAlses)
				{
					if ((nextAls.DigitsMask & currentRccDigitsMask) != currentRccDigitsMask)
					{
						// Not all digits are covered.
						continue;
					}

					// Check whether all RCC digits in next ALS can see digits from the current ALS.
					var areAllDigitsInNextAlsCanSeeOnesFromPreviousAls = true;
					var nextAlsCells = nextAls.Cells;
					foreach (var digit in currentRccDigitsMask)
					{
						if (((nextAlsCells & CandidatesMap[digit]).BlockMask >> currentAlsSharedBlock & 1) == 0)
						{
							areAllDigitsInNextAlsCanSeeOnesFromPreviousAls = false;
							break;
						}
					}
					if (!areAllDigitsInNextAlsCanSeeOnesFromPreviousAls)
					{
						continue;
					}

					var nextRccs = CandidateMap.Empty;
					var tempNextBlocksMask = (Mask)0;
					foreach (var digit in nextAls.DigitsMask)
					{
						// Check whether the digit can form an RCC.
						var possibleDigitRcc = (nextAls.Cells & CandidatesMap[digit]) * digit & ~currentRcc;
						var blockMask = possibleDigitRcc.Cells.BlockMask;
						tempNextBlocksMask |= blockMask;
						if (PopCount((uint)blockMask) == 1 && possibleDigitRcc.Count < 3)
						{
							nextRccs |= possibleDigitRcc;
						}
					}
					if (!IsPow2(tempNextBlocksMask) || nextRccs.Cells.Count == 3)
					{
						// Skip for two cases mentioned above.
						continue;
					}

					// Add node to the next iteration.
					queue.Enqueue(new(nextAls, nextRccs, currentNode));

					// TODO: Check for block enclosing states.
				}
			}
		}

		return null;
	}
}

/// <summary>
/// Represents a file-local queue node on calculating domino chains.
/// </summary>
/// <param name="pattern"><inheritdoc cref="Pattern" path="/summary"/></param>
/// <param name="restrictedCommon"><inheritdoc cref="RestrictedCommon" path="/summary"/></param>
/// <param name="parent"><inheritdoc cref="Parent" path="/summary"/></param>
file sealed class QueueNode(AlmostLockedSetPattern pattern, in CandidateMap restrictedCommon, QueueNode? parent)
{
	/// <summary>
	/// Indicates the restricted common candidates.
	/// </summary>
	public CandidateMap RestrictedCommon { get; } = restrictedCommon;

	/// <summary>
	/// Indicates the pattern.
	/// </summary>
	public AlmostLockedSetPattern Pattern { get; } = pattern;

	/// <summary>
	/// Indicates the parent node.
	/// </summary>
	public QueueNode? Parent { get; } = parent;


	/// <inheritdoc/>
	public override string ToString()
	{
		var sb = new StringBuilder();
		sb.Append($"{nameof(QueueNode)} {{ ");
		sb.Append($"{nameof(Pattern)} = ");
		sb.Append(Pattern.ToString());
		sb.Append($", {nameof(RestrictedCommon)} = ");
		sb.Append(RestrictedCommon.ToString());
		sb.Append($", {nameof(Parent)} = ");
		sb.Append(Parent is null ? "<null>" : $"{{ {Parent.Pattern} (RCC: {RestrictedCommon}) }}");
		return sb.ToString();
	}
}
