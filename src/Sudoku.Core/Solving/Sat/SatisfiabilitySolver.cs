namespace Sudoku.Solving.Sat;

/// <summary>
/// <para>
/// Represents a solver that solves a puzzle using a SAT solver pipeline.
/// The solver reduces the problem to a CNF formula (<see cref="CnfFormula"/>) and then
/// searches for a satisfying assignment using a DPLL-based engine enhanced with
/// CDCL, First-UIP clause learning, two-watched-literals propagation and VSIDS heuristics.
/// </para>
/// <para>
/// If we can reduce a problem into SAT ones, we can use SAT solving system to solve the problem.
/// For example, to solve a sudoku puzzle, we construct boolean CNF clauses encoding the rules
/// and provided clues, then search for a variable assignment that satisfies every clause.
/// </para>
/// </summary>
/// <remarks>
/// Helpful references for the implemented techniques:
/// <list type="number">
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
/// <see href="https://www.princeton.edu/~chaff/publication/iccad2001_final.pdf">First-UIP Cut (paper)</see>
/// </item>
/// <item>
/// <see href="https://en.wikipedia.org/wiki/Conflict-driven_clause_learning">Wikipedia - CDCL</see>
/// </item>
/// <item>
/// <see href="https://en.wikipedia.org/wiki/Boolean_satisfiability_algorithm_heuristics#Variable_State_Independent_Decaying_Sum">Wikipedia - VSIDS</see>
/// </item>
/// </list>
/// </remarks>
/// <!--
/// Some complex examples that I have used:
/// <code>
/// ....62........98........37........527.......984........69........27........48....
/// </code>
/// -->
/// <seealso cref="CnfFormula"/>
public sealed class SatisfiabilitySolver : ISolver, ISolutionEnumerableSolver<SatisfiabilitySolver>
{
	/// <summary>
	/// The root CNF formula to be solved.
	/// </summary>
	private CnfFormula? _formula;


	/// <inheritdoc/>
	string ISolver.UriLink => "https://en.wikipedia.org/wiki/Boolean_satisfiability_problem";


	/// <inheritdoc/>
	public event EventHandler<SatisfiabilitySolver, SolverSolutionFoundEventArgs>? SolutionFound;


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		Encode(grid, out var mappedVariables);

		(result, var @return) = new Dpll(_formula, null, mappedVariables, this).Solve() switch
		{
			[var assignmentStates] => (Dpll.BuildSolution(assignmentStates, mappedVariables), true),
			[var firstAssignmentStates, ..] => (Dpll.BuildSolution(firstAssignmentStates, mappedVariables), false),
			_ => (Grid.Undefined, (bool?)null)
		};
		return @return;
	}

	/// <inheritdoc/>
	void ISolutionEnumerableSolver<SatisfiabilitySolver>.EnumerateSolutionsCore(in Grid grid, CancellationToken cancellationToken)
	{
		Encode(grid, out var mappedVariables);
		new Dpll(_formula, SolutionFound, mappedVariables, this).Solve(cancellationToken);
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
	[MemberNotNull(nameof(_formula))]
	private void Encode(in Grid grid, out Dictionary<Candidate, int> mappedVariables)
	{
		// 1) Collect mapping indices in order to compress variables.
		mappedVariables = [];
		var virtualId = 1;
		for (var cell = 0; cell < 81; cell++)
		{
			foreach (var digit in grid.GetCandidates(cell))
			{
				mappedVariables.Add(cell * 9 + digit, virtualId++);
			}
		}

		_formula = new(virtualId - 1);

		// 2a) Cell constraints (exactly one digit per cell).
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
				var candidates = grid.GetCandidates(r * 9 + c);

				// At least one digit in cell: <c>(r, c)</c> must be one of 1..9.
				var atLeast = new List<int>();
				foreach (var d in candidates)
				{
					atLeast.Add(mappedVariables[(r * 9 + c) * 9 + d]);
				}
				_formula.AddClause(atLeast.AsMemory());

				// At most one digit: for every pair <c>(d1, d2)</c>, they cannot both be true.
				foreach (var pair in candidates.AllSets & 2)
				{
					_formula.AddClause(
						(int[])[
							-mappedVariables[(r * 9 + c) * 9 + pair[0]],
							-mappedVariables[(r * 9 + c) * 9 + pair[1]]
						]
					);
				}
			}
		}

		// 2b) Row constraints (each digit once per row).
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
				_formula.AddClause(atLeast.AsMemory());

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_formula.AddClause((int[])[-pair[0], -pair[1]]);
				}
			}
		}

		// 2c) Column constraints (each digit once per column).
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
				_formula.AddClause(atLeast.AsMemory());

				foreach (var pair in atLeast.AsSpan() & 2)
				{
					_formula.AddClause((int[])[-pair[0], -pair[1]]);
				}
			}
		}

		// 2d) Block constraints (each block).
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
					_formula.AddClause(atLeast.AsMemory());

					foreach (var pair in atLeast.AsSpan() & 2)
					{
						_formula.AddClause((int[])[-pair[0], -pair[1]]);
					}
				}
			}
		}

		// 3) Initial clues as unit clauses.
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
				if (grid.GetDigit(r * 9 + c) is var d and not -1)
				{
					// Force <c>(r, c) = d</c> by adding single literal clause.
					_formula.AddClause(Array.Single(mappedVariables[(r * 9 + c) * 9 + d]));
				}
			}
		}
	}
}

