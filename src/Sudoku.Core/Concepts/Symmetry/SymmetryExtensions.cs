namespace Sudoku.Concepts.Symmetry;

/// <summary>
/// Provides extension methods that calculates symmetric type of variant sudoku concept types.
/// </summary>
public static class SymmetryExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(in CellMap @this)
	{
		/// <summary>
		/// Indicates symmetric type of the cells formed.
		/// </summary>
		public SymmetricType Symmetry
		{
			get
			{
				foreach (var symmetry in ~SymmetricType.AllValues[1..])
				{
					var isThisSymmetry = true;
					foreach (var cell in @this)
					{
						var symmetricCells = symmetry.GetOrbit(cell);
						if ((@this & symmetricCells) != symmetricCells)
						{
							isThisSymmetry = false;
							break;
						}
					}
					if (!isThisSymmetry)
					{
						continue;
					}
					return symmetry;
				}
				return SymmetricType.None;
			}
		}
	}

	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(in Grid @this)
	{
		/// <summary>
		/// Indicates the symmetric type of given cells formed in the grid.
		/// </summary>
		public SymmetricType Symmetry => @this.GivenCells.Symmetry;
	}
}
