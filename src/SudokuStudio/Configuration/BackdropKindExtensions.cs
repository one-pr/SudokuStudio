#if !CUSTOMIZED_BACKDROP
using WinUICommunity;

namespace SudokuStudio.Configuration;

/// <summary>
/// Provides with extension methods on <see cref="BackdropKind"/>.
/// </summary>
/// <seealso cref="BackdropKind"/>
public static class BackdropKindExtensions
{
	/// <summary>
	/// Provides extension members on <see cref="BackdropKind"/>.
	/// </summary>
	extension(BackdropKind @this)
	{
		/// <summary>
		/// Try to get target <see cref="SystemBackdrop"/> instance.
		/// </summary>
		/// <returns>The target <see cref="SystemBackdrop"/> instance.</returns>
		/// <exception cref="NotSupportedException">Throws when the value is out of range.</exception>
		public SystemBackdrop? GetBackdrop()
			=> @this switch
			{
				BackdropKind.Default => null,
				BackdropKind.Mica => new MicaBackdrop(),
				BackdropKind.MicaDeep => new MicaBackdrop { Kind = MicaKind.BaseAlt },
				BackdropKind.Acrylic => new DesktopAcrylicBackdrop(),
				BackdropKind.AcrylicThin => new AcrylicSystemBackdrop(DesktopAcrylicKind.Thin),
				BackdropKind.Transparent => new TransparentBackdrop(),
				_ => throw new NotSupportedException()
			};
	}
}
#endif