/// <summary>
/// Implements DPLL algorithm (Davis–Putnam–Logemann–Loveland) extended with CDCL (clause learning),
/// two-watched-literals propagation and VSIDS heuristic for variable selection.
/// </summary>
file sealed class Dpll
{
	/// <summary>
	/// Decay factor (typical .95).
	/// On each conflict we multiply <see cref="_variableIncrement"/> by <c>1 / <see cref="VariableDecay"/></c>.
	/// This implements MiniSAT-style increment/decay behavior for VSIDS.
	/// </summary>
	private const double VariableDecay = .95;

	/// <summary>
	/// Threshold to trigger activity rescale to avoid overflow.
	/// </summary>
	private const double ActivityRescaleThreshold = 1E100;


	/// <summary>
	/// After solving, retrieve the assignment array
	/// (index: <c>variable</c> is either <see langword="true"/> or <see langword="false"/>).
	/// </summary>
	/// <remarks>
	/// Values:
	/// <list type="table">
	/// <listheader>
	/// <term>Value</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><see langword="true"/></term>
	/// <description>Literal assigned <see langword="true"/></description>
	/// </item>
	/// <item>
	/// <term><see langword="false"/></term>
	/// <description>Literal assigned <see langword="false"/></description>
	/// </item>
	/// <item>
	/// <term><see langword="null"/></term>
	/// <description>Unassigned</description>
	/// </item>
	/// </list>
	/// The array is 1-based: index 0 is unused.
	/// </remarks>
	private readonly bool?[] _assignmentStates;

	/// <summary>
	/// Event handler used to report found solutions when enumerating all solutions.
	/// </summary>
	private readonly EventHandler<SatisfiabilitySolver, SolverSolutionFoundEventArgs>? _solutionFoundEventHandler;

	/// <summary>
	/// The CNF formula being solved.
	/// </summary>
	private readonly CnfFormula _formula;

	/// <summary>
	/// Mapping from puzzle candidates to SAT variable indices (used to construct final grid solutions).
	/// </summary>
	private readonly Dictionary<Candidate, int>? _mappedVariables;

	/// <summary>
	/// The parent solver that created this instance (used for callbacks).
	/// </summary>
	private readonly SatisfiabilitySolver? _parentSolver;

	/// <summary>
	/// Decision level per variable (0..). Used by conflict analysis and backjumping.
	/// </summary>
	private readonly int[] _variableLevel;

	/// <summary>
	/// Signed literals in assignment order (the trail).
	/// <list type="table">
	/// <listheader>
	/// <term>Sign</term>
	/// <description>Meaning</description>
	/// </listheader>
	/// <item>
	/// <term><c>+v</c></term>
	/// <description><c>v = <see langword="true"/></c></description>
	/// </item>
	/// <item>
	/// <term><c>-v</c></term>
	/// <description><c>v = <see langword="false"/></c></description>
	/// </item>
	/// </list>
	/// </summary>
	private readonly List<int> _trail;

	/// <summary>
	/// Clause that implied this variable (null for decision variables). Stored as the clause read-only memory reference.
	/// </summary>
	private readonly ReadOnlyMemory<int>?[] _antecedent;

	/// <summary>
	/// Stack: variable chosen at each decision level (for bookkeeping).
	/// </summary>
	private readonly List<int> _decisionLevels;

	/// <summary>
	/// Current decision level.
	/// </summary>
	private int _decisionLevel;

	/// <summary>
	/// Process <c>_trail</c> from this index forward during unit propagation.
	/// </summary>
	private int _propagationIndex;

	/// <summary>
	/// Indexed by mapped literal index -> list of clause indices that watch this literal.
	/// The array size is (2 * N + 1), mapping positive and negative signed literals to separate buckets.
	/// </summary>
	private List<int>[]? _watches;

	/// <summary>
	/// Watched literal A per clause (signed literal).
	/// </summary>
	private List<int> _watchLiteralA = [];

	/// <summary>
	/// Watched literal B per clause (signed literal); 0 if absent (unary clause).
	/// </summary>
	private List<int> _watchLiteralB = [];

	/// <summary>
	/// VSIDS activity score per variable (1-based).
	/// </summary>
	private readonly double[] _activity;

	/// <summary>
	/// Current variable increment used when bumping variables (MiniSAT-style).
	/// </summary>
	private double _variableIncrement = 1.0;


	/// <summary>
	/// Initializes a <see cref="Dpll"/> instance with required structures.
	/// </summary>
	/// <param name="formula"><inheritdoc cref="_formula" path="/summary"/></param>
	/// <param name="solutionFoundEventHandler"><inheritdoc cref="_solutionFoundEventHandler" path="/summary"/></param>
	/// <param name="mappedVariables"><inheritdoc cref="_mappedVariables" path="/summary"/></param>
	/// <param name="parentSolver"><inheritdoc cref="_parentSolver" path="/summary"/></param>
	public Dpll(
		CnfFormula formula,
		EventHandler<SatisfiabilitySolver, SolverSolutionFoundEventArgs>? solutionFoundEventHandler,
		Dictionary<Candidate, int>? mappedVariables,
		SatisfiabilitySolver? parentSolver
	)
	{
		// Initialize base fields.
		_formula = formula;
		_assignmentStates = new bool?[_formula.VariablesCount + 1];
		_solutionFoundEventHandler = solutionFoundEventHandler;
		_mappedVariables = mappedVariables;
		_parentSolver = parentSolver;

		// First-UIP:
		// initialize structures used by conflict analysis
		// (trail, decision level bookkeeping, variable-to-level map and antecedents).
		// These are required for First-UIP resolution.
		_trail = [];
		_decisionLevels = [];
		_variableLevel = new int[_formula.VariablesCount + 1];
		_antecedent = new ReadOnlyMemory<int>?[_formula.VariablesCount + 1];

		// Two-watched-literals: Build initial watches and enqueue unit clauses.
		EnsureWatchesInit();
		for (var i = 0; i < _formula.ClauseCount; i++)
		{
			RegisterClauseWatches(i);
		}
		// Enqueue initial unit clauses (level 0 assignments inferred from unit clauses).
		for (var i = 0; i < _formula.ClauseCount; i++)
		{
			if (_formula.Clauses[i].Span is [var a] && Math.Abs(a) is var v && _assignmentStates[v] is null)
			{
				_assignmentStates[v] = a > 0;
				_variableLevel[v] = 0;
				_antecedent[v] = _formula.Clauses[i];
				_trail.Add(a);
			}
		}

		// First-UIP: <c>_propagationIndex</c> will start processing from 0 when <c>UnitPropagation</c> runs.
		_propagationIndex = 0;

		// VSIDS: initialize activity scores.
		_activity = new double[_formula.VariablesCount + 1];

		// Simple static heuristic: count literal occurrences as initial activity (seed for VSIDS).
		for (var clauseIndex = 0; clauseIndex < _formula.ClauseCount; clauseIndex++)
		{
			foreach (var literal in _formula.Clauses[clauseIndex].Span)
			{
				_activity[Math.Abs(literal)] += 1.0;
			}
		}
	}


	/// <summary>
	/// Try to find a satisfying assignment. Entry point for the CDCL search.
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
	/// Performs DPLL recursive method with CDCL loop. Algorithm flow (high level):
	/// <list type="number">
	/// <item>Perform unit propagation to simplify (two-watched-literals based).</item>
	/// <item>If conflict -> analyze (First-UIP), learn clause, bump VSIDS, backjump and continue.</item>
	/// <item>If all variables assigned -> report solution.</item>
	/// <item>Otherwise pick a variable (VSIDS) and branch on true/false (decision), recursing.</item>
	/// </list>
	/// </summary>
	private bool Backtracking(List<bool?[]> solutions, CancellationToken cancellationToken)
	{
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
				// Unsatisfiable at root level: the formula is UNSAT.
				return false;
			}

			// VSIDS: bump variables appeared in conflicting clause (signal to heuristic they matter).
			BumpActivityForClause(conflictClause);

			// First-UIP: perform conflict analysis to produce a learned clause using First-UIP resolution.
			if (ConflictAnalyze(conflictClause.ToArray()) is not { Length: not 0 } learned)
			{
				// Degenerate case (tautology/empty) -> treat as UNSAT.
				return false;
			}

			// VSIDS: bump variables in the learned clause, then decay var increment (MiniSAT-style).
			BumpActivityForClause(learned);
			DecayActivities();

			// CDCL: add learned clause to the formula and register watches for its propagation.
			_formula.AddClause(learned.AsMemory());
			RegisterClauseWatches(_formula.ClauseCount - 1);

			// Compute backjump level: maximum level among literals in learned clause except current level.
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

			// After backjump, the learned clause should be unit (the UIP literal) and must be propagated.
			// Find the literal in learned that is unassigned now (expected to be the UIP).
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

			// Defensive: normally <c>unassignedCount == 1</c> after proper First-UIP learning.
			if (unassignedCount == 1)
			{
				var v = Math.Abs(unitLiteral);
				var value = unitLiteral > 0;
				_assignmentStates[v] = value;
				_variableLevel[v] = backjumpLevel;
				_antecedent[v] = learned.AsMemory();
				_trail.Add(unitLiteral);
			}

			// Continue propagation loop (<c>UnitPropagation</c> will be called again).
		}

		// 2) Check if all variables assigned -> solution.
		var variable = PickBranchingVariable();
		if (variable == -1)
		{
			// All variables assigned without conflict => SAT.
			Debug.Assert(_parentSolver is not null);
			Debug.Assert(_mappedVariables is not null);

			solutions.Add(_assignmentStates[..]);
			_solutionFoundEventHandler?.Invoke(_parentSolver, new(BuildSolution(_assignmentStates, _mappedVariables)));
			return _solutionFoundEventHandler is null;
		}

		if (cancellationToken.IsCancellationRequested)
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

		// 4a) Recurse.
		// Try assigning 'variable' = true.
		_assignmentStates[variable] = true;
		if (Backtracking(solutions, cancellationToken))
		{
			return true;
		}

		if (cancellationToken.IsCancellationRequested)
		{
			// Canceled.
			return false;
		}

		// 4b) Try opposite branch.
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

		if (cancellationToken.IsCancellationRequested)
		{
			// Canceled.
			return false;
		}

		// 5) Both branches failed -> backtrack decision.
		BacktrackToLevel(_decisionLevel - 1);
		_decisionLevels.RemoveAt(^1);
		_decisionLevel--;
		return false;
	}

	/// <summary>
	/// Unit propagation (CDCL-aware), implemented using the two-watched-literals scheme.
	/// </summary>
	/// <returns>
	/// Returns <see langword="null"/> if no conflict; otherwise returns the conflicting clause (the clause causing conflict).
	/// </returns>
	private ReadOnlyMemory<int>? UnitPropagation()
	{
		// Two-watched-literals: process trail incrementally using watched lists.
		EnsureWatchesInit();

		Debug.Assert(_watches is not null);

		while (_propagationIndex < _trail.Count)
		{
			var literalJustAssigned = _trail[_propagationIndex++];

			// The literal that becomes false is the negation of assigned literal:
			var falseLiteral = -literalJustAssigned;
			var watchIndex = LiteralToIndex(falseLiteral);
			var watchList = _watches[watchIndex];

			// Iterate with index so we can modify the list in-place (swap-remove) when we move watches.
			for (var i = 0; i < watchList.Count;)
			{
				var clauseIndex = watchList[i];
				var clause = _formula.Clauses[clauseIndex].Span;

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

				// Try to find alternative literal (unassigned or true) in clause to replace <c>falseLiteral</c> watch.
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

					// Accept if literal is unassigned or true: then we can watch it instead of the false one.
					if (_assignmentStates[vv] is null || _assignmentStates[vv] == literal > 0)
					{
						foundAlternative = literal;
						break;
					}
				}

				if (foundAlternative != 0)
				{
					// Replace watch: move clause from the current watch list to the alternative watch's bucket.
					var last = watchList[^1];
					watchList[i] = last;
					watchList.RemoveAt(^1);

					// Set watch slot to <c>foundAlternative</c>.
					(wa == falseLiteral ? _watchLiteralA : _watchLiteralB)[clauseIndex] = foundAlternative;

					// Add to watches for <c>foundAlternative</c>.
					_watches[LiteralToIndex(foundAlternative)].Add(clauseIndex);

					// Don't increment <c>i</c> because we replaced current entry with last; need to re-examine it.
					continue;
				}

				// No alternative watch found: clause is now watched only by <c>otherWatched</c> (or unary).
				var otherVar = Math.Abs(otherWatched);
				if (otherWatched != 0 && _assignmentStates[otherVar] is null)
				{
					// Unit clause -> assign <c>otherWatched</c> to satisfy clause.
					var value = otherWatched > 0;
					_assignmentStates[otherVar] = value;
					_variableLevel[otherVar] = _decisionLevel;
					_antecedent[otherVar] = _formula.Clauses[clauseIndex];
					_trail.Add(otherWatched);

					// Move to next watch in same list (current entry still refers to same clauseIndex because we didn't remove it).
					i++;
					continue;
				}

				// If <c>otherWatched</c> is assigned false (or <c>otherWatched == 0</c> meaning clause length 0) -> conflict.
				// Return the conflicting clause for analysis.
				return _formula.Clauses[clauseIndex];
			}
		}

		// No conflict.
		return null;
	}

	/// <summary>
	/// Conflict analysis implementing the First-UIP scheme.
	/// The routine performs conflict-driven resolution until the learned clause contains at most one literal
	/// from the current decision level (the First Unique Implication Point), then returns the learned clause.
	/// </summary>
	/// <param name="conflictClause">Conflicting clause (array of literals).</param>
	/// <returns>Learned clause (<see cref="int"/>[]), or <see langword="null"/> / <c>[]</c> for degenerate cases.</returns>
	private int[]? ConflictAnalyze(int[] conflictClause)
	{
		// First-UIP: start from the conflict clause and resolve until the First-UIP condition holds.
		var clause = conflictClause.ToList(); // Mutable worklist.
		while (true)
		{
			// Count literals at current decision level; First-UIP is reached when <= 1 such literals remain.
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
				// Cannot find pivot (should not happen in normal runs) -> abort.
				break;
			}

			var pivotVariable = Math.Abs(pivotLiteral);
			if (_antecedent[pivotVariable] is not { } ante)
			{
				// First-UIP: pivot is a decision variable; resolving with null antecedent
				// simply removes the pivot literal from the clause (it cannot be resolved further).
				clause.RemoveAll(x => Math.Abs(x) == pivotVariable);
				continue;
			}

			// Resolve clause with antecedent on <c>pivotVariable</c>.
			clause = [.. ResolveOnVariable(clause.AsSpan(), ante.Span, pivotVariable)];

			// If resolution produced tautology or empty -> abort (rare/degenerate case).
			if (clause.Count == 0)
			{
				return null;
			}
		}

		// Clause is now the learned clause (First-UIP). Simplify: remove duplicates and normalize.
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
	/// Standard resolution: union of both clauses without literals of the resolved variable.
	/// </summary>
	/// <param name="c1">Clause.</param>
	/// <param name="c2">Antecedent.</param>
	/// <param name="variableToResolve">The variable to be resolved.</param>
	/// <returns>The result.</returns>
	private static int[] ResolveOnVariable(ReadOnlySpan<int> c1, ReadOnlySpan<int> c2, int variableToResolve)
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
	/// After <c>BacktrackToLevel(L)</c> all variables with level &gt; L become unassigned.
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
				// Undo: mark variable unassigned and clear antecedent/level information.
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
	/// Map signed literal to watches array index.
	/// Positive <c>v</c> -> <c>index = v</c> (1..N); Negative <c>-v</c> -> <c>index = N + v</c> (N+1 .. 2N).
	/// We keep index 0 unused.
	/// </summary>
	/// <param name="literal">The literal.</param>
	/// <returns>Watches array index.</returns>
	private int LiteralToIndex(int literal)
	{
		var n = _formula.VariablesCount;
		return literal > 0 ? literal : n + -literal;
	}

	/// <summary>
	/// Ensure watches array has correct size for current variable count.
	/// Reinitializes watch buckets and watch literal lists when variable count changes.
	/// </summary>
	private void EnsureWatchesInit()
	{
		var n = _formula.VariablesCount;
		var size = (n << 1) + 1;
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
	/// This sets the two watched literals (or single watch for unit clauses) and places the clause index into the
	/// appropriate watch buckets so propagation can efficiently find affected clauses when literals change.
	/// </summary>
	/// <param name="clauseIndex">The clause.</param>
	private void RegisterClauseWatches(int clauseIndex)
	{
		EnsureWatchesInit();

		Debug.Assert(_watches is not null);

		switch (_formula.Clauses[clauseIndex].Span)
		{
			case []:
			{
				// Empty clause (immediate conflict) - register placeholders but no watched literals.
				_watchLiteralA.Add(0);
				_watchLiteralB.Add(0);
				break;
			}
			case [var a]:
			{
				// Unit clause: watch the single literal in slot A, slot B = 0.
				_watchLiteralA.Add(a);
				_watchLiteralB.Add(0);
				_watches[LiteralToIndex(a)].Add(clauseIndex);
				break;
			}
			case [var literalA, var literalB, ..]:
			{
				// Length >= 2: watch the first two literals initially.
				_watchLiteralA.Add(literalA);
				_watchLiteralB.Add(literalB);
				_watches[LiteralToIndex(literalA)].Add(clauseIndex);
				_watches[LiteralToIndex(literalB)].Add(clauseIndex);
				break;
			}
		}
	}

	/// <summary>
	/// Bump activity for variables appearing in a clause (used by VSIDS heuristic).
	/// Each variable's activity is increased by the current variable increment; if the activity grows
	/// too large a rescale is performed to avoid numerical overflow.
	/// </summary>
	/// <param name="clause">The clause.</param>
	private void BumpActivityForClause(ReadOnlyMemory<int> clause)
	{
		foreach (var literal in clause)
		{
			var v = Math.Abs(literal);
			_activity[v] += _variableIncrement;

			// If any activity gets too large, rescale everything to keep values numerically stable.
			if (_activity[v] > ActivityRescaleThreshold)
			{
				RescaleActivities();

				// After rescale, no need to continue rescaling checks here.
			}
		}
	}

	/// <summary>
	/// Update <see cref="_variableIncrement"/> on conflict (decay / increase the increment).
	/// MiniSAT style: increase variable increment so future bumps have larger effect,
	/// which is effectively a decay of past activity influence.
	/// </summary>
	private void DecayActivities() => _variableIncrement *= 1D / VariableDecay;

	/// <summary>
	/// Rescale activities to avoid overflow: divide all activities and <see cref="_variableIncrement"/> by a large factor.
	/// </summary>
	private void RescaleActivities()
	{
		// Find max to scale down relative magnitudes.
		var max = .0;
		for (var i = 1; i < _activity.Length; i++)
		{
			if (_activity[i] > max)
			{
				max = _activity[i];
			}
		}
		if (max <= .0)
		{
			return;
		}

		var scale = 1D / max;
		for (var i = 1; i < _activity.Length; i++)
		{
			_activity[i] *= scale;
		}
		_variableIncrement *= scale;
	}

	/// <summary>
	/// Pick branching variable using VSIDS: highest activity among unassigned variables.
	/// </summary>
	private int PickBranchingVariable()
	{
		var best = -1;
		var bestScore = double.NegativeInfinity;
		for (var i = 1; i <= _formula.VariablesCount; i++)
		{
			if (_assignmentStates[i] is null)
			{
				var score = _activity[i];

				// Tie-breaker: smaller index preferred naturally by >.
				if (score > bestScore)
				{
					bestScore = score;
					best = i;
				}
			}
		}

		// Fallback to first unassigned if something odd happened.
		if (best == -1)
		{
			for (var i = 1; i <= _formula.VariablesCount; i++)
			{
				if (_assignmentStates[i] is null)
				{
					return i;
				}
			}
		}
		return best;
	}


	/// <summary>
	/// Build solution via the specified states and mapped variables.
	/// </summary>
	/// <param name="assignmentStates"><inheritdoc cref="_assignmentStates" path="/summary"/></param>
	/// <param name="mappedVariables"><inheritdoc cref="_mappedVariables" path="/summary"/></param>
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
