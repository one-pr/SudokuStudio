#define ENABLE_NOGOOD_LEARNING
#define ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING

namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Represents a solver that solves a puzzle using SAT algorithm.
/// <see href="https://en.wikipedia.org/wiki/Boolean_satisfiability_problem">SAT problem</see> is a way
/// to reduce complex puzzles to boolean expressions to be solved.
/// </summary>
public sealed class SatSolver : ISolver, ISolutionEnumerableSolver<SatSolver>
{
	/// <summary>
	/// Defines an expression.
	/// </summary>
	private CnfExpression? _expression;


	/// <inheritdoc/>
	string ISolver.UriLink => "https://en.wikipedia.org/wiki/Boolean_satisfiability_problem";


	/// <inheritdoc/>
	public event EventHandler<SatSolver, SolverSolutionFoundEventArgs>? SolutionFound;


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		EncodeSudoku(grid, out var mappedVariables);

		(result, var @return) = new Dpll(_expression, null, mappedVariables, this).Solve() switch
		{
			[var assignmentStates] => (Dpll.BuildSolution(assignmentStates, mappedVariables), true),
			[var firstAssignmentStates, ..] => (Dpll.BuildSolution(firstAssignmentStates, mappedVariables), false),
			_ => (Grid.Undefined, (bool?)null)
		};
		return @return;
	}

	/// <inheritdoc/>
	void ISolutionEnumerableSolver<SatSolver>.EnumerateSolutionsCore(Grid grid, CancellationToken cancellationToken)
	{
		EncodeSudoku(grid, out var mappedVariables);
		new Dpll(_expression, SolutionFound, mappedVariables, this).Solve(cancellationToken);
	}

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
	[MemberNotNull(nameof(_expression))]
	private void EncodeSudoku(in Grid grid, out Dictionary<Candidate, int> mappedVariables)
	{
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

		// 1. Cell constraints (exactly one digit per cell).
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
				var candidates = grid.GetCandidates(r * 9 + c);

				// At least one digit in cell: (r, c) must be one of 1..9.
				var atLeast = new List<int>();
				foreach (var d in candidates)
				{
					atLeast.Add(mappedVariables[(r * 9 + c) * 9 + d]);
				}
				_expression.AddClause(atLeast.AsMemory());

				// At most one digit: for every pair (d1, d2), they cannot both be true.
				foreach (var pair in candidates.AllSets & 2)
				{
					_expression.AddClause(
						(int[])[
							-mappedVariables[(r * 9 + c) * 9 + pair[0]],
							-mappedVariables[(r * 9 + c) * 9 + pair[1]]
						]
					);
				}
			}
		}

		// 2. Row constraints (each digit once per row).
		for (var r = 0; r < 9; r++)
		{
			for (var d = 0; d < 9; d++)
			{
				var atLeast = new List<int>();
				foreach (var cell in HousesMap[r + 9])
				{
					if ((grid.GetCandidates(cell) >> d & 1) != 0)
					{
						atLeast.Add(mappedVariables[cell * 9 + d]);
					}
				}
				_expression.AddClause(atLeast.AsMemory());

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_expression.AddClause((int[])[-pair[0], -pair[1]]);
				}
			}
		}

		// 3. Column constraints (each digit once per column).
		for (var c = 0; c < 9; c++)
		{
			for (var d = 0; d < 9; d++)
			{
				var atLeast = new List<int>();
				foreach (var cell in HousesMap[c + 18])
				{
					if ((grid.GetCandidates(cell) >> d & 1) != 0)
					{
						atLeast.Add(mappedVariables[cell * 9 + d]);
					}
				}
				_expression.AddClause(atLeast.AsMemory());

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_expression.AddClause((int[])[-pair[0], -pair[1]]);
				}
			}
		}

		// 4. Block constraints (each block).
		for (var br = 0; br < 3; br++)
		{
			for (var bc = 0; bc < 3; bc++)
			{
				for (var d = 0; d < 9; d++)
				{
					var atLeast = new List<int>();
					foreach (var cell in HousesMap[br * 3 + bc])
					{
						if ((grid.GetCandidates(cell) >> d & 1) != 0)
						{
							atLeast.Add(mappedVariables[cell * 9 + d]);
						}
					}
					_expression.AddClause(atLeast.AsMemory());

					foreach (var pair in atLeast.AsSpan() & 2)
					{
						_expression.AddClause((int[])[-pair[0], -pair[1]]);
					}
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
					_expression.AddClause(Array.Single(mappedVariables[(r * 9 + c) * 9 + d]));
				}
			}
		}
	}
}

