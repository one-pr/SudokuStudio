namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// The base overrides for double exocet steps.
/// </summary>
internal interface IDoubleExocet : IComponent
{
	/// <summary>
	/// A list of cells as the base cells.
	/// </summary>
	CellMap BaseCells { get; }

	/// <summary>
	/// A list of cells as the target cells.
	/// </summary>
	CellMap TargetCells { get; }

	/// <summary>
	/// A list of cells as the other pair of base cells.
	/// </summary>
	CellMap BaseCellsTheOther { get; }

	/// <summary>
	/// A list of cells as the other pair of target cells.
	/// </summary>
	CellMap TargetCellsTheOther { get; }

	/// <inheritdoc/>
	ComponentType IComponent.Type => ComponentType.DoubleExocet;
}
