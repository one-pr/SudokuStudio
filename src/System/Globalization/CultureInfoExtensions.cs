namespace System.Globalization;

/// <summary>
/// Provides extension methods on <see cref="CultureInfo"/>.
/// </summary>
/// <seealso cref="CultureInfo"/>
public static class CultureInfoExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="CultureInfo"/>.
	/// </summary>
	/// <param name="this">The instance.</param>
	extension(CultureInfo @this)
	{
		/// <summary>
		/// Indicates whether the culture is Chinese.
		/// </summary>
		public bool IsChinese => @this.Name.StartsWith(SR.ChineseLanguage, StringComparison.OrdinalIgnoreCase);

		/// <summary>
		/// Indicates whether the culture is English.
		/// </summary>
		public bool IsEnglish => @this.Name.StartsWith(SR.EnglishLanguage, StringComparison.OrdinalIgnoreCase);
	}
}
