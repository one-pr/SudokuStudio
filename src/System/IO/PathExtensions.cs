namespace System.IO;

/// <summary>
/// Provides extension methods on <see cref="Path"/>.
/// </summary>
/// <seealso cref="Path"/>
public static class PathExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Path"/>.
	/// </summary>
	extension(Path)
	{
		/// <summary>
		/// Returns file name (without extension), and its extension.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <returns>A pair of strings - file name and its extension.</returns>
		public static (string FileName, string Extension) GetFileNameAndExtension(string path)
		{
			var lastPeriod = path.LastIndexOf('.');
			return (Path.GetFileName(path[..lastPeriod]), path[lastPeriod..]);
		}
	}
}
