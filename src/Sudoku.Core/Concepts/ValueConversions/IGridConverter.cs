namespace Sudoku.Concepts.ValueConversions;

/// <summary>
/// Provides an object that converts data between <see cref="string"/> and instantiated <see cref="Grid"/> instances.
/// </summary>
/// <seealso cref="Grid"/>
public interface IGridConverter
{
	/// <summary>
	/// Indicates the priority on parsing.
	/// </summary>
	/// <remarks>
	/// <para>
	/// If multiple different parsers can construct <see cref="Grid"/> instances,
	/// this value will adopt the first one with higher priority value;
	/// if different <see cref="IGridConverter"/> instances hold same parsing priority, a runtime exception will be thrown
	/// when all parser modules from them execute successfully.
	/// </para>
	/// <para>
	/// This value is only reserved for usages on distinction of specialized characters in some parsing rules.
	/// Like a pencilmark grid, the target string representation must contain a sequence of characters <c>"-+-"</c>,
	/// which isn't appeared in strings produced by the other grid conversion rules. If we can detect with that,
	/// the priority value can be greater than the others in order to prevent malformed string parsing error produced
	/// in some other parsing modules.
	/// </para>
	/// <para>
	/// The value can be -1 if such instances are not elements defined in built-in converters collection.
	/// </para>
	/// </remarks>
	int ParsingPriority { get; }


	/// <summary>
	/// Performs parsing operation.
	/// </summary>
	/// <param name="text">The text.</param>
	/// <param name="provider">The provider.</param>
	/// <param name="result">The result parsed.</param>
	/// <returns>A <see cref="bool"/> result indicating whether operation has encountered some invalid cases.</returns>
	bool TryParse(ReadOnlySpan<char> text, IFormatProvider? provider, out Grid result);

	/// <summary>
	/// Performs formatting operation.
	/// </summary>
	/// <param name="grid">The grid.</param>
	/// <param name="provider">The provider.</param>
	/// <param name="result">The result.</param>
	/// <returns>A <see cref="bool"/> result indicating whether operation has encountered some invalid cases.</returns>
	bool TryFormat(ref readonly Grid grid, IFormatProvider? provider, [NotNullWhen(true)] out string? result);
}
