namespace Sudoku.Concepts.Supersymmetry;

/// <summary>
/// Represents a way to find a path that connects block cells.
/// </summary>
public static class BlockCellConnection
{
	/// <summary>
	/// Try to get a list of pair of cells that describes the connection segments.
	/// This method can be used by forming a valid connection map used by rendering in XSudo like truths and links.
	/// </summary>
	/// <param name="cells">The cells to be traversed.</param>
	/// <returns>A list of pairs of cells as connection segments.</returns>
	/// <exception cref="ArgumentException">Throws when the specified cells cannot be covered by only one block.</exception>
	public static ReadOnlySpan<Cell> GetConnections(in CellMap cells)
		=> cells.SharedBlock != FallbackConstants.@int
			? new HamiltonianPathFinder(in cells).FindPath()
			: throw new ArgumentException("All cells must inside one same block.", nameof(cells));
}

/// <summary>
/// Represents a Hamiltonian path finder.
/// </summary>
/// <param name="points">The points.</param>
file ref struct HamiltonianPathFinder(ref readonly CellMap points)
{
	/// <summary>
	/// Indicates the original points.
	/// </summary>
	private readonly ref readonly CellMap _originalPoints = ref points;

	/// <summary>
	/// Indicates the graph.
	/// </summary>
	private readonly Dictionary<Cell, CellMap> _graph = [];

	/// <summary>
	/// Indicates all points.
	/// </summary>
	private CellMap _allPoints = points;


	/// <summary>
	/// Try to find a path; if none found, an empty array will be returned.
	/// </summary>
	/// <returns>A path with cells connected; if failed to find, an empty array will be returned.</returns>
	public ReadOnlySpan<Cell> FindPath()
	{
		var spiritCells = HousesMap[_originalPoints.SharedBlock] & ~_originalPoints;

		var isDefault = true;
		var result = (ReadOnlySpan<Cell>)new Cell[9];
		for (var c = 0; c <= spiritCells.Count; c++)
		{
			foreach (ref readonly var spirits in c == 0 ? [CellMap.Empty] : spiritCells & c)
			{
				var original = _allPoints;
				_allPoints |= spirits;

				fillWithSpirits(ref this);
				buildGraph(this);

				foreach (var start in _allPoints)
				{
					var path = new List<Cell>();
					var visited = new HashSet<Cell>();
					if (dfs(this, start, visited, path) && path.Count <= result.Length)
					{
						result = path.AsSpan();
						if (result.Length == _originalPoints.Count)
						{
							return result;
						}

						isDefault = false;
					}
				}

				// Revert the operation.
				_allPoints = original;
			}
		}
		return isDefault ? [] : result;


		static CellMap bfs_GetReachableCells(Cell start, in CellMap available)
		{
			var result = CellMap.Empty;
			var queue = new LinkedList<Cell>();
			queue.AddLast(start);
			result.Add(start);

			while (queue.Count != 0)
			{
				var current = queue.RemoveFirstNode();
				foreach (var neighbor in current.Neighbors)
				{
					if (available.Contains(neighbor) && !result.Contains(neighbor))
					{
						result.Add(neighbor);
						queue.AddLast(neighbor);
					}
				}
			}
			return result;
		}

		void buildGraph(in HamiltonianPathFinder @this)
		{
			var allPoints = @this._allPoints;
			foreach (var point in allPoints)
			{
				@this._graph[point] = from n in point.Neighbors where allPoints.Contains(n) select n;
			}
		}

		void fillWithSpirits(ref HamiltonianPathFinder @this)
		{
			var toCheck = new LinkedList<(Cell, Cell)>();
			var arr = @this._originalPoints.ToArray();

			for (var i = 0; i < arr.Length - 1; i++)
			{
				for (var j = i + 1; j < arr.Length; j++)
				{
					toCheck.AddLast((arr[i], arr[j]));
				}
			}

			ref var allPoints = ref @this._allPoints;
			while (toCheck.Count > 0)
			{
				var (a, b) = toCheck.RemoveFirstNode();
				var reachable = bfs_GetReachableCells(a, @this._allPoints);
				if (!reachable.Contains(b))
				{
					var bridge = -1;
					foreach (var n in a.Neighbors & b.Neighbors)
					{
						if (!allPoints.Contains(n))
						{
							bridge = n;
							break;
						}
					}
					if (bridge != -1)
					{
						allPoints.Add(bridge);
						foreach (var p in allPoints)
						{
							toCheck.AddLast((bridge, p));
						}
					}
				}
			}
		}

		bool dfs(in HamiltonianPathFinder @this, Cell current, HashSet<Cell> visited, List<Cell> path)
		{
			visited.Add(current);
			path.Add(current);

			if (visited.Count == @this._allPoints.Count)
			{
				return true;
			}

			foreach (var neighbor in @this._graph[current])
			{
				if (!visited.Contains(neighbor))
				{
					if (dfs(@this, neighbor, visited, path))
					{
						return true;
					}
				}
			}

			visited.Remove(current);
			path.RemoveAt(^1);
			return false;
		}
	}
}

/// <include file='../../global-doc-comments.xml' path='g/csharp11/feature[@name="file-local"]/target[@name="class" and @when="extension"]'/>
file static class Extensions
{
	/// <summary>
	/// Provides extension members on <see cref="Cell"/>.
	/// </summary>
	extension(Cell @this)
	{
		/// <summary>
		/// Indicates the neighbors.
		/// </summary>
		public CellMap Neighbors
		{
			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			get
			{
				var block = @this.ToHouse(HouseType.Block);

				var result = CellMap.Empty;
				if (@this - 9 is var up && up >= 0 && up.ToHouse(HouseType.Block) == block)
				{
					result.Add(up);
				}
				if (@this + 9 is var down && down < 81 && down.ToHouse(HouseType.Block) == block)
				{
					result.Add(down);
				}
				if (@this - 1 is var left && left >= 0 && left.ToHouse(HouseType.Block) == block)
				{
					result.Add(left);
				}
				if (@this + 1 is var right && right < 81 && right.ToHouse(HouseType.Block) == block)
				{
					result.Add(right);
				}
				return result;
			}
		}
	}
}