/// <summary>
/// Implements a simple SAT solver using the DPLL algorithm (<i>Davis–Putnam–Logemann–Loveland algorithm</i>)
/// with unit propagation.
/// For more information about DPLL algorithm, please visit <see href="https://en.wikipedia.org/wiki/DPLL_algorithm">this link</see>.
/// </summary>
file sealed class Dpll
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
	/// <description>Represents <c><see langword="true"/></c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>Represents <c><see langword="false"/></c> literal</description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Leaves unassigned</description>
	/// </item>
	/// </list>
	/// This array starts at index 1. Please use 1-based indexing to operate variables.
	/// </remarks>
#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
	private readonly bool?[] _assignmentStates;
#else
	private bool?[] _assignmentStates;
#endif

	/// <summary>
	/// Indicates event handler for solution found.
	/// </summary>
	private readonly EventHandler<SatSolver, SolverSolutionFoundEventArgs>? _solutionFoundEventHandler;

	/// <summary>
	/// Indicates the backing expression.
	/// </summary>
	private readonly CnfExpression _expression;

	/// <summary>
	/// Indicates the mapped variables.
	/// </summary>
	private readonly Dictionary<Candidate, int>? _mappedVariables;

	/// <summary>
	/// The parent solver.
	/// </summary>
	private readonly SatSolver? _parentSolver;

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
	/// <summary>
	/// Decision level per variable (0..).
	/// </summary>
	private readonly int[] _variableLevel;

	/// <summary>
	/// Signed literals in assignment order
	/// (<c>+v</c> means v = <see langword="true"/>, <c>-v</c> means v = <see langword="false"/>).
	/// </summary>
	private readonly List<int> _trail;

	/// <summary>
	/// Clause that implied this variable (<see langword="null"/> for decision variables).
	/// </summary>
	private readonly ReadOnlyMemory<int>?[] _antecedent;

	/// <summary>
	/// Stack: variable chosen at each decision level (for bookkeeping).
	/// </summary>
	private readonly List<int> _decisionLevels;

	/// <summary>
	/// Indicates the current decision level.
	/// </summary>
	private int _decisionLevel;
#endif

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
	/// Initializes a <see cref="Dpll"/> instance via the specified instances.
	/// </summary>
	/// <param name="expression"><inheritdoc cref="_expression" path="/summary"/></param>
	/// <param name="solutionFoundEventHandler"><inheritdoc cref="_solutionFoundEventHandler" path="/summary"/></param>
	/// <param name="mappedVariables">
	/// <para><inheritdoc cref="_mappedVariables" path="/summary"/></para>
	/// <para>
	/// The value can be <see langword="null"/> if <paramref name="solutionFoundEventHandler"/> is <see langword="null"/>.
	/// </para>
	/// </param>
	/// <param name="parentSolver">
	/// <para><inheritdoc cref="_parentSolver" path="/summary"/></para>
	/// <para>
	/// The value can be <see langword="null"/> if <paramref name="solutionFoundEventHandler"/> is <see langword="null"/>.
	/// </para>
	/// </param>
	public Dpll(
		CnfExpression expression,
		EventHandler<SatSolver, SolverSolutionFoundEventArgs>? solutionFoundEventHandler,
		Dictionary<Candidate, int>? mappedVariables,
		SatSolver? parentSolver
	)
	{
		_expression = expression;
		_assignmentStates = new bool?[_expression.VariablesCount + 1];
		_solutionFoundEventHandler = solutionFoundEventHandler;
		_mappedVariables = mappedVariables;
		_parentSolver = parentSolver;

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		_trail = [];
		_decisionLevels = [];
		_variableLevel = new int[_expression.VariablesCount + 1];
		_antecedent = new ReadOnlyMemory<int>?[_expression.VariablesCount + 1];
#endif
	}


	/// <summary>
	/// Try to find a satisfying assignment.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	public List<bool?[]> Solve(CancellationToken cancellationToken = default)
	{
#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		// Init CDCL data.
		_assignmentStates.AsSpan().Clear();
		_variableLevel.AsSpan().Clear();
		_antecedent.AsSpan().Clear();
		_trail.Clear();
		_decisionLevels.Clear();
		_decisionLevel = 0;
#endif

#if ENABLE_NOGOOD_LEARNING
		_decisionStack = [];
		_decisionSnapshots = [];
#endif
		var solutions = new List<bool?[]>();
		Backtracking(solutions, cancellationToken);
		return solutions;
	}


