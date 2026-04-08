namespace System.Reflection;

/// <summary>
/// Provides extension members on <see cref="MethodInfo"/> instances.
/// </summary>
/// <seealso cref="MethodInfo"/>
public static class MethodInfoExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(MethodInfo @this)
	{
		/// <summary>
		/// Indicates the parameters defined in this method.
		/// </summary>
		public ReadOnlySpan<ParameterInfo> Parameters => @this.GetParameters();
	}
}
