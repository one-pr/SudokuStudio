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
	public SerializableColor BackgroundColor { get; set; } = new(SKColors.White);

	/// <summary>
	/// Indicates stroke color of candidate auxiliary lines. By default it's black (#FF000000).
	/// </summary>
	public SerializableColor CandidateAuxiliaryLineStrokeColor { get; set; } = new(SKColors.Black);

	/// <summary>
	/// Indicates stroke color of grid lines. By default it's black (#FF000000).
	/// </summary>
	public SerializableColor GridLineStrokeColor { get; set; } = new(SKColors.Black);

	/// <summary>
	/// Indicates stroke color of block lines. By default it's black (#FF000000).
	/// </summary>
	public SerializableColor BlockLineStrokeColor { get; set; } = new(SKColors.Black);

	/// <summary>
	/// Indicates color of given digits. By default it's black (#FF000000).
	/// </summary>
	public SerializableColor GivenDigitsColor { get; set; } = new(SKColors.Black);

	/// <summary>
	/// Indicates color of modifiable digits. By default it's blue (#FF0000FF).
	/// </summary>
	public SerializableColor ModifiableDigitsColor { get; set; } = new(SKColors.Blue);

	/// <summary>
	/// Indicates color of candidates. By default it's dim gray (#FF696969).
	/// </summary>
	public SerializableColor CandidatesColor { get; set; } = new(SKColors.DimGray);

	/// <summary>
	/// Indicates the normal color.
	/// </summary>
	public SerializableColor NormalColor { get; set; } = new(63, 218, 101); // Green

	/// <summary>
	/// Indicates the color that draws for an assignment.
	/// </summary>
	public SerializableColor AssignmentColor { set; get; } = new(63, 218, 101); // Green

	/// <summary>
	/// Indicates the color that draws for an overlapped assignment.
	/// </summary>
	public SerializableColor OverlappedAssignmentColor { set; get; } = new(0, 255, 204); // Aqua

	/// <summary>
	/// Indicates the elimination color.
	/// </summary>
	public SerializableColor EliminationColor { get; set; } = new(255, 118, 132); // Red

	/// <summary>
	/// Indicates the cannibalism color.
	/// </summary>
	public SerializableColor CannibalismColor { get; set; } = new(235, 0, 0); // Dark red

	/// <summary>
	/// Indicates the exo-fin color.
	/// </summary>
	public SerializableColor ExofinColor { get; set; } = new(255, 192, 89); // Orange

	/// <summary>
	/// Indicates the endo-fin color.
	/// </summary>
	public SerializableColor EndofinColor { get; set; } = new(216, 178, 255); // Purple

	/// <summary>
	/// Indicates the link color.
	/// </summary>
	public SerializableColor LinkColor { get; set; } = new(SKColors.Red);

	/// <summary>
	/// Indicates the auxiliary color set.
	/// </summary>
	public SerializableColorCollection AuxiliaryColors { get; set; } = [
		new(255, 192, 89), // Orange
		new(127, 187, 255), // Skyblue
		new(216, 178, 255) // Purple
	];

	/// <summary>
	/// Indicates the almost locked set color set.
	/// </summary>
	public SerializableColorCollection AlmostLockedSetColors { get; set; } = [
		new(220, 212, 252), // Purple
		new(255, 118, 132), // Red
		new(206, 251, 237), // Light skyblue
		new(215, 255, 215), // Light green
		new(192, 192, 192) // Gray
	];

	/// <summary>
	/// Indicates the almost rectangle color set.
	/// </summary>
	public SerializableColorCollection RectangleColors { get; set; } = [
		new(216, 178, 255), // Purple
		new(204, 150, 248), // Purple
		new(114, 82, 170), // Dark purple
	];

	/// <summary>
	/// Indicates the user-defined color palette.
	/// </summary>
	public SerializableColorCollection UserDefinedColorPalette { get; set; } = [
		new(63, 218, 101), // Green
		new(255, 192, 89), // Orange
		new(127, 187, 255), // Skyblue
		new(216, 178, 255), // Purple
		new(197, 232, 140), // Yellowish green
		new(255, 203, 203), // Light red
		new(178, 223, 223), // Blue green
		new(252, 220, 165), // Light orange
		new(255, 255, 150), // Yellow
		new(247, 222, 143), // Golden yellow
		new(220, 212, 252), // Purple
		new(255, 118, 132), // Red
		new(206, 251, 237), // Light skyblue
		new(215, 255, 215), // Light green
		new(192, 192, 192) // Gray
	];


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
