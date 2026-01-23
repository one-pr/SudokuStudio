namespace Windows.Foundation;

/// <summary>
/// Provides with extension methods on <see cref="Point"/>.
/// </summary>
/// <seealso cref="Point"/>
public static class PointExtensions
{
	/// <include
	///     file="../../global-doc-comments.xml"
	///     path="/g/csharp14/feature[@name='extension-container']/target[@name='container']"/>
	/// <param name="this">The current instance.</param>
	extension(Point @this)
	{
		/// <include file="../../global-doc-comments.xml" path="g/csharp7/feature[@name='deconstruction-method']/target[@name='method']"/>
		public void Deconstruct(out double x, out double y) => (x, y) = (@this.X, @this.Y);

		/// <summary>
		/// Gets the distance between the two points, starting with the current point, and ending with the specified point.
		/// </summary>
		/// <param name="other">The other point.</param>
		/// <returns>The distance of the two points.</returns>
		public double DistanceTo(Point other)
			=> Sqrt((@this.X - other.X) * (@this.X - other.X) + (@this.Y - other.Y) * (@this.Y - other.Y));
	}
}
