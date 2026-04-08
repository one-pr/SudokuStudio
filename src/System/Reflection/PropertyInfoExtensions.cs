namespace System.Reflection;

/// <summary>
/// Provides extension members on <see cref="PropertyInfo"/> instances.
/// </summary>
/// <seealso cref="PropertyInfo"/>
public static class PropertyInfoExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(PropertyInfo @this)
	{
		/// <summary>
		/// Indicates whether the property is modified by keyword <see langword="static"/> or not.
		/// </summary>
		public bool IsStatic => (@this.GetMethod?.IsStatic ?? false) || (@this.SetMethod?.IsStatic ?? false);

		/// <summary>
		/// Indicates whether the property is modified by keyword <see langword="readonly"/> or not.
		/// </summary>
		public bool IsReadOnly
			=> (@this.DeclaringType?.IsDefined(typeof(IsReadOnlyAttribute)) ?? false)
			|| (@this.GetMethod?.IsDefined(typeof(IsReadOnlyAttribute)) ?? false)
			|| (@this.SetMethod?.IsDefined(typeof(IsReadOnlyAttribute)) ?? false);

		/// <summary>
		/// Indicates the parameters defined in this indexer property.
		/// If the property is not an indexer, an empty array will be returned.
		/// </summary>
		public ReadOnlySpan<ParameterInfo> IndexParameters => @this.GetIndexParameters();
	}
}
