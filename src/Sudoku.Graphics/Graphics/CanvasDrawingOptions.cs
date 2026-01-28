namespace Sudoku.Graphics;

/// <summary>
/// Represents drawing options.
/// </summary>
public sealed partial class CanvasDrawingOptions
{
	/// <summary>
	/// Indicates the default options.
	/// </summary>
	public static readonly CanvasDrawingOptions Default = new();


	/// <summary>
	/// Indicates whether candidate auxiliary lines are also drawn or not. By default it's <see langword="false"/>.
	/// </summary>
	public bool DrawCandidateAuxiliaryLines { get; set; } = false;

	/// <summary>
	/// Indicates stroke thickness of candidate auxiliary lines. By default it's 0.5.
	/// The value should be consumed if <see cref="DrawCandidateAuxiliaryLines"/> is <see langword="true"/>.
	/// </summary>
	/// <seealso cref="DrawCandidateAuxiliaryLines"/>
	public float CandidateAuxiliaryLineStrokeThickness { get; set; } = .5F;

	/// <summary>
	/// Indicates stroke thickness of grid lines. By default it's 2.
	/// </summary>
	public float GridLineStrokeThickness { get; set; } = 2F;

	/// <summary>
	/// Indicates stroke thickness of block lines. By default it's 8.
	/// </summary>
	public float BlockLineStrokeThickness { get; set; } = 8F;

	/// <summary>
	/// Indicates font size of given digits. By default it's 60.
	/// </summary>
	public float GivenDigitsFontSize { get; set; } = 60F;

	/// <summary>
	/// Indicates font size of modifiable digits. By default it's 60.
	/// </summary>
	public float ModifiableDigitsFontSize { get; set; } = 60F;

	/// <summary>
	/// Indicates font size of candidates. By default it's 20.
	/// </summary>
	public float CandidatesFontSize { get; set; } = 20F;

	/// <summary>
	/// Indicates given digits font name. By default it's <c>"Tahoma"</c>.
	/// </summary>
	public string GivenDigitsFontName { get; set; } = "Tahoma";

	/// <summary>
	/// Indicates modifiable digits font name. By default it's <c>"Tahoma"</c>.
	/// </summary>
	public string ModifiableDigitsFontName { get; set; } = "Tahoma";

	/// <summary>
	/// Indicates candidates font name. By default it's <c>"Tahoma"</c>.
	/// </summary>
	public string CandidatesFontName { get; set; } = "Tahoma";

	/// <summary>
	/// Indicates grid line dash sequence. By default it's empty array.
	/// </summary>
	[JsonConverter(typeof(LineDashSequenceConverter))]
	public LineDashSequence GridLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates cell line dash sequence. By default it's empty array.
	/// </summary>
	[JsonConverter(typeof(LineDashSequenceConverter))]
	public LineDashSequence BlockLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates candidate auxiliary line dash sequence. By default it's empty array.
	/// </summary>
	[JsonConverter(typeof(LineDashSequenceConverter))]
	public LineDashSequence CandidateAuxiliaryLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates background color.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor BackgroundColor { get; set; } = SKColors.White;

	/// <summary>
	/// Indicates stroke color of candidate auxiliary lines. By default it's <see cref="SKColors.Black"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor CandidateAuxiliaryLineStrokeColor { get; set; } = SKColors.Black;

	/// <summary>
	/// Indicates stroke color of grid lines. By default it's <see cref="SKColors.Black"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor GridLineStrokeColor { get; set; } = SKColors.Black;

	/// <summary>
	/// Indicates stroke color of block lines. By default it's <see cref="SKColors.Black"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor BlockLineStrokeColor { get; set; } = SKColors.Black;

	/// <summary>
	/// Indicates color of given digits. By default it's <see cref="SKColors.Black"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor GivenDigitsColor { get; set; } = SKColors.Black;

	/// <summary>
	/// Indicates color of modifiable digits. By default it's <see cref="SKColors.Blue"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor ModifiableDigitsColor { get; set; } = SKColors.Blue;

	/// <summary>
	/// Indicates color of candidates. By default it's <see cref="SKColors.DimGray"/>.
	/// </summary>
	[JsonConverter(typeof(SKColorConverter))]
	public SKColor CandidatesColor { get; set; } = SKColors.DimGray;


	/// <summary>
	/// Loads configuration from JSON file, specified by its path.
	/// If the specified file doesn't exist, <see langword="null"/> will be returned.
	/// </summary>
	/// <param name="filePath">The target file path.</param>
	/// <returns>The options loaded.</returns>
	public static CanvasDrawingOptions? LoadFrom(string filePath)
		=> File.Exists(filePath) ? JsonSerializer.Deserialize<CanvasDrawingOptions>(File.ReadAllText(filePath)) : null;

	/// <summary>
	/// Saves the specified options, converting it into JSON string and save to the specified file.
	/// If the specified file exists, truncated and overwritten;
	/// if <paramref name="options"/> is <see langword="null"/>, do nothing.
	/// </summary>
	/// <param name="options">The instance.</param>
	/// <param name="filePath">The target file path.</param>
	public static void SaveTo(CanvasDrawingOptions? options, string filePath)
	{
		if (options is not null)
		{
			File.WriteAllText(filePath, JsonSerializer.Serialize(options));
		}
	}
}
