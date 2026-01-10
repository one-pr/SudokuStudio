namespace Sudoku.Analytics.Construction.Components;

/// <summary>
/// Represents a component that is a chain or forcing chains.
/// </summary>
public interface IChainOrForcingChains : IComponent
{
	/// <summary>
	/// Indicates whether the pattern is grouped (i.e. contains a node uses at least 2 candidates).
	/// </summary>
	bool IsGrouped { get; }

	/// <summary>
	/// Indicates whether the pattern is strictly grouped,
	/// meaning at least one link (no matter what kind of link, strong or weak) uses advanced checking rules like AUR and ALS,
	/// or returns <see langword="true"/> from property <see cref="IsGrouped"/>.
	/// </summary>
	/// <seealso cref="IsGrouped"/>
	bool IsStrictlyGrouped { get; }


	/// <inheritdoc cref="object.ToString"/>
	string ToString();

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified culture.
	/// </summary>
	/// <param name="culture">The culture.</param>
	/// <returns>The string.</returns>
	string ToString(CultureInfo culture);

	/// <inheritdoc cref="ToString(IChainConverter, IFormatProvider?)"/>
	string ToString(CoordinateConverter converter);

	/// <inheritdoc cref="ToString(IChainConverter, IFormatProvider?)"/>
	string ToString(IChainConverter converter);

	/// <summary>
	/// Converts the current instance into <see cref="string"/> representation via the specified converter.
	/// </summary>
	/// <param name="converter">The converter.</param>
	/// <param name="formatProvider">The format provider.</param>
	/// <returns>The string.</returns>
	string ToString(IChainConverter converter, IFormatProvider? formatProvider);
}
