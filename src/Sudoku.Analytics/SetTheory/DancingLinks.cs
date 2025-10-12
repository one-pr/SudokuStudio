namespace Sudoku.SetTheory;

/// <summary>
/// Dancing Links implementation that supports primary (must-cover exactly once) and secondary (at-most-one) columns.
/// <list type="bullet">
/// <item>Columns are 0..(colCount - 1)</item>
/// <item>You add rows by listing the column indices this row has 1 in</item>
/// <item>Primary columns are covered/selected by algorithm; secondary columns are "optional"</item>
/// </list>
/// </summary>
internal sealed class DancingLinks
{
	/// <summary>
	/// Indicates head.
	/// </summary>
	private readonly ColumnHeader _head;

	/// <summary>
	/// Indicates columns.
	/// </summary>
	private readonly ColumnHeader[] _columns;

	/// <summary>
	/// Indicates row nodes, keeping references to row's first node.
	/// </summary>
	private readonly List<Node> _rowNodes = [];

	/// <summary>
	/// Indicates list of row IDs chosen. This will be a solution stored.
	/// </summary>
	private readonly List<Candidate> _solutionStack = [];

	/// <summary>
	/// Indicates results.
	/// </summary>
	private readonly List<Permutation> _results = [];

	/// <summary>
	/// Indicates limit maximum solutions to be checked.
	/// </summary>
	private int _limitMaxSolutions = int.MaxValue;


	/// <summary>
	/// Initializes a <see cref="DancingLinks"/> instance via columns count and is-primary property to every column header.
	/// </summary>
	/// <param name="columnCount">The number of columns.</param>
	/// <param name="isPrimary">Is-primary property to every column header.</param>
	public DancingLinks(int columnCount, ReadOnlySpan<bool> isPrimary)
	{
		ArgumentOutOfRangeException.ThrowIfNotEqual(columnCount, isPrimary.Length);

		_head = new ColumnHeader(-1, true);
		_head.L = _head.R = _head;
		_columns = new ColumnHeader[columnCount];
		for (var i = 0; i < columnCount; i++)
		{
			var ch = new ColumnHeader(i, isPrimary[i]);
			{
				// Link into header's horizontal list.
				ch.R = _head;
				ch.L = _head.L;
				_head.L.R = ch;
				_head.L = ch;
				ch.U = ch.D = ch;
			}

			_columns[i] = ch;
		}
	}

	/// <summary>
	/// Add a row with given <paramref name="rowId"/> and column indices <paramref name="colIndices"/> (may be empty list).
	/// </summary>
	/// <param name="rowId">The row ID.</param>
	/// <param name="colIndices">Column indices.</param>
	public void AddRow(Candidate rowId, ReadOnlySpan<Candidate> colIndices)
	{
		var first = default(Node);
		var prev = default(Node);
		foreach (var cidx in colIndices)
		{
			ArgumentOutOfRangeException.ThrowIfNegative(cidx);
			ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(cidx, _columns.Length);

			var col = _columns[cidx];
			var node = new Node { C = col, RowId = rowId };
			{
				// Vertical link: insert node at bottom of column.
				node.D = col;
				node.U = col.U;
				col.U.D = node;
				col.U = node;
				col.Size++;
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
			var placeholder = new Node
			{
				C = null,
				RowId = rowId
			};
			placeholder.L = placeholder.R = placeholder;
			first = placeholder;
		}
		_rowNodes.Add(first);
	}

	/// <summary>
	/// Do cover operation.
	/// </summary>
	/// <param name="c">The column.</param>
	private void Cover(ColumnHeader c)
	{
		// Remove column header.
		c.R.L = c.L;
		c.L.R = c.R;

		// For each row with a 1 in column c.
		for (var i = c.D; !ReferenceEquals(i, c); i = i.D)
		{
			// For each node in that row, unlink vertically.
			for (var j = i.R; !ReferenceEquals(j, i); j = j.R)
			{
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
	private void Uncover(ColumnHeader c)
	{
		// Inverse of Cover: reinsert nodes and header.
		for (var i = c.U; !ReferenceEquals(i, c); i = i.U)
		{
			for (var j = i.L; !ReferenceEquals(j, i); j = j.L)
			{
				j.C.Size++;
				j.D.U = j;
				j.U.D = j;
			}
		}

		c.R.L = c;
		c.L.R = c;
	}

	/// <summary>
	/// Choose a column.
	/// </summary>
	/// <returns>The chosen column. If failed to choose, <see langword="null"/> will be returned.</returns>
	private ColumnHeader? ChooseColumn()
	{
		// Pick primary column of minimal size.
		var best = default(ColumnHeader);
		var bestSize = int.MaxValue;
		for (var c = _head.R; !ReferenceEquals(c, _head); c = c.R)
		{
			var ch = (ColumnHeader)c;
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
	/// Enumerate solutions up to <paramref name="maxSolutions"/>.
	/// The row IDs in each solution correspond to those given in <see cref="AddRow(Candidate, ReadOnlySpan{Candidate})"/>.
	/// </summary>
	/// <seealso cref="AddRow(Candidate, ReadOnlySpan{Candidate})"/>
	public List<Permutation> Solve(int maxSolutions)
	{
		_limitMaxSolutions = maxSolutions;
		_results.Clear();
		_solutionStack.Clear();

		Search(0);

		return _results;
	}

	/// <summary>
	/// Perform depth-first searching to find solutions.
	/// </summary>
	/// <param name="k">The current index.</param>
	private void Search(int k)
	{
		// If no primary columns remain, we have a valid partial cover -> record solution.
		var hasPrimary = false;
		for (var c = _head.R; !ReferenceEquals(c, _head); c = c.R)
		{
			var ch = (ColumnHeader)c;
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
			_solutionStack.Add(r.RowId);

			// Cover other columns in this row.
			for (var j = r.R; !ReferenceEquals(j, r); j = j.R)
			{
				Cover(j.C);
			}

			// Recurse.
			if (_results.Count < _limitMaxSolutions)
			{
				Search(k + 1);
			}

			// Backtrack.
			for (var j = r.L; !ReferenceEquals(j, r); j = j.L)
			{
				Uncover(j.C);
			}
			_solutionStack.RemoveAt(^1);

			if (_results.Count >= _limitMaxSolutions)
			{
				break;
			}
		}
		Uncover(col);
	}
}
