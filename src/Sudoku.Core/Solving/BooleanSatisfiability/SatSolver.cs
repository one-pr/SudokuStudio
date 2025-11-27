#define ENABLE_VARIABLE_COMPRESSION
#define ENABLE_NOGOOD_LEARNING

namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Represents a solver that solves a puzzle using SAT algorithm.
/// <see href="https://en.wikipedia.org/wiki/Boolean_satisfiability_problem">SAT problem</see> is a way
/// to reduce complex puzzles to boolean expressions to be solved.
/// </summary>
public sealed class SatSolver : ISolver, ISolutionEnumerableSolver
{
	/// <summary>
	/// Defines an expression.
	/// </summary>
	private CnfExpression? _expression;


	/// <inheritdoc/>
	string ISolver.UriLink => "https://en.wikipedia.org/wiki/Boolean_satisfiability_problem";


	/// <inheritdoc/>
	public event EventHandler<SolverSolutionFoundEventArgs>? SolutionFound;


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		EncodeSudoku(
			grid
#if ENABLE_VARIABLE_COMPRESSION
			,
			out var mappedVariables
#endif
		);

		switch (new DpllSolver(_expression, null, mappedVariables).Solve())
		{
			case [var assignmentStates]:
			{
				// Read off which literal is true in each cell.
				result = DpllSolver.BuildSolution(assignmentStates, mappedVariables);
				return true;
			}
			case [var firstAssignmentStates, ..]:
			{
				result = DpllSolver.BuildSolution(firstAssignmentStates, mappedVariables);
				return false;
			}
			default:
			{
				result = Grid.Undefined;
				return null;
			}
		}
	}

	/// <inheritdoc/>
	void ISolutionEnumerableSolver.EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken)
	{
		EncodeSudoku(
			grid
#if ENABLE_VARIABLE_COMPRESSION
			,
			out var mappedVariables
#endif
		);
		new DpllSolver(_expression, SolutionFound, mappedVariables).Solve(cancellationToken);
	}

#if ENABLE_VARIABLE_COMPRESSION
	/// <summary>
	/// Adds CNF clauses representing Sudoku rules:
	/// <list type="number">
	/// <item>Each cell contains exactly one digit.</item>
	/// <item>Each digit appears exactly once per row, column, and block.</item>
	/// <item>Given clues are fixed by unit clauses.</item>
	/// </list>
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="mappedVariables">
	/// Indicates mapped variables that maps indices to each variable, in order to construct solution to the grid.
	/// </param>
#else
	/// <summary>
	/// Adds CNF clauses representing Sudoku rules:
	/// <list type="number">
	/// <item>Each cell contains exactly one digit.</item>
	/// <item>Each digit appears exactly once per row, column, and block.</item>
	/// <item>Given clues are fixed by unit clauses.</item>
	/// </list>
	/// </summary>
	/// <param name="grid">The grid.</param>
#endif
	[MemberNotNull(nameof(_expression))]
	private void EncodeSudoku(
		in Grid grid
#if ENABLE_VARIABLE_COMPRESSION
		,
		out Dictionary<Candidate, int> mappedVariables
#endif
	)
	{
#if ENABLE_VARIABLE_COMPRESSION
		// 0. Collect mapping indices in order to compress variables.
		mappedVariables = [];
		var virtualId = 1;
		for (var cell = 0; cell < 81; cell++)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				mappedVariables.Add(cell * 9 + digit, virtualId++);
			}
		}

		_expression = new(virtualId - 1);
#else
		_expression = new(9 * 9 * 9);
#endif

		// 1. Cell constraints (exactly one digit per cell).
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
#if ENABLE_VARIABLE_COMPRESSION
				var candidates = grid.GetCandidates(r * 9 + c);

				// At least one digit in cell: (r, c) must be one of 1..9.
				var atLeast = new List<int>();
				foreach (var d in candidates)
				{
					atLeast.Add(mappedVariables[(r * 9 + c) * 9 + d]);
				}
				_expression.AddClause([.. atLeast]);

				// At most one digit: for every pair (d1, d2), they cannot both be true.
				foreach (var pair in candidates.AllSets & 2)
				{
					_expression.AddClause(-mappedVariables[(r * 9 + c) * 9 + pair[0]], -mappedVariables[(r * 9 + c) * 9 + pair[1]]);
				}
#else
				// At least one digit in cell: (r, c) must be one of 1..9.
				var atleast = new int[9];
				for (var d = 0; d < 9; d++)
				{
					atleast[d] = MapVariable(r, c, d);
				}
				_expression.AddClause(atleast);

				// At most one digit: for every pair (d1, d2), they cannot both be true.
				for (var d1 = 0; d1 < 9; d1++)
				{
					for (var d2 = d1 + 1; d2 < 9; d2++)
					{
						_expression.AddClause(-MapVariable(r, c, d1), -MapVariable(r, c, d2));
					}
				}
