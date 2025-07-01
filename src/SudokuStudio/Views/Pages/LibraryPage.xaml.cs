namespace SudokuStudio.Views.Pages;

/// <summary>
/// Represents library page.
/// </summary>
public sealed partial class LibraryPage : Page
{
	/// <summary>
	/// Initializes a <see cref="LibraryPage"/> instance.
	/// </summary>
	public LibraryPage() => InitializeComponent();


	private async void AddOnePuzzleItem_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: MenuFlyout { Target: GridViewItem { Content: LibraryBindableSource { Library: var lib } } } })
		{
			return;
		}

		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("LibraryPage_AddOnePuzzleDialogTitle", App.CurrentCulture),
			DefaultButton = ContentDialogButton.Primary,
			IsPrimaryButtonEnabled = true,
			PrimaryButtonText = SR.Get("LibraryPage_AddOnePuzzleDialogSure", App.CurrentCulture),
			CloseButtonText = SR.Get("LibraryPage_AddOnePuzzleDialogCancel", App.CurrentCulture),
			Content = new AddOnePuzzleDialogContent()
		};
		if (await dialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}

		await lib.WriteLineAsync(((AddOnePuzzleDialogContent)dialog.Content).TextCodeInput.Text);
	}

	private async void AddListItem_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem
			{
				Tag: MenuFlyout
				{
					Target: GridViewItem
					{
						Content: LibraryBindableSource
						{
							Library: var lib
						} instance
					}
				}
			})
		{
			return;
		}

		var fop = new FileOpenPicker();
		fop.Initialize(this);
		fop.ViewMode = PickerViewMode.Thumbnail;
		fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fop.AddFileFormat(FileFormats.Text);
		fop.AddFileFormat(FileFormats.PlainText);

		if (await fop.PickSingleFileAsync() is not { Path: var filePath })
		{
			return;
		}

		instance.IsActive = true;

		await lib.WriteLinesAsync(File.ReadLinesAsync(filePath));

		instance.IsActive = false;
	}

	private async void RemoveDuplicatePuzzlesItem_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem
			{
				Tag: MenuFlyout
				{
					Target: GridViewItem
					{
						Content: LibraryBindableSource
						{
							Library: var lib
						} instance
					}
				}
			})
		{
			return;
		}

		instance.IsActive = true;

		await lib.DeduplicateAsync();

		instance.IsActive = false;
	}

#if false
	private void VisitItem_Click(object sender, RoutedEventArgs e)
	{
	}
