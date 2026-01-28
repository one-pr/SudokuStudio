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
	/// Indicates stroke thickness ratio of candidate auxiliary lines, relative to grid size.
	/// By default it's 4.63E-4F (approximately 0.0463%).
	/// Approximately thickness <b>0.5</b> in a <b>1080 * 1080</b> picture.
	/// The value should be consumed if <see cref="DrawCandidateAuxiliaryLines"/> is <see langword="true"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The ratio value can be calculated by <c>fact-stroke-thickness / grid-size</c>,
	/// or another formula <c>fact-stroke-thickness = grid-size * ratio</c>.
	/// </para>
	/// <para>
	/// For example, if <c>picture-size</c> is 1100, and <c>margin</c> is 10,
	/// and <c>fact-stroke-thickness</c> is 0.5,
	/// then we can get <c>ratio = 0.5 / (1100 - 2 * 10) = 0.0004[629] ~= 4.629E-4 ~= 0.046%</c>.
	/// </para>
	/// <para><i>
	/// Well, I know this design is a little bit weird but I do want to design it using a relative system
	/// instead of traditional pixel-based absolute values :)
	/// </i></para>
	/// </remarks>
	/// <seealso cref="DrawCandidateAuxiliaryLines"/>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio CandidateAuxiliaryLineStrokeThicknessRatio { get; set; } = 4.63E-4F;

	/// <summary>
	/// Indicates stroke thickness ratio of grid lines, relative to grid size.
	/// By default it's 1.85E-3 (approximately 0.185%,
	/// 4 times with <see cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>).
	/// Approximately thickness <b>2</b> in a <b>1080 * 1080</b> picture.
	/// </summary>
	/// <remarks><inheritdoc cref="CandidateAuxiliaryLineStrokeThicknessRatio" path="/remarks"/></remarks>
	/// <seealso cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio GridLineStrokeThicknessRatio { get; set; } = 1.85E-3F;

	/// <summary>
	/// Indicates stroke thickness ratio of block lines, relative to grid size.
	/// By default it's 7.41E-3 (approximately 0.741%,
	/// 8 times with <see cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>).
	/// Approximately thickness <b>4</b> in a <b>1080 * 1080</b> picture.
	/// </summary>
	/// <remarks><inheritdoc cref="CandidateAuxiliaryLineStrokeThicknessRatio" path="/remarks"/></remarks>
	/// <seealso cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio BlockLineStrokeThicknessRatio { get; set; } = 7.41E-3F;

	/// <summary>
	/// Indicates font size ratio of given digits, relative to cell size.
	/// By default it's 75%.
	/// </summary>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio GivenDigitsFontSizeRatio { get; set; } = .75F;

	/// <summary>
	/// Indicates font size ratio of modifiable digits, relative to cell size.
	/// By default it's 75%.
	/// </summary>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio ModifiableDigitsFontSizeRatio { get; set; } = .75F;

	/// <summary>
	/// Indicates font size ratio of candidates, relative to cell size.
	/// By default it's 25%.
	/// </summary>
	[JsonConverter(typeof(RatioConverter))]
	public Ratio CandidatesFontSizeRatio { get; set; } = .25F;

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
