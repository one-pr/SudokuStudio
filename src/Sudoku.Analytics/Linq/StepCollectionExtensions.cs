namespace Sudoku.Linq;

/// <summary>
/// Provides extension methods on <see cref="Step"/> sequence.
/// </summary>
public static class StepCollectionExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <typeparam name="TStep">The type of step to be casted.</typeparam>
	/// <param name="this">The current instance.</param>
	extension<TStep>(ReadOnlySpan<Step> @this) where TStep : Step
	{
		/// <inheritdoc cref="ICastMethod{TSelf, TSource}.Cast{TResult}"/>
		public ReadOnlySpan<TStep> Cast()
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