#if ENABLE_NOGOOD_LEARNING || ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
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

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		// It seems that two variables are not null anyway.
		//Debug.Assert(_varLevel is not null && _antecedent is not null);
#endif

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		// 1) propagate and handle conflicts (CDCL loop).
		// If propagation finds a conflict, do conflict analysis + learning + backjump and continue.
		while (true)
		{
			if (UnitPropagation() is not { } conflictClause)
			{
				// No conflict, continue to branching.
				break;
			}

			// Conflict detected.
			if (_decisionLevel == 0)
			{
				// Unsatisfiable at root.
				return false;
			}

			// Perform First-UIP conflict analysis -> <c>learnedClause</c>.
			if (ConflictAnalyze(conflictClause.ToArray()) is not { Length: not 0 } learned)
			{
				// Something degenerate (tautology or empty) -> treat as UNSAT.
				return false;
			}

			// Add learned clause to the expression.
			_expression.AddClause(learned.AsMemory());

			// Compute backjump level.
			var backjumpLevel = 0;
			var litAtCurrLevel = 0;
			foreach (var literal in learned)
			{
				var v = Math.Abs(literal);
				var level = _variableLevel[v];
				if (level == _decisionLevel)
				{
					// The UIP literal (there will be exactly one).
					litAtCurrLevel = literal;
				}
				else
				{
					backjumpLevel = Math.Max(backjumpLevel, level);
				}
			}

			// Backjump: undo assignments whose level > <c>backjumpLevel</c>.
			BacktrackToLevel(backjumpLevel);

			// After backjump, the learned clause is unit (the UIP literal) and must be propagated:
			// Find the literal in learned that is unassigned now (the UIP).
			// Assign it implied by learned clause.
			// Note: antecedent set to learned clause; level = backjumpLevel.
			var unitLiteral = 0;
			var unassignedCount = 0;
			foreach (var literal in learned)
			{
				var v = Math.Abs(literal);
				if (_assignmentStates[v] is null)
				{
					unitLiteral = literal;
					unassignedCount++;
				}
			}

			// Defensive: normally unassignedCount == 1.
			if (unassignedCount == 1)
			{
				var v = Math.Abs(unitLiteral);
				var value = unitLiteral > 0;
				_assignmentStates[v] = value;
				_variableLevel[v] = backjumpLevel;
				_antecedent[v] = learned.AsMemory();
				_trail.Add(unitLiteral);
			}
			//else
			//{
			//	// Fallback: just continue; next iteration will re-run propagation.
			//	continue;
			//}

			// Continue propagation loop (<c>UnitPropagation</c> will be called again).
		}
#else
		if (!UnitPropagation())
		{
#if ENABLE_NOGOOD_LEARNING
			// Conflict detected during propagation.
			// If there's no decision to backjump to => UNSAT.
			if (_decisionStack.Count == 0)
			{
				return false;
			}

			// Naive nogood: learn a clause that is the negation of the decision literal(s) at the current (deepest) decision level.
			// In this simple solver each decision level corresponds to exactly one decision variable, so we negate that one.
			var (decisionVariable, decisionValue) = _decisionStack[^1];

			// <list type="bullet">
			// <item>
			// If <c>decisionValue == true</c>, decision literal was <c>+decisionVariable</c>, so learned is <c>-decisionVariable</c>
			// </item>
			// <item>
			// If <c>decisionValue == false</c>, decision literal was <c>-decisionVariable</c>, so learned is <c>+decisionVariable</c>
			// </item>
			// </list>
			var learnedLiteral = decisionValue ? -decisionVariable : decisionVariable;

			// Add the learned unit clause to the expression to prevent repeating this decision.
			_expression.AddClause(Array.Single(learnedLiteral));

			// Backjump one decision level: restore assignment to snapshot <i>before</i> that decision.
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
#endif

		// 2) Check if all vars assigned -> solution.
		// Find a variable index that has not been assigned yet (0), or -1 if all variables are assigned.
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
			Debug.Assert(_parentSolver is not null);
			Debug.Assert(_mappedVariables is not null);

			solutions.Add(_assignmentStates[..]);
			_solutionFoundEventHandler?.Invoke(_parentSolver, new(BuildSolution(_assignmentStates, _mappedVariables)));
			return _solutionFoundEventHandler is null;
		}

		if (!cancellationToken)
		{
			// Canceled.
			return false;
		}

		// 3) Decision: pick unassigned variable -> increase decision level and try true / false.
		// Save state for backtracking.
		var snapshotBeforeDecision = _assignmentStates[..];

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		_decisionLevel++;
		_decisionLevels.Add(variable);
		_variableLevel[variable] = _decisionLevel;
		_antecedent[variable] = null; // Decision var has no antecedent.
		_trail.Add(variable); // +variable means true.
