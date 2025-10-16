namespace Sudoku.IO;

/// <summary>
/// Provides with extension methods on <see cref="Library"/>.
/// </summary>
/// <seealso cref="Library"/>
public static class LibraryTransformationExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="Library"/>.
	/// </summary>
	extension(Library @this)
	{
		/// <summary>
		/// Randomly read one puzzle in the specified file, and return it.
		/// </summary>
		/// <param name="transformTypes">Indicates the available transform type that the chosen grid can be transformed.</param>
		/// <param name="cancellationToken">The cancellation token that can cancel the current asynchronous operation.</param>
		/// <returns>A <see cref="Task{TResult}"/> of <see cref="Grid"/> instance as the result.</returns>
		/// <exception cref="InvalidOperationException">Throw when the library file is not initialized.</exception>
		public async Task<Grid> RandomReadOneAsync(TransformType transformTypes = TransformType.None, CancellationToken cancellationToken = default)
		{
			var chosen = Grid.Parse(await @this.SelectOneAsync(cancellationToken));
			chosen.Transform(transformTypes);
			return chosen;
		}
	}
}
