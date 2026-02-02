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
			return @this switch
			{
				(_, (byte a, byte r, byte g, byte b)) => new(r, g, b, a),
				(_, int paletteId) => options.UserDefinedColorPalette[paletteId].AsSKColor(),
				(_, ColorDescriptorAlias item) => item switch
				{
					ColorDescriptorAlias.Normal => options.NormalColor.AsSKColor(),
					ColorDescriptorAlias.Auxiliary1 => options.AuxiliaryColors[0].AsSKColor(),
					ColorDescriptorAlias.Auxiliary2 => options.AuxiliaryColors[1].AsSKColor(),
					ColorDescriptorAlias.Auxiliary3 => options.AuxiliaryColors[2].AsSKColor(),
					ColorDescriptorAlias.Assignment => options.AssignmentColor.AsSKColor(),
					ColorDescriptorAlias.OverlappedAssignment => options.OverlappedAssignmentColor.AsSKColor(),
					ColorDescriptorAlias.Elimination => options.EliminationColor.AsSKColor(),
					ColorDescriptorAlias.Cannibalism => options.CannibalismColor.AsSKColor(),
					ColorDescriptorAlias.Exofin => options.ExofinColor.AsSKColor(),
					ColorDescriptorAlias.Endofin => options.EndofinColor.AsSKColor(),
					ColorDescriptorAlias.Link => options.LinkColor.AsSKColor(),
					ColorDescriptorAlias.AlmostLockedSet1 => options.AlmostLockedSetColors[0].AsSKColor(),
					ColorDescriptorAlias.AlmostLockedSet2 => options.AlmostLockedSetColors[1].AsSKColor(),
					ColorDescriptorAlias.AlmostLockedSet3 => options.AlmostLockedSetColors[2].AsSKColor(),
					ColorDescriptorAlias.AlmostLockedSet4 => options.AlmostLockedSetColors[3].AsSKColor(),
					ColorDescriptorAlias.AlmostLockedSet5 => options.AlmostLockedSetColors[4].AsSKColor(),
					ColorDescriptorAlias.Rectangle1 => options.RectangleColors[0].AsSKColor(),
					ColorDescriptorAlias.Rectangle2 => options.RectangleColors[1].AsSKColor(),
					ColorDescriptorAlias.Rectangle3 => options.RectangleColors[2].AsSKColor(),
					_ => throw new InvalidOperationException($"The value '{nameof(@this.AliasedItem)}' is invalid.")
				},
				_ => throw new ArgumentOutOfRangeException(nameof(@this))
			};
		}
	}
}