#elif ENABLE_NOGOOD_LEARNING
		_decisionSnapshots.Add(snapshotBeforeDecision);
		_decisionStack.Add((variable, true)); // Assume true on first try.
#endif

		// 4) Recurse.
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

		// Try opposite branch.
#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		// Undo assignments that happened after decision (pop assignments until variable is unassigned),
		// but keep current decision as switching.
		// We'll restore by popping trail to the point just before the decision variable's assignment.

		// Undo any implications at current decision level except decision itself.
		// Switch recorded decision variable to false.
		BacktrackToLevel(_decisionLevel - 1);
		_assignmentStates[variable] = false;
		_variableLevel[variable] = _decisionLevel;
		_antecedent[variable] = null;
		_trail.Add(-variable); // assign false on trail
#elif ENABLE_NOGOOD_LEARNING
		// Backtrack and try 'variable' = false.
		_assignmentStates = snapshotBeforeDecision;
		_decisionStack[^1] = (variable, false); // Switch the recorded decision value to false.
		_assignmentStates[variable] = false;
#else
		// Backtrack and try 'variable' = false.
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

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
		// Both branches failed -> backtrack decision.
		BacktrackToLevel(_decisionLevel - 1);
		_decisionLevels.RemoveAt(^1);
		_decisionLevel--;
#elif ENABLE_NOGOOD_LEARNING
		// Both assignments led to conflict => unsatisfiable under current partial assignment.
		_assignmentStates = _decisionSnapshots[^1][..];
		_decisionStack.RemoveAt(^1);
		_decisionSnapshots.RemoveAt(^1);
#else
		_assignmentStates = snapshotBeforeDecision;
#endif
		return false;
	}