#endif

	private void LibrariesDisplayer_ItemClick(object sender, ItemClickEventArgs e)
	{
		if (sender is not GridView { ItemsPanelRoot.Children: var children })
		{
			return;
		}

		foreach (var child in children)
		{
			if (child is not GridViewItem { Content: LibraryBindableSource source, ContextFlyout: var flyout })
			{
				continue;
			}

			if (!ReferenceEquals(source, (LibraryBindableSource)e.ClickedItem))
			{
				continue;
			}

			flyout.ShowAt(child, new() { Placement = FlyoutPlacementMode.Auto });
			break;
		}
	}

	private void ClearPuzzlesItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: MenuFlyout { Target: GridViewItem { Content: LibraryBindableSource { Library: var lib } } } })
		{
			return;
		}

		lib.Clear();
	}

	private void DeleteLibraryItem_Click(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: MenuFlyout { Target: GridViewItem { Content: LibraryBindableSource { Library: var lib } } } })
		{
			return;
		}

		lib.Delete();

		var p = (ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource;
		for (var i = 0; i < p.Count; i++)
		{
			var libraryBindableSource = p[i];
			if (libraryBindableSource.Library == lib)
			{
				((ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource).RemoveAt(i);
				return;
			}
		}
	}

	private async void PropertiesItem_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: MenuFlyout { Target: GridViewItem { Content: LibraryBindableSource { Library: var lib } } } })
		{
			return;
		}

		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("LibraryPage_LibraryPropertiesDialogTitle", App.CurrentCulture),
			DefaultButton = ContentDialogButton.Close,
			IsPrimaryButtonEnabled = false,
			CloseButtonText = SR.Get("LibraryPage_LibraryPropertiesDialogClose", App.CurrentCulture),
			Content = new LibraryPropertiesDialogContent
			{
				Library = lib,
				LibraryName = lib.ReadName() is var name and not "" ? name : LibraryBindableSource.NameDefaultValue,
				LibraryAuthor = lib.ReadAuthor() is var author and not "" ? author : LibraryBindableSource.AuthorDefaultValue,
				LibraryDescription = lib.ReadDescription() is var description and not ""
					? description
					: LibraryBindableSource.DescriptionDefaultValue,
				LibraryLastModifiedTime = lib.LastModifiedTime
			}
		};
		await dialog.ShowAsync();
	}

	private async void ModifyPropertiesItem_ClickAsync(object sender, RoutedEventArgs e)
	{
		if (sender is not MenuFlyoutItem { Tag: MenuFlyout { Target: GridViewItem { Content: LibraryBindableSource { Library: var lib } } } })
		{
			return;
		}

		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("LibraryPage_ModifyPropertiesDialogTitle", App.CurrentCulture),
			DefaultButton = ContentDialogButton.Primary,
			IsPrimaryButtonEnabled = true,
			PrimaryButtonText = SR.Get("LibraryPage_ModifyPropertiesDialogSure", App.CurrentCulture),
			CloseButtonText = SR.Get("LibraryPage_ModifyPropertiesDialogCancel", App.CurrentCulture),
			Content = new LibraryModifyPropertiesDialogContent
			{
				LibraryName = lib.ReadName(),
				LibraryAuthor = lib.ReadAuthor(),
				LibraryDescription = lib.ReadDescription(),
				LibraryTags = [.. lib.ReadTags()]
			}
		};
		if (await dialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}

		// Replace with the original dictionary set to refresh UI.
		var content = (LibraryModifyPropertiesDialogContent)dialog.Content;
		var fileId = io::Path.GetFileNameWithoutExtension(lib.LibraryPath);
		var finalName = content.LibraryName is var name and not (null or "")
			? name
			: LibraryBindableSource.NameDefaultValue;
		var finalAuthor = content.LibraryAuthor is var author and not (null or "")
			? author
			: LibraryBindableSource.AuthorDefaultValue;
		var finalDescription = content.LibraryDescription is var description and not (null or "")
			? description
			: LibraryBindableSource.DescriptionDefaultValue;
		var finalTags = content.LibraryTags is { Count: not 0 } tags ? tags.ToArray() : [];
		lib.WriteName(finalName);
		lib.WriteAuthor(finalAuthor);
		lib.WriteDescription(finalDescription);
		lib.WriteTags(finalTags);

		var newInstance = new LibraryBindableSource
		{
			FileId = fileId,
			Name = finalName,
			Author = finalAuthor,
			Description = finalDescription,
			Tags = finalTags
		};

		var p = (ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource;
		for (var i = 0; i < p.Count; i++)
		{
			var libraryBindableSource = p[i];
			if (libraryBindableSource.Library == lib)
			{
				((ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource)[i] = newInstance;
				return;
			}
		}
	}

	private async void AddLibraryButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var dialog = new ContentDialog
		{
			XamlRoot = XamlRoot,
			Title = SR.Get("LibraryPage_AddLibraryDialogTitle", App.CurrentCulture),
			DefaultButton = ContentDialogButton.Primary,
			IsPrimaryButtonEnabled = true,
			PrimaryButtonText = SR.Get("LibraryPage_AddLibraryDialogSure", App.CurrentCulture),
			CloseButtonText = SR.Get("LibraryPage_AddLibraryDialogCancel", App.CurrentCulture),
			Content = new AddLibraryDialogContent()
		};
		if (await dialog.ShowAsync() != ContentDialogResult.Primary)
		{
			return;
		}

		// Update UI.
		var content = (AddLibraryDialogContent)dialog.Content;
		if (!content.IsNameValidAsFileId)
		{
			return;
		}

		var libraryCreated = new Library(CommonPaths.Library, content.FileId);
		var finalName = content.LibraryName is var name and not (null or "")
			? name
			: LibraryBindableSource.NameDefaultValue;
		var finalAuthor = content.LibraryAuthor is var author and not (null or "")
			? author
			: LibraryBindableSource.AuthorDefaultValue;
		var finalDescription = content.LibraryDescription is var description and not (null or "")
			? description
			: LibraryBindableSource.DescriptionDefaultValue;
		var finalTags = content.LibraryTags is { Count: not 0 } tags ? tags.ToArray() : [];
		libraryCreated.WriteName(finalName);
		libraryCreated.WriteAuthor(finalAuthor);
		libraryCreated.WriteDescription(finalDescription);
		libraryCreated.WriteTags(finalTags);


		((ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource).Add(
			new()
			{
				FileId = content.FileId,
				Name = finalName,
				Author = finalAuthor,
				Description = finalDescription,
				Tags = finalTags
			}
		);
	}

	private async void LoadLibraryFileButton_ClickAsync(object sender, RoutedEventArgs e)
	{
		var fop = new FileOpenPicker();
		fop.Initialize(this);
		fop.ViewMode = PickerViewMode.Thumbnail;
		fop.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
		fop.AddFileFormat(FileFormats.Text);
		fop.AddFileFormat(FileFormats.PlainText);

		if (await fop.PickSingleFileAsync() is not { Path: var filePath })
		{
			return;
		}

		var fileName = io::Path.GetFileNameWithoutExtension(filePath);
		var lib = new Library(CommonPaths.Library, fileName);
		lib.WriteName(fileName);

		var source = new LibraryBindableSource { IsActive = true, FileId = fileName };
		((ObservableCollection<LibraryBindableSource>)LibrariesDisplayer.ItemsSource).Add(source);

		await lib.WriteLinesAsync(File.ReadLinesAsync(filePath));

		source.IsActive = false;
	}
}
