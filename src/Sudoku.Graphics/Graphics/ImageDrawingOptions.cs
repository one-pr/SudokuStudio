namespace Sudoku.Graphics;

/// <summary>
/// Represents image drawing options.
/// </summary>
public sealed partial class ImageDrawingOptions
{
	/// <summary>
	/// Indicates the default options.
	/// </summary>
	public static readonly ImageDrawingOptions Default = new();


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
	/// Indicates stroke thickness of grid lines. By default it's 1.5.
	/// </summary>
	public float GridLineStrokeThickness { get; set; } = 2F;

	/// <summary>
	/// Indicates stroke thickness of block lines. By default it's 3.
	/// </summary>
	public float BlockLineStrokeThickness { get; set; } = 8F;

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
}