#endif
			}
		}

		// 2. Row constraints (each digit once per row).
		for (var r = 0; r < 9; r++)
		{
			for (var d = 0; d < 9; d++)
			{
#if ENABLE_VARIABLE_COMPRESSION
				var atLeast = new List<int>();
				foreach (var cell in HousesMap[r + 9])
				{
					if ((grid.GetCandidates(cell) >> d & 1) != 0)
					{
						atLeast.Add(mappedVariables[cell * 9 + d]);
					}
				}
				_expression.AddClause([.. atLeast]);

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_expression.AddClause(-pair[0], -pair[1]);
				}
#else
				var atleast = new int[9];
				for (var c = 0; c < 9; c++)
				{
					atleast[c] = MapVariable(r, c, d);
				}
				_expression.AddClause(atleast);

				for (var c1 = 0; c1 < 9; c1++)
				{
					for (var c2 = c1 + 1; c2 < 9; c2++)
					{
						_expression.AddClause(-MapVariable(r, c1, d), -MapVariable(r, c2, d));
					}
				}
#endif
			}
		}

		// 3. Column constraints (each digit once per column).
		for (var c = 0; c < 9; c++)
		{
			for (var d = 0; d < 9; d++)
			{
#if ENABLE_VARIABLE_COMPRESSION
				var atLeast = new List<int>();
				foreach (var cell in HousesMap[c + 18])
				{
					if ((grid.GetCandidates(cell) >> d & 1) != 0)
					{
						atLeast.Add(mappedVariables[cell * 9 + d]);
					}
				}
				_expression.AddClause([.. atLeast]);

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_expression.AddClause(-pair[0], -pair[1]);
				}
#else
				var atleast = new int[9];
				for (var r = 0; r < 9; r++)
				{
					atleast[r] = MapVariable(r, c, d);
				}
				_expression.AddClause(atleast);

				for (var r1 = 0; r1 < 9; r1++)
				{
					for (var r2 = r1 + 1; d < 9 && r2 < 9; r2++)
					{
						_expression.AddClause(-MapVariable(r1, c, d), -MapVariable(r2, c, d));
					}
				}
#endif
			}
		}

		// 4. Block constraints (each block).
		for (var br = 0; br < 3; br++)
		{
			for (var bc = 0; bc < 3; bc++)
			{
				for (var d = 0; d < 9; d++)
				{
#if ENABLE_VARIABLE_COMPRESSION
					var atLeast = new List<int>();
					foreach (var cell in HousesMap[br * 3 + bc])
					{
						if ((grid.GetCandidates(cell) >> d & 1) != 0)
						{
							atLeast.Add(mappedVariables[cell * 9 + d]);
						}
					}
					_expression.AddClause([.. atLeast]);

					foreach (var pair in atLeast.AsSpan() & 2)
					{
						_expression.AddClause(-pair[0], -pair[1]);
					}
#else
					var atleastList = new List<int>();

					// Collect all vars in this block with digit 'd'.
					for (var r = 0; r < 3; r++)
					{
						for (var c = 0; c < 3; c++)
						{
							atleastList.Add(MapVariable(br * 3 + r, bc * 3 + c, d));
						}
					}

					_expression.AddClause([.. atleastList]);

					// At most one in the block.
					for (var i = 0; i < 9; i++)
					{
						for (var j = i + 1; j < 9; j++)
						{
							_expression.AddClause(-atleastList[i], -atleastList[j]);
						}
					}
#endif
				}
			}
		}

		// 5. Initial clues as unit clauses.
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
				if (grid.GetDigit(r * 9 + c) is var d and not -1)
				{
					// Force (r, c) = d by adding single literal clause.
					_expression.AddClause(
#if ENABLE_VARIABLE_COMPRESSION
						mappedVariables[(r * 9 + c) * 9 + d]
#else
						MapVariable(r, c, d)
#endif
					);
				}
			}
		}
	}

#if !ENABLE_VARIABLE_COMPRESSION
	/// <summary>
	/// Maps 3D coordinates (row, column, digit) to a unique SAT variable index.
	/// Indices start from 1 up to 729.
	/// </summary>
	private int MapVariable(int r, int c, int d) => r * 81 + c * 9 + d + 1;
#endif
}

