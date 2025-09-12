namespace System.Text;

/// <summary>
/// Provides extension methods on <see cref="StringBuilder"/>.
/// </summary>
/// <seealso cref="StringBuilder"/>
public static class StringBuilderExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="StringBuilder"/>.
	/// </summary>
	extension(StringBuilder @this)
	{
		/// <summary>
		/// Removes the specified number of characters from end, from the current <see cref="StringBuilder"/> instance.
		/// </summary>
		/// <param name="count">The desired number of characters to remove.</param>
		/// <returns>The reference of the current instance.</returns>
		public StringBuilder RemoveFromEnd(int count)
		{
			var index = ^count;
			return @this.Remove(index.GetOffset(@this.Length), index.Value);
		}

		/// <summary>
		/// Appends a list of elements of type <typeparamref name="T"/> into the <see cref="StringBuilder"/> instance,
		/// using the specified converter to convert each element into <see cref="string"/> value.
		/// </summary>
		/// <typeparam name="T">The type of each element.</typeparam>
		/// <param name="stringConverter">The converter method.</param>
		/// <param name="appender">
		/// The append method for the builder instance, telling the handler which appending operation will be handled.
		/// By default, the method is equivalent to lambda:
		/// <code>
		/// <see langword="static"/> (<see cref="StringBuilder"/> sb, <typeparamref name="T"/> v) => sb.Append(v)
		/// </code>
		/// </param>
		/// <param name="elements">
		/// <para>The elements to be appended.</para>
		/// <include file="../../global-doc-comments.xml" path="//g/csharp12/feature[@name='params-collections']/target[@name='parameter']"/>
		/// </param>
		/// <returns>The reference of the current instance.</returns>
		public StringBuilder AppendRange<T>(
			Func<T, string> stringConverter,
			Func<StringBuilder, string, StringBuilder>? appender = null,
			params ReadOnlySpan<T> elements
		) where T : notnull
		{
			appender ??= static (sb, v) => sb.Append(v);
			foreach (ref readonly var element in elements)
			{
				appender(@this, stringConverter(element));
			}
			return @this;
		}
	}
}
