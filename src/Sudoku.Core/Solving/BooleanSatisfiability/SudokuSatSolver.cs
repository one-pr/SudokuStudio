namespace Sudoku.Solving.BooleanSatisfiability;

/// <summary>
/// Represents a solver that solves a puzzle using SAT algorithm.
/// <see href="https://en.wikipedia.org/wiki/Boolean_satisfiability_problem">SAT problem</see> is a way
/// to reduce complex puzzles to boolean expressions to be solved.
/// </summary>
/// <remarks>
/// It is strongly <b>not</b> recommend to use this solver to find multiple solutions
/// because it uses tree iteration with simple backtracking to find multiple solutions,
/// which will expand the tree to be more and more complex.
/// Therefore, this solver doesn't know whether a puzzle has a unique solution or not.
/// In practice, method <see cref="Solve(in Grid, out Grid)"/> can only
/// return <see langword="false"/> or <see langword="null"/>.
/// </remarks>
/// <seealso cref="Solve(in Grid, out Grid)"/>
public sealed class SatSolver : ISolver
{
	/// <summary>
	/// Defines a formula.
	/// </summary>
	private readonly ConjunctiveNormalFormFormula _formula = new(9 * 9 * 9);


	/// <inheritdoc/>
	string ISolver.UriLink => "https://en.wikipedia.org/wiki/Boolean_satisfiability_problem";


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		EncodeSudoku(grid);

		var solver = new BacktrackingSolver(_formula);
		var isSolved = solver.Solve();
		if (!isSolved)
		{
			result = Grid.Undefined;
			return null;
		}

		var assignmentStates = solver.GetAssignmentStates();

		// Read off which literal is true in each cell.
		result = Grid.Empty;
		for (var row = 0; row < 9; row++)
		{
			for (var column = 0; column < 9; column++)
			{
				for (var digit = 0; digit < 9; digit++)
				{
					if (assignmentStates[MapVariable(row, column, digit)] is true)
					{
						result.SetDigit(row * 9 + column, digit);
					}
				}
			}
		}

