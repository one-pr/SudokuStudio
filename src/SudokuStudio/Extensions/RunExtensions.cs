namespace Microsoft.UI.Xaml.Documents;

/// <summary>
/// Provides with extension methods on <see cref="Run"/>.
/// </summary>
/// <seealso cref="Run"/>
public static class RunExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Run @this)
	{
		/// <summary>
		/// Creates a <see cref="Bold"/> instance with a singleton value of <see cref="Run"/>.
		/// </summary>
		/// <returns>A <see cref="Bold"/> instance.</returns>
		public TSpan SingletonSpan<TSpan>() where TSpan : Span, new()
		{
			var result = new TSpan();
			result.Inlines.Add(@this);
			return result;
		}
	}
}
