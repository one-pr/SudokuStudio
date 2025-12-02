namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// <para>
/// Represents a solver that solves a puzzle using SAT algorithm.
/// SAT problem is a type of problems that can be solved via CNFs (corresponding to type <see cref="CnfExpression"/>),
/// i.e. boolean expressions (or say, formulae).
/// </para>
/// <para>
/// If we can reduce a problem into SAT ones, we can use SAT solving system to solve the problem.
/// For example, to solve a sudoku puzzle, we should construct some boolean formulae (CNFs),
/// and then find variables in order to make such formulae satisfied.
/// </para>
/// </summary>
/// <remarks>
/// There're some pages that are really helpful:
/// <list type="bullet">
/// <item>
/// <see href="https://en.wikipedia.org/wiki/Boolean_satisfiability_problem">Wikipedia - SAT Problem</see>
/// </item>
/// <item>
/// <see href="https://en.wikipedia.org/wiki/Conjunctive_normal_form">Wikipedia - CNF (Conjunctive Normal Form)</see>
/// </item>
/// <item>
/// <see href="https://en.wikipedia.org/wiki/DPLL_algorithm">Wikipedia - DPLL Algorithm</see>
/// </item>
/// <item>
/// <see href="https://www.princeton.edu/~chaff/publication/iccad2001_final.pdf">First-UIP Cut (Paper)</see>
/// </item>
/// <item>
/// <see href="https://en.wikipedia.org/wiki/Conflict-driven_clause_learning">Wikipedia - CDCL (Conflict-Driven Clause Learning)</see>
/// </item>
/// </list>
/// </remarks>
/// <seealso cref="CnfExpression"/>
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
/// Implements DPLL algorithm (<i>Davis–Putnam–Logemann–Loveland algorithm</i>).
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
	private readonly bool?[] _assignmentStates;

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

	/// <summary>
	/// Process <c>_trail</c> from this index forward.
	/// </summary>
	private int _propagationIndex;

	/// <summary>
	/// Indexed by mapped literal index -> list of clause indices.
	/// </summary>
	private List<int>[]? _watches;

	/// <summary>
	/// Watched literal A per clause (signed literal).
	/// </summary>
	private List<int> _watchLiteralA = [];

	/// <summary>
	/// Watched literal B per clause (signed literal); 0 if absent (unary)
	/// </summary>
	private List<int> _watchLiteralB = [];


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

		_trail = [];
		_decisionLevels = [];
		_variableLevel = new int[_expression.VariablesCount + 1];
		_antecedent = new ReadOnlyMemory<int>?[_expression.VariablesCount + 1];

		// Build initial watches and enqueue units.
		EnsureWatchesInit();
		for (var i = 0; i < _expression.ClauseCount; i++)
		{
			RegisterClauseWatches(i);
		}
		for (var i = 0; i < _expression.ClauseCount; i++)
		{
			if (_expression.Clauses[i].Span is [var a] && Math.Abs(a) is var v && _assignmentStates[v] is null)
			{
				_assignmentStates[v] = a > 0;
				_variableLevel[v] = 0;
				_antecedent[v] = _expression.Clauses[i];
				_trail.Add(a);
			}
		}

		// <c>_propagationIndex</c> will start processing from 0 when <c>UnitPropagation</c> runs.
		_propagationIndex = 0;
	}


	/// <summary>
	/// Try to find a satisfying assignment.
	/// </summary>
	/// <param name="cancellationToken">The cancellation token.</param>
	public List<bool?[]> Solve(CancellationToken cancellationToken = default)
	{
		// Init CDCL data.
		_assignmentStates.AsSpan().Clear();
		_variableLevel.AsSpan().Clear();
		_antecedent.AsSpan().Clear();
		_trail.Clear();
		_decisionLevels.Clear();
		_decisionLevel = 0;

		var solutions = new List<bool?[]>();
		Backtracking(solutions, cancellationToken);
		return solutions;
	}


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
	private bool Backtracking(List<bool?[]> solutions, CancellationToken cancellationToken)
	{
		// It seems that two variables are not null anyway.
		//Debug.Assert(_varLevel is not null && _antecedent is not null);

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
			RegisterClauseWatches(_expression.ClauseCount - 1);

			// Compute backjump level.
			var backjumpLevel = 0;
			foreach (var literal in learned)
			{
				var v = Math.Abs(literal);
				var level = _variableLevel[v];
				if (level != _decisionLevel)
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
		_decisionLevel++;
		_decisionLevels.Add(variable);
		_variableLevel[variable] = _decisionLevel;
		_antecedent[variable] = null; // Decision var has no antecedent.
		_trail.Add(variable); // +variable means true.

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
		// Undo assignments that happened after decision (pop assignments until variable is unassigned),
		// but keep current decision as switching.
		// We'll restore by popping trail to the point just before the decision variable's assignment.

		// Undo any implications at current decision level except decision itself.
		// Switch recorded decision variable to false.
		BacktrackToLevel(_decisionLevel - 1);
		_assignmentStates[variable] = false;
		_variableLevel[variable] = _decisionLevel;
		_antecedent[variable] = null;
		_trail.Add(-variable); // Assign false on trail.
		if (Backtracking(solutions, cancellationToken))
		{
			return true;
		}

		if (!cancellationToken)
		{
			// Canceled.
			return false;
		}

		// Both branches failed -> backtrack decision.
		BacktrackToLevel(_decisionLevel - 1);
		_decisionLevels.RemoveAt(^1);
		_decisionLevel--;
		return false;
	}

	/// <summary>
	/// Unit propagation (CDCL-aware), with two-watched literals checking.
	/// </summary>
	/// <returns>
	/// Returns <see langword="null"/> if no conflict;
	/// otherwise returns the conflicting clause (the clause causing conflict).
	/// </returns>
	private ReadOnlyMemory<int>? UnitPropagation()
	{
		// Process trail incrementally using watched lists.
		EnsureWatchesInit();

		Debug.Assert(_watches is not null);

		while (_propagationIndex < _trail.Count)
		{
			var literalJustAssigned = _trail[_propagationIndex++];

			// The literal that becomes false is the negation of assigned literal:
			var falseLiteral = -literalJustAssigned;
			var watchIndex = LiteralToIndex(falseLiteral);
			var watchList = _watches[watchIndex];

			// Iterate with index so we can modify the list in-place (swap-remove).
			for (var i = 0; i < watchList.Count;)
			{
				var clauseIndex = watchList[i];
				var clause = _expression.Clauses[clauseIndex].Span;

				// Determine which watched slot in this clause corresponds to <c>falseLiteral</c>.
				var wa = _watchLiteralA[clauseIndex];
				var wb = _watchLiteralB[clauseIndex];
				var otherWatched = wa == falseLiteral ? wb : wa;

				// If the other watched literal already evaluates to true, clause is satisfied — skip.
				var ov = Math.Abs(otherWatched);
				if (otherWatched != 0 && _assignmentStates[ov] == otherWatched > 0)
				{
					// Nothing to do for this clause.
					i++;
					continue;
				}

				// Try to find alternative literal (not assigned false) in clause to replace <c>falseLiteral</c> watch.
				var foundAlternative = 0;
				for (var k = 0; k < clause.Length; k++)
				{
					var literal = clause[k];
					if (literal == wa || literal == wb)
					{
						// Skip current watches.
						continue;
					}

					var vv = Math.Abs(literal);
					// Accept if literal is unassigned or true: then we can watch it.
					if (_assignmentStates[vv] is null || _assignmentStates[vv] == literal > 0)
					{
						foundAlternative = literal;
						break;
					}
				}

				if (foundAlternative != 0)
				{
					// Replace watch:
					// Remove <c>clauseIndex</c> from current watch list and add to alt watch list.
					// Swap-remove from watchList at <c>i</c>.
					var last = watchList[^1];
					watchList[i] = last;
					watchList.RemoveAt(^1);

					// Set watch slot to <c>foundAlternative</c>.
					(wa == falseLiteral ? _watchLiteralA : _watchLiteralB)[clauseIndex] = foundAlternative;

					// Add to watches for <c>foundAlternative</c>.
					_watches[LiteralToIndex(foundAlternative)].Add(clauseIndex);

					// Don't increment i because we replaced current entry with last; need to re-examine it.
					continue;
				}

				// No alternative watch found:
				// Clause currently has only <c>otherWatched</c> (which may be unassigned or assigned false).
				var otherVar = Math.Abs(otherWatched);
				if (otherWatched != 0 && _assignmentStates[otherVar] is null)
				{
					// Unit clause -> assign otherWatched to satisfy clause.
					var value = otherWatched > 0;
					_assignmentStates[otherVar] = value;
					_variableLevel[otherVar] = _decisionLevel;
					_antecedent[otherVar] = _expression.Clauses[clauseIndex];
					_trail.Add(otherWatched);

					// Move to next watch in same list
					// (current entry still refers to same clauseIndex because we didn't remove it).
					i++;
					continue;
				}

				// If <c>otherWatched</c> is assigned false (or <c>otherWatched == 0</c> meaning clause length 0) -> conflict.
				// Return the conflicting clause.
				return _expression.Clauses[clauseIndex];
			}
		}

		// No conflict.
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

		_propagationIndex = Math.Min(_propagationIndex, _trail.Count);
	}

	/// <summary>
	/// <para>Map signed literal to watches array index.</para>
	/// <para>
	/// <list type="bullet">
	/// <item>Positive <c>v</c> -> <c>index = v</c> (1..N)</item>
	/// <item>Negative <c>-v</c> -> <c>index = N + v</c> (N+1 .. 2N)</item>
	/// </list>
	/// </para>
	/// <para>We keep index 0 unused.</para>
	/// </summary>
	/// <param name="literal">The literal.</param> 
	/// <returns>Result value.</returns>
	private int LiteralToIndex(int literal)
	{
		var n = _expression.VariablesCount;
		return literal > 0 ? literal : n + -literal;
	}

	/// <summary>
	/// Ensure watches array has correct size for current variable count.
	/// </summary>
	private void EnsureWatchesInit()
	{
		var n = _expression.VariablesCount;
		var size = 2 * n + 1;
		if (_watches is null || _watches.Length != size)
		{
			_watches = new List<int>[size];
			for (var i = 0; i < size; i++)
			{
				_watches[i] = [];
			}

			_watchLiteralA = [];
			_watchLiteralB = [];
		}
	}

	/// <summary>
	/// Register watches for a clause <paramref name="clauseIndex"/>. Called during initial build and when learning clauses.
	/// </summary>
	/// <param name="clauseIndex">The index of clause.</param>
	private void RegisterClauseWatches(int clauseIndex)
	{
		EnsureWatchesInit();

		Debug.Assert(_watches is not null);

		switch (_expression.Clauses[clauseIndex].Span)
		{
			case []:
			{
				// Empty clause (should be treated as immediate conflict) - still register but watches left as 0.
				_watchLiteralA.Add(0);
				_watchLiteralB.Add(0);
				break;
			}
			case [var a]:
			{
				_watchLiteralA.Add(a);
				_watchLiteralB.Add(0);
				_watches[LiteralToIndex(a)].Add(clauseIndex);
				break;
			}
			case [var literalA, var literalB, ..]:
			{
				// Length >= 2: watch first two literals.
				_watchLiteralA.Add(literalA);
				_watchLiteralB.Add(literalB);
				_watches[LiteralToIndex(literalA)].Add(clauseIndex);
				_watches[LiteralToIndex(literalB)].Add(clauseIndex);
				break;
			}
		}
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
