#if ENABLE_SMT_SOLVER
using Microsoft.Z3;

namespace Sudoku.Solving.BooleanSatisfiability.ModuloTheories;

/// <summary>
/// Represents a solver, using SMT algorithm approach.
/// </summary>
/// <remarks>
/// This algorithm requires package <see href="https://github.com/Z3Prover/z3">Z3</see> to construct model
/// because I don't think I have any ability to provide with a package-free implementation.
/// </remarks>
public sealed class SmtSolver : ISolver
{
	/// <inheritdoc/>
	string? ISolver.UriLink => "https://en.wikipedia.org/wiki/Satisfiability_modulo_theories";


	/// <inheritdoc/>
	public bool? Solve(in Grid grid, out Grid result)
	{
		using var context = new Context();

		EncodeSudoku(grid, context, out var integerVariables, out var gridConstraints, out var givenCellsConstraints);

		var s = context.MkSolver();
		s.Assert(gridConstraints);
		s.Assert(givenCellsConstraints);

		if (s.Check() != Status.SATISFIABLE)
		{
			result = Grid.Undefined;
			return null;
		}

		var model = s.Model;
		var resultExpression = new Expr[9, 9];
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				resultExpression[i, j] = model.Evaluate(integerVariables[i][j]);
			}
		}

		var resultValue = new int[81];
		for (var i = 0; i < 9; i++)
		{
			for (var j = 0; j < 9; j++)
			{
				resultValue[i * 9 + j] = int.Parse(resultExpression[i, j].ToString());
			}
		}

		result = Grid.Create(resultValue, GridCreatingOption.MinusOne);
		return false;
	}


	/// <summary>
	/// Encode sudoku as constraints.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="context">The context.</param>
	/// <param name="integerVariables">Indicates integer variables representing result values.</param>
	/// <param name="gridConstraints">Indicates the grid constraints.</param>
	/// <param name="givenCellsConstraints">Indicates given cells constraints.</param>
	private void EncodeSudoku(
		in Grid grid,
		Context context,
		out IntExpr[][] integerVariables,
		out BoolExpr gridConstraints,
		out BoolExpr givenCellsConstraints
	)
	{
		// 9x9 matrix of integer variables.
		integerVariables = new IntExpr[9][];
		for (var i = 0; i < 9; i++)
		{
			integerVariables[i] = new IntExpr[9];
			for (var j = 0; j < 9; j++)
			{
				integerVariables[i][j] = (IntExpr)context.MkConst(context.MkSymbol($"r{i + 1}c{j + 1}"), context.IntSort);
			}
		}

		// Cell constraints: Each cell contains a value in [1..9].
		var cellConstraints = new Expr[9][];
		for (var i = 0; i < 9; i++)
		{
			cellConstraints[i] = new BoolExpr[9];
			for (var j = 0; j < 9; j++)
			{
				cellConstraints[i][j] = context.MkAnd(
					context.MkGe(integerVariables[i][j], context.MkInt(1)),
					context.MkLe(integerVariables[i][j], context.MkInt(9))
				);
			}
		}

		// Row constraints: Each row contains a digit at most once.
		var rowConstraints = new BoolExpr[9];
		for (var i = 0; i < 9; i++)
		{
			rowConstraints[i] = context.MkDistinct(integerVariables[i]);
		}

		// Column constraints: Each column contains a digit at most once.
		var columnConstraints = new BoolExpr[9];
		for (var j = 0; j < 9; j++)
		{
			var column = new IntExpr[9];
			for (var i = 0; i < 9; i++)
			{
				column[i] = integerVariables[i][j];
			}
			columnConstraints[j] = context.MkDistinct(column);
		}

		// Block constraints: each block contains a digit at most once.
		var blockConstraints = new BoolExpr[3][];
		for (var i0 = 0; i0 < 3; i0++)
		{
			blockConstraints[i0] = new BoolExpr[3];
			for (var j0 = 0; j0 < 3; j0++)
			{
				var block = new IntExpr[9];
				for (var i = 0; i < 3; i++)
				{
					for (var j = 0; j < 3; j++)
					{
						block[3 * i + j] = integerVariables[3 * i0 + i][3 * j0 + j];
					}
				}
				blockConstraints[i0][j0] = context.MkDistinct(block);
			}
		}

		gridConstraints = context.MkTrue();
		foreach (var t in cellConstraints)
		{
			gridConstraints = context.MkAnd(context.MkAnd((BoolExpr[])t), gridConstraints);
		}

		gridConstraints = context.MkAnd(context.MkAnd(rowConstraints), gridConstraints);
		gridConstraints = context.MkAnd(context.MkAnd(columnConstraints), gridConstraints);
		foreach (var t in blockConstraints)
		{
			gridConstraints = context.MkAnd(context.MkAnd(t), gridConstraints);
		}

		givenCellsConstraints = context.MkTrue();
		for (var cell = 0; cell < 81; cell++)
		{
			if (grid.GetDigit(cell) is var digit and not -1)
			{
				givenCellsConstraints = context.MkAnd(
					givenCellsConstraints,
					(BoolExpr)context.MkITE(
						context.MkEq(context.MkInt(digit + 1), context.MkInt(0)),
						context.MkTrue(),
						context.MkEq(integerVariables[cell / 9][cell % 9], context.MkInt(digit + 1))
					)
				);
			}
		}
	}
}
#endif