#if ENABLE_CONFLICT_DRIVEN_CLAUSE_LEARNING
	/// <summary>
	/// Unit propagation (CDCL-aware).
	/// </summary>
	/// <returns>
	/// Returns <see langword="null"/> if no conflict;
	/// otherwise returns the conflicting clause (the clause causing conflict).
	/// </returns>
	private ReadOnlyMemory<int>? UnitPropagation()
	{
		bool changed;
		do
		{
			changed = false;
			foreach (var clause in _expression.Clauses)
			{
				var span = clause.Span;
				var unassignedCount = 0;
				var lastUnassignedLiteral = 0;
				var clauseSatisfied = false;
				for (var i = 0; i < span.Length; i++)
				{
					var literal = span[i];
					var v = Math.Abs(literal);
					var sign = literal > 0;
					if (_assignmentStates[v] == sign)
					{
						clauseSatisfied = true;
						break;
					}
					if (_assignmentStates[v] is null)
					{
						unassignedCount++;
						lastUnassignedLiteral = literal;
					}
				}
				if (clauseSatisfied)
				{
					continue;
				}

				if (unassignedCount == 0)
				{
					// Conflict: all literals false under current assignment.
					return clause;
				}

				if (unassignedCount == 1)
				{
					// Unit clause: assign the <c>lastUnassignedLiteral</c>.
					var v = Math.Abs(lastUnassignedLiteral);
					var value = lastUnassignedLiteral > 0;
					_assignmentStates[v] = value;
					_variableLevel[v] = _decisionLevel;
					_antecedent[v] = clause;
					_trail.Add(lastUnassignedLiteral);
					changed = true;
				}
			}
		} while (changed);
		return null;
	}

	/// <summary>
	/// Conflict analysis -> First-UIP.
	/// </summary>
	/// <param name="conflictClause">Conflicting clause (array of literals).</param>
	/// <returns>Learned clause (<see cref="int"/>[]), or <see langword="null"/> / <c>[]</c> for degenerate.</returns>
	private int[]? ConflictAnalyze(int[] conflictClause)
	{
		// Start from conflict clause.
		var clause = conflictClause.ToList(); // Mutable worklist.
		while (true)
		{
			// Count literals at current decision level.
			if (countAtCurrentLevel() <= 1)
			{
				// Reached First-UIP condition.
				break;
			}

			// Find the latest assigned literal in the trail that appears in clause and is at current level.
			var pivotLiteral = 0;
			for (var i = _trail.Count - 1; i >= 0; i--)
			{
				var literal = _trail[i];
				var v = Math.Abs(literal);
				if (_variableLevel[v] == _decisionLevel && clause.Contains(literal))
				{
					pivotLiteral = literal;
					break;
				}
			}
			if (pivotLiteral == 0)
			{
				// Cannot find pivot (should not happen) -> abort.
				break;
			}

			var pivotVariable = Math.Abs(pivotLiteral);
			if (_antecedent[pivotVariable] is not { } ante)
			{
				// Pivot is a decision variable; resolving with null antecedent is equivalent to removing the pivot literal.
				clause.RemoveAll(x => Math.Abs(x) == pivotVariable);
				continue;
			}

			// Resolve clause with antecedent on <c>pivotVariable</c>.
			clause = [.. ResolveOnVariable(clause, ante.ToArray(), pivotVariable)];

			// If resolution produced tautology or empty -> abort (rare).
			if (clause.Count == 0)
			{
				return null;
			}
		}

		// Clause now is the learned clause (First-UIP) simplify: remove duplicates and normalize.
		var result = new HashSet<int>(clause);

		// Remove pairs producing tautology.
		foreach (var literal in clause)
		{
			if (result.Contains(-literal))
			{
				// Tautology -> ignore learned clause.
				return null;
			}
		}
		return [.. result];


		int countAtCurrentLevel()
		{
			var count = 0;
			foreach (var literal in clause)
			{
				if (_variableLevel[Math.Abs(literal)] == _decisionLevel)
				{
					count++;
				}
			}
			return count;
		}
	}

	/// <summary>
	/// Resolve two clauses on variable <paramref name="variableToResolve"/>.
	/// </summary>
	/// <param name="c1">Clause.</param>
	/// <param name="c2">Antecedent.</param>
	/// <param name="variableToResolve">The variable to be resolved.</param>
	/// <returns>The result.</returns>
	private static int[] ResolveOnVariable(List<int> c1, int[] c2, int variableToResolve)
	{
		var result = new HashSet<int>();
		foreach (var literal in c1)
		{
			if (Math.Abs(literal) != variableToResolve)
			{
				result.Add(literal);
			}
		}
		foreach (var literal in c2)
		{
			if (Math.Abs(literal) != variableToResolve)
			{
				result.Add(literal);
			}
		}

		// Check tautology.
		foreach (var literal in result)
		{
			if (result.Contains(-literal))
			{
				return [];
			}
		}
		return [.. result];
	}

	/// <summary>
	/// Backtrack (undo assignments) down to level <paramref name="level"/> (inclusive).
	/// That is, after <c>BacktrackToLevel(L)</c> all variables with level &gt; <c>L</c> become unassigned.
	/// </summary>
	/// <param name="level">The level.</param>
	private void BacktrackToLevel(int level)
	{
		for (var i = _trail.Count - 1; i >= 0; i--)
		{
			var literal = _trail[i];
			var v = Math.Abs(literal);
			if (_variableLevel[v] > level)
			{
				// Undo.
				_assignmentStates[v] = null;
				_variableLevel[v] = 0;
				_antecedent[v] = null;
				_trail.RemoveAt(i);
			}
		}

		// Also shrink decision level bookkeeping if necessary.
		while (_decisionLevel > level)
		{
			_decisionLevels.RemoveAt(^1);
			_decisionLevel--;
		}
	}
#else
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
#endif


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
					if (mappedVariables.TryGetValue((row * 9 + column) * 9 + digit, out var variable)
						&& assignmentStates[variable] is true)
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
