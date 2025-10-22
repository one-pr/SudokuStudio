#if SET_THROW_IF_LIMIT_IS_REACHED && !SET_LIMIT_MAX_SOLUTIONS
#warning Symbol 'SET_THROW_IF_LIMIT_IS_REACHED' cannot work if 'SET_LIMIT_MAX_SOLUTIONS' is unset. 'SET_LIMIT_MAX_SOLUTIONS' now re-defined.
#define SET_LIMIT_MAX_SOLUTIONS
#endif

namespace Sudoku.SetTheory;

/// <summary>
/// Dancing Links implementation that supports primary (must-cover exactly once) and secondary (at-most-one) columns.
/// </summary>
/// <!--Primary columns are covered/selected by algorithm; secondary columns are "optional".-->
internal sealed class SetTheoryDancingLinks
{
	/// <summary>
	/// Indicates head.
	/// </summary>
	private readonly SetTheoryColumnHeader _head;

	/// <summary>
	/// Indicates columns.
	/// </summary>
	private readonly SetTheoryColumnHeader[] _columns;

	/// <summary>
	/// Indicates row nodes, keeping references to row's first node.
	/// </summary>
	private readonly List<SetTheoryNode> _rowNodes = [];

	/// <summary>
	/// Indicates list of row IDs chosen. This will be a solution stored.
	/// </summary>
	private readonly List<Candidate> _solutionStack = [];

	/// <summary>
	/// Indicates results.
	/// </summary>
	private readonly List<Permutation> _results = [];

#if SET_LIMIT_MAX_SOLUTIONS
	/// <summary>
	/// Indicates limit maximum solutions to be checked.
	/// </summary>
	private int _limitMaxSolutions = int.MaxValue;
#endif


	/// <summary>
	/// Initializes a <see cref="SetTheoryDancingLinks"/> instance via columns count and is-primary property to every column header.
	/// </summary>
	/// <param name="columnCount">The number of columns.</param>
	/// <param name="isPrimary">Is-primary property to every column header.</param>
	public SetTheoryDancingLinks(int columnCount, ReadOnlySpan<bool> isPrimary)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(columnCount, isPrimary.Length);

