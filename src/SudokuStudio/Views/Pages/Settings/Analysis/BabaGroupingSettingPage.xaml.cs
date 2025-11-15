namespace SudokuStudio.Views.Pages.Settings.Analysis;

/// <summary>
/// Represents baba grouping setting page.
/// </summary>
public sealed partial class BabaGroupingSettingPage : Page
{
	/// <summary>
	/// Initializes a <see cref="BabaGroupingSettingPage"/> instance.
	/// </summary>
	public BabaGroupingSettingPage() => InitializeComponent();


	private void InitialLetterComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var letter = (BabaGroupInitialLetter)((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag;
		Application.CurrentApp.Preference.AnalysisPreferences.InitialLetter = letter;
	}

	private void LetterCasingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		var flag = (bool)((SegmentedItem)((Segmented)sender).SelectedItem).Tag;
		Application.CurrentApp.Preference.AnalysisPreferences.LetterCasing = flag
			? BabaGroupLetterCase.Upper
			: BabaGroupLetterCase.Lower;
	}

	private void Page_Loaded(object sender, RoutedEventArgs e)
	{
		var analysisPref = Application.CurrentApp.Preference.AnalysisPreferences;
		InitialLetterComboBox.SelectedIndex = InitialLetterComboBox.Items
			.Select(valueSelector)
			.FirstOrDefault(p => p?.Control.Tag is BabaGroupInitialLetter letter && letter == analysisPref.InitialLetter)?
			.Index
			?? -1;
		LetterCasingComboBox.SelectedIndex = analysisPref.LetterCasing switch
		{
			BabaGroupLetterCase.Upper => 0,
			BabaGroupLetterCase.Lower => 1,
			_ => -1
		};


		static (ComboBoxItem Control, int Index)? valueSelector(object v, int i) => ((ComboBoxItem)v, i);
	}
}
