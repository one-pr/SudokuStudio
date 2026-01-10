namespace Sudoku.Concepts.Coordinates;

/// <summary>
/// Provides with a formatter object that convert the current map instance
/// into a <see cref="string"/> representation equivalent to the current object.
/// </summary>
/// <typeparam name="T">The type of map.</typeparam>
/// <typeparam name="TElement">The type of each element.</typeparam>
/// <param name="map">The map to be converted.</param>
/// <returns>The string result.</returns>
public delegate string BitStateMapFormatter<T, TElement>(in T map)
	where T : unmanaged, IBitStateMap<T, TElement>
	where TElement : notnull;