		result.Fix();
		return false;
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
	private void EncodeSudoku(in Grid grid)
	{
		// 1. Cell constraints (exactly one digit per cell).
		for (var r = 0; r < 9; r++)
		{
			for (var c = 0; c < 9; c++)
			{
				// At least one digit in cell: (r, c) must be one of 1..9.
				var atleast = new int[9];
				for (var d = 0; d < 9; d++)
				{
					atleast[d] = MapVariable(r, c, d);
				}
				_formula.AddClause(atleast);

				// At most one digit: for every pair (d1, d2), they cannot both be true.
				for (var d1 = 0; d1 < 9; d1++)
				{
					for (var d2 = d1 + 1; d2 < 9; d2++)
					{
						_formula.AddClause(-MapVariable(r, c, d1), -MapVariable(r, c, d2));
					}
				}
			}
		}

		// 2. Row constraints (each digit once per row).
		for (var r = 0; r < 9; r++)
		{
			for (var d = 0; d < 9; d++)
			{
				var atleast = new int[9];
				for (var c = 0; c < 9; c++)
				{
					atleast[c] = MapVariable(r, c, d);
				}
				_formula.AddClause(atleast);

				for (var c1 = 0; c1 < 9; c1++)
				{
					for (var c2 = c1 + 1; c2 < 9; c2++)
					{
						_formula.AddClause(-MapVariable(r, c1, d), -MapVariable(r, c2, d));
					}
				}
			}
		}

		// 3. Column constraints (each digit once per column).
		for (var c = 0; c < 9; c++)
		{
			for (var d = 0; d < 9; d++)
			{
				var atleast = new int[9];
				for (var r = 0; r < 9; r++)
				{
					atleast[r] = MapVariable(r, c, d);
				}
				_formula.AddClause(atleast);

				for (var r1 = 0; r1 < 9; r1++)
				{
					for (var r2 = r1 + 1; d < 9 && r2 < 9; r2++)
					{
						_formula.AddClause(-MapVariable(r1, c, d), -MapVariable(r2, c, d));
					}
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
					var atleastList = new List<int>();

					// Collect all vars in this block with digit 'd'.
					for (var r = 0; r < 3; r++)
					{
						for (var c = 0; c < 3; c++)
						{
							atleastList.Add(MapVariable(br * 3 + r, bc * 3 + c, d));
						}
					}

					_formula.AddClause([.. atleastList]);

					// At most one in the block.
					for (var i = 0; i < 9; i++)
					{
						for (var j = i + 1; j < 9; j++)
						{
							_formula.AddClause(-atleastList[i], -atleastList[j]);
						}
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
					_formula.AddClause(MapVariable(r, c, d));
				}
			}
		}
	}

	/// <summary>
	/// Maps 3D coordinates (row, col, digit) to a unique SAT variable index.
	/// Indices start from 1 up to 729.
	/// </summary>
	private int MapVariable(int r, int c, int d) => r * 81 + c * 9 + d + 1;
}

/// <summary>
/// Implements a simple SAT solver using the DPLL algorithm with unit propagation.
/// For more information about DPLL algorithm, please visit
/// <see href="https://en.wikipedia.org/wiki/DPLL_algorithm">this link</see>.
/// </summary>
/// <param name="formula"><inheritdoc cref="_formula" path="/summary"/></param>
file sealed class BacktrackingSolver(ConjunctiveNormalFormFormula formula)
{
	/// <summary>
	/// Indicates the formula.
	/// </summary>
	private readonly ConjunctiveNormalFormFormula _formula = formula;

	/// <summary>
	/// Represents the assignment values. The result value only represents for 3 values:
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
	/// </summary>
	private bool?[] _assignment = new bool?[formula.NumVars + 1];


	/// <summary>
	/// Try to find a satisfying assignment.
	/// </summary>
	public bool Solve() => Backtracking();

	/// <summary>
	/// Performs DPLL recursive method. DPLL recursive routine:
	/// <list type="number">
	/// <item>Perform unit propagation to simplify.</item>
	/// <item>If conflict, backtrack (return false).</item>
	/// <item>If all vars assigned, formula is satisfied.</item>
	/// <item>Otherwise, pick an unassigned variable and branch on true/false.</item>
	/// </list>
	/// </summary>
	private bool Backtracking()
	{
		if (!UnitPropagation())
		{
			return false; // Conflict detected.
		}

		var variable = GetUnassignedVar();
		if (variable == -1)
		{
			return true; // All variables assigned without conflict => SAT.
		}

		// Save state for backtracking.
		var snapshot = (bool?[])_assignment.Clone();

		// Try assigning 'variable' = true.
		_assignment[variable] = true;
		if (Backtracking())
		{
			return true;
		}

		// Backtrack and try 'variable' = false.
		_assignment = snapshot;
		_assignment[variable] = false;
		if (Backtracking())
		{
			return true;
		}

		// Both assignments led to conflict => unsatisfiable under current partial assignment.
		_assignment = snapshot;
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
			foreach (var clause in _formula)
			{
				var (unassignedCount, unassignedLit, clauseSatisfied) = (0, 0, false);

				// Check each literal in the clause.
				foreach (var literal in clause)
				{
					var variable = Math.Abs(literal);
					var sign = literal > 0;
					if (_assignment[variable] == sign)
					{
						clauseSatisfied = true; // Clause is already satisfied.
						break;
					}
					if (_assignment[variable] is null)
					{
						unassignedCount++;
						unassignedLit = literal;
					}
				}

				if (clauseSatisfied) continue;

				// If no unassigned lits left and none true => conflict.
				if (unassignedCount == 0)
				{
					return false;
				}

				// Exactly one unassigned literal => must be set to satisfy the clause.
				if (unassignedCount == 1)
				{
					var variable = Math.Abs(unassignedLit);
					var sign = unassignedLit > 0;
					_assignment[variable] = sign;
					isChanged = true;
				}
			}
		} while (isChanged);
		return true;
	}

	/// <summary>
	/// Find a variable index that has not been assigned yet (0).
	/// Returns -1 if all variables are assigned.
	/// </summary>
	private int GetUnassignedVar()
	{
		for (var i = 1; i <= _formula.NumVars; i++)
		{
			if (_assignment[i] is null)
			{
				return i;
			}
		}
		return -1;
	}

	/// <summary>
	/// After solving, retrieve the assignment array
	/// (index: <c>variable</c> is either <see langword="true"/> or <see langword="false"/>).
	/// </summary>
	public bool?[] GetAssignmentStates() => _assignment;
}
