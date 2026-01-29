namespace Sudoku.Graphics;

/// <summary>
/// Represents drawing options.
/// </summary>
public sealed class CanvasDrawingOptions
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
	/// Indicates given digits font weight. By default it's <see cref="SKFontStyleWeight.Normal"/>.
	/// </summary>
	public SKFontStyleWeight GivenDigitsFontWeight { get; set; } = SKFontStyleWeight.Normal;

	/// <summary>
	/// Indicates modifiable digits font weight. By default it's <see cref="SKFontStyleWeight.Normal"/>.
	/// </summary>
	public SKFontStyleWeight ModifiableDigitsFontWeight { get; set; } = SKFontStyleWeight.Normal;

	/// <summary>
	/// Indicates candidates font weight. By default it's <see cref="SKFontStyleWeight.Normal"/>.
	/// </summary>
	public SKFontStyleWeight CandidatesFontWeight { get; set; } = SKFontStyleWeight.Normal;

	/// <summary>
	/// Indicates given digits font width. By default it's <see cref="SKFontStyleWidth.Normal"/>.
	/// </summary>
	public SKFontStyleWidth GivenDigitsFontWidth { get; set; } = SKFontStyleWidth.Normal;

	/// <summary>
	/// Indicates modifiable digits font width. By default it's <see cref="SKFontStyleWidth.Normal"/>.
	/// </summary>
	public SKFontStyleWidth ModifiableDigitsFontWidth { get; set; } = SKFontStyleWidth.Normal;

	/// <summary>
	/// Indicates candidates font width. By default it's <see cref="SKFontStyleWidth.Normal"/>.
	/// </summary>
	public SKFontStyleWidth CandidatesFontWidth { get; set; } = SKFontStyleWidth.Normal;

	/// <summary>
	/// Indicates given digits font slant. By default it's <see cref="SKFontStyleSlant.Upright"/> (default).
	/// </summary>
	public SKFontStyleSlant GivenDigitsFontSlant { get; set; } = SKFontStyleSlant.Upright;

	/// <summary>
	/// Indicates modifiable digits font slant. By default it's <see cref="SKFontStyleSlant.Upright"/> (default).
	/// </summary>
	public SKFontStyleSlant ModifiableDigitsFontSlant { get; set; } = SKFontStyleSlant.Upright;

	/// <summary>
	/// Indicates candidates font slant. By default it's <see cref="SKFontStyleSlant.Upright"/> (default).
	/// </summary>
	public SKFontStyleSlant CandidatesFontSlant { get; set; } = SKFontStyleSlant.Upright;

	/// <summary>
	/// Indicates stroke thickness ratio of candidate auxiliary lines, relative to candidate size.
	/// By default it's 0.0125.
	/// The value should be consumed if <see cref="DrawCandidateAuxiliaryLines"/> is <see langword="true"/>.
	/// </summary>
	/// <remarks>
	/// <para>
	/// The ratio value can be calculated by <c>fact-stroke-thickness / candidate-size</c>,
	/// or another formula <c>fact-stroke-thickness = candidate-size * ratio</c>.
	/// </para>
	/// <para>
	/// For example, if <c>candidate-size</c> is 40 and <c>fact-stroke-thickness</c> is 0.5,
	/// then we can get <c>ratio = 0.5 / 40 = 0.0125</c>.
	/// </para>
	/// </remarks>
	/// <seealso cref="DrawCandidateAuxiliaryLines"/>
	public Ratio CandidateAuxiliaryLineStrokeThicknessRatio { get; set; } = .0125F;

	/// <summary>
	/// Indicates stroke thickness ratio of normal cell lines, relative to candidate size.
	/// By default it's 0.05 (4 times with <see cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>).
	/// </summary>
	/// <remarks><inheritdoc cref="CandidateAuxiliaryLineStrokeThicknessRatio" path="/remarks"/></remarks>
	/// <seealso cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>
	public Ratio GridLineStrokeThicknessRatio { get; set; } = .05F;

	/// <summary>
	/// Indicates stroke thickness ratio of block lines, relative to candidate size.
	/// By default it's 0.2 (16 times with <see cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>).
	/// </summary>
	/// <remarks><inheritdoc cref="CandidateAuxiliaryLineStrokeThicknessRatio" path="/remarks"/></remarks>
	/// <seealso cref="CandidateAuxiliaryLineStrokeThicknessRatio"/>
	public Ratio BlockLineStrokeThicknessRatio { get; set; } = .2F;

	/// <summary>
	/// Indicates font size ratio of given digits, relative to cell size. By default it's 75%.
	/// </summary>
	public Ratio GivenDigitsFontSizeRatio { get; set; } = .75F;

	/// <summary>
	/// Indicates font size ratio of modifiable digits, relative to cell size. By default it's 75%.
	/// </summary>
	public Ratio ModifiableDigitsFontSizeRatio { get; set; } = .75F;

	/// <summary>
	/// Indicates font size ratio of candidates, relative to cell size. By default it's 25%.
	/// </summary>
	public Ratio CandidatesFontSizeRatio { get; set; } = .25F;

	/// <summary>
	/// Indicates grid line dash sequence. By default it's empty array.
	/// </summary>
	public LineDashSequence GridLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates cell line dash sequence. By default it's empty array.
	/// </summary>
	public LineDashSequence BlockLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates candidate auxiliary line dash sequence. By default it's empty array.
	/// </summary>
	public LineDashSequence CandidateAuxiliaryLineDashSequence { get; set; } = [];

	/// <summary>
	/// Indicates background color. By default it's white (#FFFFFFFF).
	/// </summary>
	public ColorDescriptor BackgroundColor { get; set; } = (255, 255, 255, 255);

	/// <summary>
	/// Indicates stroke color of candidate auxiliary lines. By default it's black (#FF000000).
	/// </summary>
	public ColorDescriptor CandidateAuxiliaryLineStrokeColor { get; set; } = (255, 0, 0, 0);

	/// <summary>
	/// Indicates stroke color of grid lines. By default it's black (#FF000000).
	/// </summary>
	public ColorDescriptor GridLineStrokeColor { get; set; } = (255, 0, 0, 0);

	/// <summary>
	/// Indicates stroke color of block lines. By default it's black (#FF000000).
	/// </summary>
	public ColorDescriptor BlockLineStrokeColor { get; set; } = (255, 0, 0, 0);

	/// <summary>
	/// Indicates color of given digits. By default it's black (#FF000000).
	/// </summary>
	public ColorDescriptor GivenDigitsColor { get; set; } = (255, 0, 0, 0);

	/// <summary>
	/// Indicates color of modifiable digits. By default it's blue (#FF0000FF).
	/// </summary>
	public ColorDescriptor ModifiableDigitsColor { get; set; } = (255, 0, 0, 255);

	/// <summary>
	/// Indicates color of candidates. By default it's dim gray (#FF696969).
	/// </summary>
	public ColorDescriptor CandidatesColor { get; set; } = (255, 105, 105, 105);


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
