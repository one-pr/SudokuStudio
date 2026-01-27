namespace Sudoku.Graphics.Primitives;

/// <summary>
/// Represents export method set.
/// </summary>
internal interface IGridImageExport
{
	/// <summary>
	/// Export image into target file.
	/// </summary>
	/// <param name="path">The file path.</param>
	/// <param name="options">The exporting options.</param>
	/// <exception cref="NotSupportedException">Throws when the target file extension is not supported.</exception>
	void Export(string path, ImageExportOptions? options = null);
}
