namespace System.Text;

/// <summary>
/// Represents a way to describe table grid builder.
/// </summary>
public sealed record TableGridBuilderOptions
{
	/// <summary>
	/// Indicates the default option.
	/// </summary>
	public static readonly TableGridBuilderOptions Default = new();


	/// <summary>
	/// Whether to print outer table borders.
	/// </summary>
	public bool PrintBorders { get; init; } = true;

	/// <summary>
	/// Whether to print separators between every row.
	/// </summary>
	public bool PrintRowSeparators { get; init; } = false;

	/// <summary>
	/// Vertical border character(s), e.g., <c>"|"</c>.
	/// </summary>
	public string Vertical { get; init; } = "|";

	/// <summary>
	/// Horizontal border character(s), e.g., <c>"-"</c>.
	/// </summary>
	public string Horizontal { get; init; } = "-";

	/// <summary>
	/// Corner character(s), e.g., <c>"+"</c>.
	/// </summary>
	public string Corner { get; init; } = "+";
}
