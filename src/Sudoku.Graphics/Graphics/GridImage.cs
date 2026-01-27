namespace Sudoku.Graphics;

/// <summary>
/// Represents an image to a sudoku grid.
/// </summary>
/// <param name="grid"><inheritdoc cref="Grid" path="/summary"/></param>
/// <param name="mapper"><inheritdoc cref="Mapper" path="/summary"/></param>
public sealed class GridImage(in Grid grid, PointMapper mapper) :
	IDisposable,
	IGridImageDrawBackground,
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
	public void Export(string path, ImageExportOptions? options = null)
	{
		options ??= ImageExportOptions.Default;

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
