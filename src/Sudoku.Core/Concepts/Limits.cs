namespace Sudoku.Concepts;

/// <summary>
/// Provides an easy entry to visit limits of types.
/// </summary>
public static partial class Limits
{
	/// <summary>
	/// Returns min value of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	/// <returns>The result.</returns>
	public static T GetMin<T>() where T : IMinMaxValue<T> => T.MinValue;

	/// <summary>
	/// Returns max value of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type.</typeparam>
	/// <returns>The result.</returns>
	public static T GetMax<T>() where T : IMinMaxValue<T> => T.MaxValue;
}