		_head = new SetTheoryColumnHeader(-1, true);
		_head.L = _head.R = _head;
		_columns = new SetTheoryColumnHeader[columnCount];
		for (var i = 0; i < columnCount; i++)
		{
			var header = new SetTheoryColumnHeader(i, isPrimary[i]);
			{
				// Link into header's horizontal list.
				header.R = _head;
				header.L = _head.L;
				_head.L.R = header;
				_head.L = header;
				header.U = header.D = header;
			}
			_columns[i] = header;
		}
	}


	/// <summary>
	/// Enumerate solutions up to <paramref name="maxSolutions"/>.
	/// The row IDs in each solution correspond to those
	/// given in <see cref="AddRow(Candidate, ReadOnlySpan{Candidate})"/>.
	/// </summary>
	/// <param name="maxSolutions">
	/// Specify the desired number of solutions this algorithm can detect.
	/// If limit is reached, an exception of type <see cref="DancingLinksTooComplexException"/> will be thrown
	/// if symbol <c>SET_THROW_IF_LIMIT_IS_REACHED</c> is configured, or return all found solutions if not.
	/// </param>
	/// <exception cref="DancingLinksTooComplexException">
	/// Throws when pattern is too complex to be calculated,
	/// and symbol <c>SET_THROW_IF_LIMIT_IS_REACHED</c> is enabled.
	/// </exception>
	/// <seealso cref="AddRow(Candidate, ReadOnlySpan{Candidate})"/>
	public List<Permutation> Solve(int maxSolutions)
	{
#if SET_LIMIT_MAX_SOLUTIONS
		_limitMaxSolutions = maxSolutions;
#endif
		_results.Clear();
		_solutionStack.Clear();

		Search(0);

		return _results;
	}

	/// <summary>
	/// Add a row with given <paramref name="rowId"/> and column indices <paramref name="columnIndices"/> (may be empty list).
	/// </summary>
	/// <param name="rowId">The row ID.</param>
	/// <param name="columnIndices">Column indices.</param>
	public void AddRow(Candidate rowId, ReadOnlySpan<Candidate> columnIndices)
	{
		var first = default(SetTheoryNode);
		var prev = default(SetTheoryNode);
		foreach (var columnIndex in columnIndices)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(columnIndex);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(columnIndex, _columns.Length);

			var column = _columns[columnIndex];
			var node = new SetTheoryNode { C = column, RowId = rowId };
			{
				// Vertical link: insert node at bottom of column.
				node.D = column;
				node.U = column.U;
				Debug.Assert(column.U is not null);
				column.U.D = node;
				column.U = node;
				column.Size++;
			}

			// Horizontal link among row.
			if (first is null)
			{
				first = node;
				prev = node;
				node.L = node.R = node;
			}
			else
			{
				node.R = first;
				node.L = prev;
				prev!.R = node;
				first.L = node;
				prev = node;
			}
		}

		// Keep reference (even if first is null, we add a placeholder node to map rowId).
		if (first is null)
		{
			// Create a singleton placeholder node not linked to any column (for row with no columns).
			var placeholder = new SetTheoryNode { C = null, RowId = rowId };
			placeholder.L = placeholder.R = placeholder;
			first = placeholder;
		}
		_rowNodes.Add(first);
	}

	/// <summary>
	/// Do cover operation.
	/// </summary>
	/// <param name="c">The column.</param>
	private void Cover(SetTheoryColumnHeader c)
	{
		// Remove column header.
		Debug.Assert(c is { R: not null, L: not null });
		c.R.L = c.L;
		c.L.R = c.R;

		// For each row with a 1 in column c.
		for (var i = c.D; !ReferenceEquals(i, c); i = i.D)
		{
			Debug.Assert(i is not null);

			// For each node in that row, unlink vertically.
			for (var j = i.R; !ReferenceEquals(j, i); j = j.R)
			{
				Debug.Assert(j is { D: not null, U: not null, C: not null });
				j.D.U = j.U;
				j.U.D = j.D;
				j.C.Size--;
			}
		}
	}

	/// <summary>
	/// Do uncover operation.
	/// </summary>
	/// <param name="c">The column.</param>
	private void Uncover(SetTheoryColumnHeader c)
	{
		// Inverse of Cover: reinsert nodes and header.
		for (var i = c.U; !ReferenceEquals(i, c); i = i.U)
		{
			Debug.Assert(i is not null);
			for (var j = i.L; !ReferenceEquals(j, i); j = j.L)
			{
				Debug.Assert(j is { D: not null, U: not null, C: not null });
				j.C.Size++;
				j.D.U = j;
				j.U.D = j;
			}
		}

		Debug.Assert(c is { R: not null, L: not null });
		c.R.L = c;
		c.L.R = c;
	}

	/// <summary>
	/// Choose a column.
	/// </summary>
	/// <returns>The chosen column. If failed to choose, <see langword="null"/> will be returned.</returns>
	private SetTheoryColumnHeader? ChooseColumn()
	{
		// Pick primary column of minimal size.
		var best = default(SetTheoryColumnHeader);
		var bestSize = int.MaxValue;
		for (var c = _head.R; !ReferenceEquals(c, _head); c = c.R)
		{
			Debug.Assert(c is not null);
			var ch = (SetTheoryColumnHeader)c;
			if (!ch.IsPrimary)
			{
				// Skip secondary columns.
				continue;
			}
			if (ch.Size < bestSize)
			{
				bestSize = ch.Size;
				best = ch;
				if (bestSize <= 1)
				{
					// Cannot get better than 0/1.
					break;
				}
			}
		}
		return best;
	}

	/// <summary>
	/// Perform depth-first searching to find solutions.
	/// </summary>
	/// <param name="k">The current index.</param>
	/// <exception cref="DancingLinksTooComplexException">Throws when pattern is too complex.</exception>
	private void Search(int k)
	{
		// If no primary columns remain, we have a valid partial cover -> record solution.
		var hasPrimary = false;
		for (var c = _head.R; !ReferenceEquals(c, _head); c = c.R)
		{
			Debug.Assert(c is not null);
			var ch = (SetTheoryColumnHeader)c;
			if (ch.IsPrimary)
			{
				hasPrimary = true;
				break;
			}
		}
		if (!hasPrimary)
		{
			// Record current copy.
			_results.Add(new(_solutionStack.ToArray()));
			return;
		}

		if (ChooseColumn() is not { Size: not 0 } col)
		{
			// Skip for two cases:
			//   1) No primary column available -> dead end (col is null)
			//   2) If a primary column has size 0, cannot satisfy (col.Size == 0)
			return;
		}

		Cover(col);
		for (var r = col.D; !ReferenceEquals(r, col); r = r.D)
		{
			Debug.Assert(r is not null);
			_solutionStack.Add(r.RowId);

			// Cover other columns in this row.
			for (var j = r.R; !ReferenceEquals(j, r); j = j.R)
			{
				Debug.Assert(j is { C: not null });
				Cover(j.C);
			}

			// Recurse.
#if SET_LIMIT_MAX_SOLUTIONS
			if (_results.Count < _limitMaxSolutions)
			{
				Search(k + 1);
			}
#else
			Search(k + 1);
#endif

			// Backtrack.
			for (var j = r.L; !ReferenceEquals(j, r); j = j.L)
			{
				Debug.Assert(j is { C: not null });
				Uncover(j.C);
			}
			_solutionStack.RemoveAt(^1);

#if SET_LIMIT_MAX_SOLUTIONS
			if (_results.Count >= _limitMaxSolutions)
			{
#if SET_THROW_IF_LIMIT_IS_REACHED
				throw new DancingLinksTooComplexException();
#else
				break;
#endif
			}
#endif
		}
		Uncover(col);
	}
}
