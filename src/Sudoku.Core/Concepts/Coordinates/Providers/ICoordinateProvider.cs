namespace Sudoku.Concepts.Coordinates.Providers;

/// <summary>
/// Represents a type that supports formatting or parsing rules around coordinates.
/// </summary>
/// <typeparam name="TSelf"><include file="../../global-doc-comments.xml" path="/g/self-type-constraint"/></typeparam>
public interface ICoordinateProvider<out TSelf> where TSelf : ICoordinateProvider<TSelf>, allows ref struct
{
	/// <summary>
	/// Indicates the <typeparamref name="TSelf"/> instance using invariant culture, meaning it ignores culture your device uses.
	/// </summary>
	static abstract TSelf InvariantCulture { get; }


	/// <summary>
	/// Try to get a <typeparamref name="TSelf"/> instance from the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>A <typeparamref name="TSelf"/> instance from the specified culture.</returns>
	static abstract TSelf GetInstance(CultureInfo? culture);
}
