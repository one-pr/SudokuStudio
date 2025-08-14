namespace Windows.Storage.Pickers;

/// <summary>
/// Provides with extension methods on <see cref="FileSavePicker"/>.
/// </summary>
/// <seealso cref="FileSavePicker"/>
public static class FileSavePickerExtensions
{
	/// <summary>
	/// Provides extension memebers on <see cref="FileSavePicker"/>.
	/// </summary>
	extension(FileSavePicker @this)
	{
		internal void Initialize<TUIElement>(TUIElement control) where TUIElement : UIElement
		{
			var window = Application.Current.AsApp().WindowManager.GetWindowForElement(control);
			var hWnd = WindowNative.GetWindowHandle(window);
			InitializeWithWindow.Initialize(@this, hWnd);
		}
	}
}
