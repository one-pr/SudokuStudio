namespace Sudoku.Linq;

/// <summary>
/// Provides extension methods on <see cref="Step"/> sequence.
/// </summary>
public static class StepCollectionExtensions
{
	/// <summary>
	/// Provides with extension members on <see cref="ReadOnlySpan{T}"/> of <see cref="Step"/>.
	/// </summary>
	/// <param name="this">The instance.</param>
	extension(ReadOnlySpan<Step> @this)
	{
		/// <inheritdoc cref="ICastMethod{TSelf, TSource}.Cast{TResult}"/>
		public ReadOnlySpan<TStep> Cast<TStep>() where TStep : Step
		{
			var result = new TStep[@this.Length];
			for (var i = 0; i < @this.Length; i++)
			{
				result[i] = (TStep)@this[i];
			}
			return result;
		}
	}
}
