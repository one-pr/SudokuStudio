namespace Windows.Storage.Pickers;

/// <summary>
/// Provides with extension methods on <see cref="FileOpenPicker"/>.
/// </summary>
/// <seealso cref="FileOpenPicker"/>
public static class FileOpenPickerExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(FileOpenPicker @this)
	{
		internal void Initialize<TUIElement>(TUIElement control) where TUIElement : UIElement
		{
			var window = Application.CurrentApp.WindowManager.GetWindowForElement(control);
			var hWnd = WindowNative.GetWindowHandle(window);
			InitializeWithWindow.Initialize(@this, hWnd);
		}
	}
}
