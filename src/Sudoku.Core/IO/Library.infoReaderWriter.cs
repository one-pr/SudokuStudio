namespace Sudoku.IO;

public partial class Library
{
	/// <summary>
	/// Writes the name to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteName(string value) => WriteProperty(static (info, value) => info.Name = value, value);

	/// <summary>
	/// Writes the description to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteDescription(string value) => WriteProperty(static (info, value) => info.Description = value, value);

	/// <summary>
	/// Writes the author to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteAuthor(string value) => WriteProperty(static (info, value) => info.Author = value, value);

	/// <summary>
	/// Writes the tags to the library information file.
	/// </summary>
	/// <param name="value">The value to be set.</param>
	public void WriteTags(ReadOnlySpan<string> value) => WriteProperty(static (info, value) => info.Tags = value.ToArray(), value);

	/// <summary>
	/// Reads the name of the library information file.
	/// </summary>
	/// <returns>The name.</returns>
	public string ReadName() => ReadProperty(static info => info.Name);

	/// <summary>
	/// Reads the description of the library information file.
	/// </summary>
	/// <returns>The description.</returns>
	public string ReadDescription() => ReadProperty(static info => info.Description);

	/// <summary>
	/// Reads the author of the library information file.
	/// </summary>
	/// <returns>The author.</returns>
	public string ReadAuthor() => ReadProperty(static info => info.Author);

	/// <summary>
	/// Reads the tags of the library information file.
	/// </summary>
	/// <returns>The tags.</returns>
	public ReadOnlySpan<string> ReadTags() => ReadProperty(static info => info.Tags);

	/// <summary>
	/// Writes the property of type <typeparamref name="T"/> to the file.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="valueAssignment">The result value assigning method.</param>
	/// <param name="value">The value to be set.</param>
	private void WriteProperty<T>(Action<LibraryInfo, T> valueAssignment, T value) where T : allows ref struct
	{
		var info = LoadOrCreate();
		valueAssignment(info, value);
		Save(info);
	}

	/// <summary>
	/// Reads the property of value of type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of value.</typeparam>
	/// <param name="resultValueCreator">The result value creator.</param>
	/// <returns>The result value.</returns>
	private T ReadProperty<T>(Func<LibraryInfo, T> resultValueCreator) where T : allows ref struct
	{
		var info = LoadOrCreate();
		return resultValueCreator(info);
	}
}
