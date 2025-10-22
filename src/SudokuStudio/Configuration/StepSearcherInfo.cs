namespace SudokuStudio.Configuration;

/// <summary>
/// Defines a serialization data of a step searcher.
/// </summary>
public sealed partial class StepSearcherInfo : DependencyObject
{
	/// <summary>
	/// Indicates whether the technique option is not fixed and can be used for drag-and-drop operation.
	/// </summary>
	[JsonIgnore]
	public bool CanDrag => !CreateStepSearcher().Metadata.IsOrderingFixed;

	/// <summary>
	/// Indicates whether the technique option is not read-only and can be used for toggle operation.
	/// </summary>
	[JsonIgnore]
	public bool CanToggle => !CreateStepSearcher().Metadata.IsReadOnly;


	/// <summary>
	/// Indicates whether the step searcher is enabled.
	/// </summary>
	[DependencyProperty(DefaultValue = true)]
	public partial bool IsEnabled { get; set; }

	/// <summary>
	/// Indicates the name of the step searcher.
	/// </summary>
	[JsonIgnore]
	public string Name => CreateStepSearcher().Metadata.GetName(App.CurrentCulture);

	/// <summary>
	/// Indicates the type name of the step searcher.
	/// This property can be used for creating instances via reflection using getMetaProperties <see cref="Activator.CreateInstance(Type)"/>.
	/// </summary>
	/// <seealso cref="Activator.CreateInstance(Type)"/>
	[DependencyProperty]
	public partial string TypeName { get; set; }


	/// <inheritdoc/>
	public override string ToString()
		=> $$"""{{nameof(StepSearcherInfo)}} { {{nameof(IsEnabled)}} = {{IsEnabled}}, {{nameof(Name)}} = {{Name}}, {{nameof(TypeName)}} = {{TypeName}} }""";

	/// <summary>
	/// Creates a list of <see cref="StepSearcher"/> instances.
	/// </summary>
	/// <returns>A list of <see cref="StepSearcher"/> instances.</returns>
	public StepSearcher CreateStepSearcher() => StepSearcher.GetStepSearcher(TypeName);
}
