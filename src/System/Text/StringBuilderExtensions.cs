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
		/// <param name="elements">The elements to be appended.</param>
		/// <param name="stringConverter">The converter method.</param>
		/// <returns>The reference of the current instance.</returns>
		public StringBuilder AppendRange<T>(ReadOnlySpan<T> elements, Func<T, string>? stringConverter) where T : notnull
		{
			stringConverter ??= static value => value.ToString()!;
			foreach (ref readonly var element in elements)
			{
				@this.Append(stringConverter(element));
			}
			return @this;
		}
	}
}
