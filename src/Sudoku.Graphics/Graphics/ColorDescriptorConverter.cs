namespace Sudoku.Graphics;

/// <summary>
/// Represents converter method on <see cref="ColorDescriptor"/> instance.
/// </summary>
/// <seealso cref="ColorDescriptor"/>
internal static class ColorDescriptorConverter
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(ColorDescriptor @this)
	{
		/// <summary>
		/// Returns target color to be drawn.
		/// </summary>
		/// <param name="options">The options.</param>
		/// <returns>The color.</returns>
		public SKColor GetTargetColor(CanvasDrawingOptions? options = null)
		{
			options ??= CanvasDrawingOptions.Default;

			switch (@this)
			{
				case (_, (byte a, byte r, byte g, byte b)):
				{
					return new(r, g, b, a);
				}
				case (_, int paletteId):
				{
					// TODO: Not implemented.
					throw new NotImplementedException();
				}
				case (_, ColorDescriptorAlias item):
				{
					// TODO: Not implemented.
					return item switch
					{
						ColorDescriptorAlias.Normal => throw new NotImplementedException(),
						ColorDescriptorAlias.Auxiliary1 => throw new NotImplementedException(),
						ColorDescriptorAlias.Auxiliary2 => throw new NotImplementedException(),
						ColorDescriptorAlias.Auxiliary3 => throw new NotImplementedException(),
						ColorDescriptorAlias.Assignment => throw new NotImplementedException(),
						ColorDescriptorAlias.OverlappedAssignment => throw new NotImplementedException(),
						ColorDescriptorAlias.Elimination => throw new NotImplementedException(),
						ColorDescriptorAlias.Cannibalism => throw new NotImplementedException(),
						ColorDescriptorAlias.Exofin => throw new NotImplementedException(),
						ColorDescriptorAlias.Endofin => throw new NotImplementedException(),
						ColorDescriptorAlias.Link => throw new NotImplementedException(),
						ColorDescriptorAlias.AlmostLockedSet1 => throw new NotImplementedException(),
						ColorDescriptorAlias.AlmostLockedSet2 => throw new NotImplementedException(),
						ColorDescriptorAlias.AlmostLockedSet3 => throw new NotImplementedException(),
						ColorDescriptorAlias.AlmostLockedSet4 => throw new NotImplementedException(),
						ColorDescriptorAlias.AlmostLockedSet5 => throw new NotImplementedException(),
						ColorDescriptorAlias.Rectangle1 => throw new NotImplementedException(),
						ColorDescriptorAlias.Rectangle2 => throw new NotImplementedException(),
						ColorDescriptorAlias.Rectangle3 => throw new NotImplementedException(),
						_ => throw new InvalidOperationException($"The value '{nameof(@this.AliasedItem)}' is invalid.")
					};
				}
				default:
				{
					throw new ArgumentOutOfRangeException(nameof(@this));
				}
			}
		}
	}
}
