namespace Windows.Storage.Pickers;

/// <summary>
/// Provides with extension methods on <see cref="FileOpenPicker"/>.
/// </summary>
/// <seealso cref="FileOpenPicker"/>
public static class FileOpenPickerExtensions
{
	/// <summary>
	/// Provides extension memebers on <see cref="FileOpenPicker"/>.
	/// </summary>
	extension(FileOpenPicker @this)
	{
		internal void Initialize<TUIElement>(TUIElement control) where TUIElement : UIElement
		{
			var window = Application.Current.AsApp().WindowManager.GetWindowForElement(control);
			var hWnd = WindowNative.GetWindowHandle(window);
			InitializeWithWindow.Initialize(@this, hWnd);
		}
	}
}
