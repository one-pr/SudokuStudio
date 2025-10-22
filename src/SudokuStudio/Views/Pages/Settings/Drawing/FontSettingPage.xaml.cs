namespace SudokuStudio.Views.Pages.Settings.Drawing;

/// <summary>
/// Represents font setting page.
/// </summary>
public sealed partial class FontSettingPage : Page
{
	/// <summary>
	/// Initializes a <see cref="FontSettingPage"/> instance.
	/// </summary>
	public FontSettingPage() => InitializeComponent();


	/// <summary>
	/// The given color.
	/// </summary>
	internal Color GivenFontColor
	{
		get => App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.GivenFontColor,
			_ => Application.CurrentApp.Preference.UIPreferences.GivenFontColor_Dark
		};

		set => _ = App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.GivenFontColor = value,
			_ => Application.CurrentApp.Preference.UIPreferences.GivenFontColor_Dark = value
		};
	}

	/// <summary>
	/// The modifiable color.
	/// </summary>
	internal Color ModifiableFontColor
	{
		get => App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.ModifiableFontColor,
			_ => Application.CurrentApp.Preference.UIPreferences.ModifiableFontColor_Dark
		};

		set => _ = App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.ModifiableFontColor = value,
			_ => Application.CurrentApp.Preference.UIPreferences.ModifiableFontColor_Dark = value
		};
	}

	/// <summary>
	/// The pencilmark color.
	/// </summary>
	internal Color PencilmarkFontColor
	{
		get => App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.PencilmarkFontColor,
			_ => Application.CurrentApp.Preference.UIPreferences.PencilmarkFontColor_Dark
		};

		set => _ = App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.PencilmarkFontColor = value,
			_ => Application.CurrentApp.Preference.UIPreferences.PencilmarkFontColor_Dark = value
		};
	}

	/// <summary>
	/// The coordinate label color.
	/// </summary>
	internal Color CoordinateLabelFontColor
	{
		get => App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontColor,
			_ => Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontColor_Dark
		};

		set => _ = App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontColor = value,
			_ => Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontColor_Dark = value
		};
	}

	/// <summary>
	/// The baba grouping label color.
	/// </summary>
	internal Color BabaGroupingFontColor
	{
		get => App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontColor,
			_ => Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontColor_Dark
		};

		set => _ = App.CurrentTheme switch
		{
			ApplicationTheme.Light => Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontColor = value,
			_ => Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontColor_Dark = value
		};
	}


	private void GivenFontPicker_SelectedFontChanged(object sender, string e)
		=> Application.CurrentApp.Preference.UIPreferences.GivenFontName = e;

	private void GivenFontPicker_SelectedFontScaleChanged(object sender, decimal e)
		=> Application.CurrentApp.Preference.UIPreferences.GivenFontScale = e;

	private void GivenFontPicker_SelectedFontColorChanged(object sender, Color e)
		=> Application.CurrentApp.Preference.UIPreferences.GivenFontColor = e;

	private void ModifiableFontPicker_SelectedFontChanged(object sender, string e)
		=> Application.CurrentApp.Preference.UIPreferences.ModifiableFontName = e;

	private void ModifiableFontPicker_SelectedFontScaleChanged(object sender, decimal e)
		=> Application.CurrentApp.Preference.UIPreferences.ModifiableFontScale = e;

	private void ModifiableFontPicker_SelectedFontColorChanged(object sender, Color e)
		=> Application.CurrentApp.Preference.UIPreferences.ModifiableFontColor = e;

	private void PencilmarkFontPicker_SelectedFontChanged(object sender, string e)
		=> Application.CurrentApp.Preference.UIPreferences.PencilmarkFontName = e;

	private void PencilmarkFontPicker_SelectedFontScaleChanged(object sender, decimal e)
		=> Application.CurrentApp.Preference.UIPreferences.PencilmarkFontScale = e;

	private void PencilmarkFontPicker_SelectedFontColorChanged(object sender, Color e)
		=> Application.CurrentApp.Preference.UIPreferences.PencilmarkFontColor = e;

	private void CoordinateFontPicker_SelectedFontChanged(object sender, string e)
		=> Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontName = e;

	private void CoordinateFontPicker_SelectedFontScaleChanged(object sender, decimal e)
		=> Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontScale = e;

	private void CoordinateFontPicker_SelectedFontColorChanged(object sender, Color e)
		=> Application.CurrentApp.Preference.UIPreferences.CoordinateLabelFontColor = e;

	private void BabaGroupingFontPicker_SelectedFontChanged(object sender, string e)
		=> Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontName = e;

	private void BabaGroupingFontPicker_SelectedFontScaleChanged(object sender, decimal e)
		=> Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontScale = e;

	private void BabaGroupingFontPicker_SelectedFontColorChanged(object sender, Color e)
		=> Application.CurrentApp.Preference.UIPreferences.BabaGroupingFontColor = e;
}