/// <summary>
/// Implements a simple SAT solver using the DPLL algorithm with unit propagation.
/// For more information about DPLL algorithm, please visit <see href="https://en.wikipedia.org/wiki/DPLL_algorithm">this link</see>.
/// </summary>
/// <param name="_expression">Indicates the backing expression.</param>
/// <param name="_solutionFoundEventHandler">Indicates event handler for solution found.</param>
/// <param name="_mappedVariables">Indicates the mapped variables.</param>
file sealed class DpllSolver(
	CnfExpression _expression,
	EventHandler<SolverSolutionFoundEventArgs>? _solutionFoundEventHandler,
	Dictionary<Candidate, int>? _mappedVariables
)
{
	/// <summary>
	/// After solving, retrieve the assignment array
	/// (index: <c>variable</c> is either <see langword="true"/> or <see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// This property represents the assignment values. The result value only represents for 3 values:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>Represents <c>true</c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>Represents <c>false</c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Leaves unassigned</description>
	/// </item>
	/// </list>
	/// This array starts at index 1. Please use 1-based indexing to operate variables.
	/// </remarks>
	private bool?[] _assignmentStates = new bool?[_expression.VariablesCount + 1];

#if ENABLE_NOGOOD_LEARNING
	/// <summary>
	/// Decision stack + snapshots for naive learning/backjump.
	/// Each decision level records which variable was chosen as decision and its intended value.
	/// </summary>
	private List<(int Variable, bool Value)>? _decisionStack;

	/// <summary>
	/// For each decision level we keep a snapshot of _assignment taken <b>before</b> the decision,
	/// so we can restore (backjump) quickly to that point.
	/// </summary>
	private List<bool?[]>? _decisionSnapshots;
#endif


	/// <summary>
	/// Try to find a satisfying assignment.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	public List<bool?[]> Solve(CancellationToken cancellationToken = default)
	{
#if ENABLE_NOGOOD_LEARNING
		_decisionStack = [];
		_decisionSnapshots = [];
#endif
		var solutions = new List<bool?[]>();
		Backtracking(solutions, cancellationToken);
		return solutions;
	}


#if ENABLE_NOGOOD_LEARNING
	/// <summary>
	/// Performs DPLL recursive method. DPLL recursive routine:
	/// <list type="number">
	/// <item>Perform unit propagation to simplify.</item>
	/// <item>
	/// If conflict, check decision level:
	/// <list type="number">
	/// <item>
	/// If there's at least one decision level learn a clause
	/// forbidding the current decision literal at that level (negation),
	/// add it to the expression and backjump by restoring the snapshot of the previous level.
	/// </item>
	/// <item>Otherwise, failed. Backtrack (return <see langword="false"/>).</item>
	/// </list>
	/// </item>
	/// <item>If all variables assigned, expression is satisfied.</item>
	/// <item>Otherwise, pick an unassigned variable and branch on true/false.</item>
	/// </list>
	/// </summary>
	/// <param name="solutions">The solutions.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
#else
	/// <summary>
	/// Performs DPLL recursive method. DPLL recursive routine:
	/// <list type="number">
	/// <item>Perform unit propagation to simplify.</item>
	/// <item>If conflict, backtrack (return <see langword="false"/>).</item>
	/// <item>If all variables assigned, expression is satisfied.</item>
	/// <item>Otherwise, pick an unassigned variable and branch on true/false.</item>
	/// </list>
	/// </summary>
	/// <param name="solutions">The solutions.</param>
	/// <param name="cancellationToken">The cancellation token.</param>
#endif
	private bool Backtracking(List<bool?[]> solutions, CancellationToken cancellationToken)
	{
#if ENABLE_NOGOOD_LEARNING
		Debug.Assert(_decisionStack is not null);
		Debug.Assert(_decisionSnapshots is not null);
#endif

		if (!UnitPropagation())
		{
#if ENABLE_NOGOOD_LEARNING
			// Conflict detected during propagation.
			// If there's no decision to backjump to => UNSAT.
			if (_decisionStack.Count == 0)
			{
				return false;
			}

			// Naive nogood: learn a clause that is the negation of the decision literal(s)
			// at the current (deepest) decision level. In this simple solver each decision level
			// corresponds to exactly one decision variable, so we negate that one.
			var (decVar, decVal) = _decisionStack[^1];

			// If decVal == true, then decision literal was +decVar, so learned is -decVar.
			// If decVal == false, decision literal was -decVar, so learned is +decVar.
			var learnedLiteral = decVal ? -decVar : decVar;

			// Add the learned unit clause to the expression to prevent repeating this decision.
			_expression.AddClause(learnedLiteral);

			// Backjump one decision level: restore assignment to snapshot BEFORE that decision.
			var restoreSnapshot = _decisionSnapshots[^1];
			_assignmentStates = restoreSnapshot[..];

			// Pop that decision level records.
			_decisionStack.RemoveAt(^1);
			_decisionSnapshots.RemoveAt(^1);

			// Return false so upper recursion can continue (it will either try alternate branch or learn more).
			return false;
#else
			// Conflict detected.
			return false;
#endif
		}

		// Find a variable index that has not been assigned yet(0), or -1 if all variables are assigned.
		var variable = -1;
		for (var i = 1; i <= _expression.VariablesCount; i++)
		{
			if (_assignmentStates[i] is null)
			{
				variable = i;
				break;
			}
		}
		if (variable == -1)
		{
			// All variables assigned without conflict => SAT.
			solutions.Add(_assignmentStates[..]);
			_solutionFoundEventHandler?.Invoke(this, new(BuildSolution(_assignmentStates, _mappedVariables!)));
			return _solutionFoundEventHandler is null;
		}

		if (!cancellationToken)
		{
			// Canceled.
			return false;
		}

		// Save state for backtracking.
		var snapshotBeforeDecision = _assignmentStates[..];

#if ENABLE_NOGOOD_LEARNING
		_decisionSnapshots.Add(snapshotBeforeDecision);
		_decisionStack.Add((variable, true)); // assume true on first try
#endif

		// Try assigning 'variable' = true.
		_assignmentStates[variable] = true;
		if (Backtracking(solutions, cancellationToken))
		{
			return true;
		}

		if (!cancellationToken)
		{
			// Canceled.
			return false;
		}

		// Backtrack and try 'variable' = false.
#if ENABLE_NOGOOD_LEARNING
		_assignmentStates = snapshotBeforeDecision;
		_decisionStack[^1] = (variable, false); // switch the recorded decision value to false
		_assignmentStates[variable] = false;
#else
		_assignmentStates[variable] = false;
#endif
		if (Backtracking(solutions, cancellationToken))
		{
			return true;
		}

		if (!cancellationToken)
		{
			// Canceled.
			return false;
		}

		// Both assignments led to conflict => unsatisfiable under current partial assignment.
#if ENABLE_NOGOOD_LEARNING
		_assignmentStates = _decisionSnapshots[^1][..];
		_decisionStack.RemoveAt(^1);
		_decisionSnapshots.RemoveAt(^1);
#else
		_assignmentStates = snapshotBeforeDecision;
#endif
		return false;
	}

	/// <summary>
	/// Unit propagation:
	/// Repeatedly scan for clauses where only one literal is unassigned and all others <see langword="false"/>,
	/// then assign that literal to <see langword="true"/> (to satisfy the clause).
	/// Returns <see langword="false"/> if a clause becomes unsatisfiable (all literals <see langword="false"/>).
	/// </summary>
	private bool UnitPropagation()
	{
		bool isChanged;
		do
		{
			isChanged = false;
			foreach (var clause in _expression)
			{
				var (unassignedCount, unassignedLiteral, clauseSatisfied) = (0, 0, false);

				// Check each literal in the clause.
				foreach (var literal in clause)
				{
					var variable = Math.Abs(literal);
					var sign = literal > 0;
					if (_assignmentStates[variable] == sign)
					{
						clauseSatisfied = true; // Clause is already satisfied.
						break;
					}
					if (_assignmentStates[variable] is null)
					{
						unassignedCount++;
						unassignedLiteral = literal;
					}
				}

				if (clauseSatisfied)
				{
					continue;
				}

				// If no unassigned lits left and none true => conflict.
				if (unassignedCount == 0)
				{
					return false;
				}

				// Exactly one unassigned literal => must be set to satisfy the clause.
				if (unassignedCount == 1)
				{
					var variable = Math.Abs(unassignedLiteral);
					var sign = unassignedLiteral > 0;
					_assignmentStates[variable] = sign;
					isChanged = true;
				}
			}
		} while (isChanged);
		return true;
	}


	/// <summary>
	/// Build solution via the specified states and mapped variables.
	/// </summary>
	/// <param name="assignmentStates">The assignment states.</param>
	/// <param name="mappedVariables">Mapped variables.</param>
	/// <returns>The grid.</returns>
	internal static Grid BuildSolution(bool?[] assignmentStates, Dictionary<Candidate, int> mappedVariables)
	{
		var result = Grid.Empty;
		for (var row = 0; row < 9; row++)
		{
			for (var column = 0; column < 9; column++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
#if ENABLE_VARIABLE_COMPRESSION
					if (mappedVariables.TryGetValue((row * 9 + column) * 9 + digit, out var variable)
						&& assignmentStates[variable] is true)
#else
					if (assignmentStates[MapVariable(row, column, digit)] is true)
#endif
					{
						result.SetDigit(row * 9 + column, digit);
					}
				}
			}
		}

		result.Fix();
		return result;
	}
}
