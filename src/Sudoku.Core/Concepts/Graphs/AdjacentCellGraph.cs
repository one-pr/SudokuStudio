namespace Sudoku.Concepts.Graphs;

/// <summary>
/// Represents a graph that displays a list of <see cref="Cell"/> instances connected with rows and columns of adjacent positions.
/// This graph will be useful to measure border lines.
/// </summary>
/// <seealso cref="Cell"/>
public readonly ref struct AdjacentCellGraph : IEquatable<AdjacentCellGraph>
{
	/// <summary>
	/// Indicates the backing field of cells.
	/// </summary>
	public readonly ref readonly CellMap Cells;


	/// <summary>
	/// Creates an <see cref="AdjacentCellGraph"/> instance via a list of cells connected by rows and columns.
	/// </summary>
	/// <param name="cells">A list of cells.</param>
	/// <returns>An <see cref="AdjacentCellGraph"/> instance created.</returns>
	/// <exception cref="ArgumentException">
	/// Throws when at least one cell is missing connection with all rows and columns of the other cells.
	/// </exception>
	public AdjacentCellGraph(ref readonly CellMap cells)
	{
		if (!verify(cells))
		{
			throw new ArgumentException(SR.ExceptionMessage("AllCellsMustBeConnectedWithAdjacentPositions"), nameof(cells));
		}
		Cells = ref cells;


		static bool verify(in CellMap cells)
		{
			// If the collection has no elements, return true to tell the runtime that it is expected and valid.
			if (!cells)
			{
				return true;
			}

			// Then we should recursively check for the last cells.
			var lastCells = cells;
			while (lastCells)
			{
				foreach (var cell in cells)
				{
					lastCells &= ~Peer.PeersMap[cell];
					if (!verify(lastCells))
					{
						return false;
					}
				}
			}
			return true;
		}
	}


	/// <summary>
	/// Try to get a list of cells and its corresponding directions that inner border lines will be created.
	/// </summary>
	public ReadOnlySpan<(Cell Cell, AdjacentCellDirection Directions)> InnerBorderLines
	{
		get
		{
			// The basic algorithm is to find connected directions, and removed from the initial collection.
			// The initial collection should make all cells as marked with all 4 directions.
			var dictionary = new Dictionary<Cell, AdjacentCellDirection>(Cells.Count);
			foreach (var cell in Cells)
			{
				dictionary.Add(cell, AdjacentCellDirections.All);
			}

			foreach (var cell in Cells)
			{
				if (cell is not (>= 0 and < 9) && Cells.Contains(cell - 9)) { dictionary[cell] &= ~AdjacentCellDirection.Up; }
				if (cell is not (>= 72 and < 81) && Cells.Contains(cell + 9)) { dictionary[cell] &= ~AdjacentCellDirection.Down; }
				if (cell % 9 != 0 && Cells.Contains(cell - 1)) { dictionary[cell] &= ~AdjacentCellDirection.Left; }
				if ((cell + 1) % 9 != 0 && Cells.Contains(cell + 1)) { dictionary[cell] &= ~AdjacentCellDirection.Right; }
			}

			var result = new List<(Cell, AdjacentCellDirection)>(Cells.Count);
			foreach (var cell in Cells)
			{
				result.Add((cell, dictionary[cell]));
			}
			return result.AsSpan();
		}
	}


	/// <inheritdoc cref="ReadOnlySpan{T}.Equals"/>
	public override bool Equals(object? obj) => false;

	/// <inheritdoc/>
	public bool Equals(AdjacentCellGraph other) => Cells == other.Cells;

	/// <inheritdoc/>
	public override int GetHashCode() => Cells.GetHashCode();

	/// <inheritdoc cref="object.ToString"/>
	public override string ToString() => ToString(new RxCyConverter());

	/// <summary>
	/// Converts the current instance into string representation via the specified coordinate converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string result.</returns>
	public string ToString(CoordinateConverter converter) => converter.CellConverter(Cells);

	/// <summary>
	/// Converts the current instance into string representation via the specified cell map converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <returns>The string result.</returns>
	public string ToString(ICellMapConverter converter)
		=> converter.TryFormat(in Cells, null, out var result) ? result : throw new FormatException();


	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Equality(TSelf, TOther)"/>
	public static bool operator ==(AdjacentCellGraph left, AdjacentCellGraph right) => left.Equals(right);

	/// <inheritdoc cref="IEqualityOperators{TSelf, TOther, TResult}.op_Inequality(TSelf, TOther)"/>
	public static bool operator !=(AdjacentCellGraph left, AdjacentCellGraph right) => !(left == right);
}
