namespace Sudoku.Graphics;

/// <summary>
/// Represents an image to a sudoku grid.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="mapper"><inheritdoc cref="Mapper" path="/summary"/></param>
public sealed class GridImage(in Grid grid, PointMapper mapper) :
	IDisposable,
	IGridImageDrawBackground,
	IGridImageDrawLine,
	IGridImageExport
{
	/// <summary>
	/// The backing field of property <see cref="Grid"/>.
	/// </summary>
	/// <seealso cref="Grid"/>
	private readonly Grid _grid = grid;

	/// <summary>
	/// Indicates the backing surface object.
	/// </summary>
	private readonly SKSurface _surface = SKSurface.Create(new SKImageInfo(mapper.Size, mapper.Size));

	/// <summary>
	/// Indicates whether the object has already been disposed.
	/// </summary>
	private bool _isDisposed;


	/// <summary>
	/// Indicates the grid used.
	/// </summary>
	public ref readonly Grid Grid => ref _grid;

	/// <summary>
	/// Indicates the mapper.
	/// </summary>
	public PointMapper Mapper { get; } = mapper;

	/// <summary>
	/// Indicates the canvas that allows you drawing on it.
	/// </summary>
	public SKCanvas Canvas => _surface.Canvas;


	/// <inheritdoc/>
	public void Dispose()
	{
		ObjectDisposedException.ThrowIf(_isDisposed, this);

		_surface.Dispose();
		_isDisposed = true;
	}

	/// <inheritdoc/>
	public void DrawBackground(SKColor color) => Canvas.Clear(color);

	/// <inheritdoc/>
	public void DrawGridLine(ImageDrawingOptions? options = null)
	{
		options ??= new();

		using var blockLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.BlockLineStrokeThickness,
			Color = options.BlockLineStrokeColor,
			IsAntialias = true,
			PathEffect = options.BlockLineDashSequence.IsEmpty ? null : options.BlockLineDashSequence
		};
		using var cellLinePaint = new SKPaint
		{
			Style = SKPaintStyle.Stroke,
			StrokeWidth = options.GridLineStrokeThickness,
			Color = options.GridLineStrokeColor,
			IsAntialias = true,
			PathEffect = options.GridLineDashSequence.IsEmpty ? null : options.GridLineDashSequence
		};
		using var candidateAuxiliaryLinePaint = options.DrawCandidateAuxiliaryLines
			? new SKPaint
			{
				Style = SKPaintStyle.Stroke,
				StrokeWidth = options.CandidateAuxiliaryLineStrokeThickness,
				Color = options.CandidateAuxiliaryLineStrokeColor,
				IsAntialias = true,
				PathEffect = options.CandidateAuxiliaryLineDashSequence.IsEmpty ? null : options.CandidateAuxiliaryLineDashSequence
			}
			: null;
		for (var i = 0; i <= 27; i++)
		{
			if ((i % 9 == 0 ? blockLinePaint : i % 3 == 0 ? cellLinePaint : candidateAuxiliaryLinePaint) is not { } paintChosen)
			{
				continue;
			}

			Canvas.DrawLine(Mapper.GetCandidateAnchor(i, 0), Mapper.GetCandidateAnchor(i, 27), paintChosen);
			Canvas.DrawLine(Mapper.GetCandidateAnchor(0, i), Mapper.GetCandidateAnchor(27, i), paintChosen);
		}
	}

	/// <inheritdoc/>
	public void Export(string path, ImageExportingOptions? options = null)
	{
		options ??= new();

		var extension = Path.GetExtension(path);
		using var image = _surface.Snapshot();
		using var data = image.Encode(
			extension switch
			{
				".jpg" => SKEncodedImageFormat.Jpeg,
				".png" => SKEncodedImageFormat.Png,
				".gif" => SKEncodedImageFormat.Gif,
				".bmp" => SKEncodedImageFormat.Bmp,
				".webp" => SKEncodedImageFormat.Webp,
				_ => throw new NotSupportedException()
			},
			options.Quality
		);
		using var stream = new MemoryStream(data.ToArray());
		using var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);
		stream.CopyTo(fileStream);
	}
}
