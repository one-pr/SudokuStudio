namespace Sudoku.Analytics.BabaGrouping;

/// <summary>
/// Provides a context to be used in group spreading.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
public readonly ref struct GroupSpreadingContext(ref readonly Grid grid)
{
	/// <summary>
	/// Indicates the grid to be used.
	/// </summary>
	public readonly ref readonly Grid Grid = ref grid;


	/// <summary>
	/// Indicates whether the spreading method only find for one conclusion.
	/// </summary>
	public required bool OnlyFindOne { get; init; }

	/// <summary>
	/// Indicates the cancellation token.
	/// </summary>
	public CancellationToken CancellationToken { get; init; }

	/// <summary>
	/// Indicates all mapped symbols.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This property is a dictionary that stores cached symbols applied into the grid, preventing them recording twice.
	/// </para>
	/// <para>
	/// This can also be checked in the phase of conclusions. There are the cases can be concluded:
	/// <list type="number">
	/// <item>A symbol must be equal to a value cell in a house</item>
	/// <item>Some symbols should form a subset or a distributed disjointed subset</item>
	/// <item>A cell should be filled with one number defined in a symbol</item>
	/// </list>
	/// </para>
	/// </remarks>
	public Dictionary<CellMap, ComplexCellSymbol> MappedSymbols { get; } = [];

	/// <summary>
	/// Indicates the dictionary that stores symbol values and corresponding digits.
	/// </summary>
	public Dictionary<CellSymbolValue, Mask> SymbolValuesLookup { get; } = [];

	/// <summary>
	/// Indicates the found symbols.
	/// </summary>
	public HashSet<ComplexCellSymbol> FoundSymbols { get; } = [];
}
