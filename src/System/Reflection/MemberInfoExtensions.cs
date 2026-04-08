namespace System.Reflection;

/// <summary>
/// Provides with extension methods on <see cref="MemberInfo"/> instances.
/// </summary>
/// <seealso cref="MemberInfo"/>
public static class MemberInfoExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(MemberInfo @this)
	{
		/// <summary>
		/// Indicates whether the member is compiler-generated.
		/// </summary>
		public bool IsCompilerGenerated => @this.IsDefined(typeof(CompilerGeneratedAttribute), false);

		/// <summary>
		/// Indicates whether the member is marked <see cref="ExtensionAttribute"/> or not.
		/// </summary>
		/// <seealso cref="ExtensionAttribute"/>
		public bool MightBeExtension => @this.IsDefined(typeof(ExtensionAttribute), false);


		/// <inheritdoc cref="CustomAttributeExtensions.IsDefined(MemberInfo, Type)"/>
		public bool IsDefined<TAttribute>() where TAttribute : Attribute => @this.IsDefined(typeof(TAttribute));
	}
}
