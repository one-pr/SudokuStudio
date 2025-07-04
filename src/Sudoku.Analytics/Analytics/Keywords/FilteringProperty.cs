namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Provides helper methods for retrieving attributes with custom inheritance rules.
/// </summary>
public static class FilteringProperty
{
	/// <summary>
	/// Represents default binding flags on property.
	/// </summary>
	private const BindingFlags PropertyBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;


	/// <summary>
	/// Determine whether the specified property name is a filtering property.
	/// </summary>
	/// <typeparam name="TStep">The type of step.</typeparam>
	/// <param name="propertyName">The property name.</param>
	/// <returns>A <see cref="bool"/> result indicating that.</returns>
	public static bool IsFilteringProperty<TStep>(string propertyName)
		=> typeof(TStep).GetProperty(propertyName) is { } propertyInfo
		&& GetAttribute<FilteringPropertyAttribute>(propertyInfo) is not null;

	/// <summary>
	/// Retrieves the attribute applied to a property according to custom inheritance rules.
	/// </summary>
	/// <typeparam name="TAttribute">The type of the attribute to retrieve.</typeparam>
	/// <param name="property">The property to inspect.</param>
	/// <returns>
	/// The attribute instance based on these rules:
	/// <list type="number">
	/// <item>For <see langword="override"/> properties: Returns base class attribute</item>
	/// <item>For <see langword="new"/> properties: Returns only if defined on current property</item>
	/// <item>For non-overridden properties: Returns base class attribute</item>
	/// </list>
	/// </returns>
	/// <exception cref="ArgumentNullException">Thrown when property is <see langword="null"/>.</exception>
	private static TAttribute? GetAttribute<TAttribute>(PropertyInfo property) where TAttribute : Attribute
	{
		ArgumentNullException.ThrowIfNull(property);

		// Check if the property uses 'new' keyword (hides base member).
		var isNewProperty = IsNewProperty(property);

		// Case 2: Property uses 'new' keyword.
		if (isNewProperty)
		{
			// Only inspect current property, ignore base classes.
			return property.GetCustomAttribute<TAttribute>(false);
		}

		// Case 1 & 3: Property uses override or has no override.
		return FindInBaseClasses<TAttribute>(property);
	}

	/// <summary>
	/// Determines if the property is declared with the <see langword="new"/> keyword.
	/// </summary>
	/// <param name="property">Property to check.</param>
	/// <returns>
	/// True if property hides base member with <see langword="new"/> keyword, False otherwise.
	/// </returns>
	private static bool IsNewProperty(PropertyInfo property)
	{
		// Get the property accessor (prefer get method if available).
		if ((property.GetGetMethod(true) ?? property.GetSetMethod(true)) is not { } accessor)
		{
			// Cannot determine without accessor.
			return false;
		}

		// Get the base definition of the accessor method.
		var baseDefinition = accessor.GetBaseDefinition();

		// Check if method is an override (declaring type differs from base definition).
		var isOverride = accessor.DeclaringType != baseDefinition.DeclaringType;

		// Conditions for 'new' property:
		// 1. Not an override method
		// 2. Base class has property with same name
		return !isOverride && baseDefinition.DeclaringType is not null
			&& baseDefinition.DeclaringType.GetProperty(property.Name, PropertyBindingFlags) is not null;
	}

	/// <summary>
	/// Finds the attribute in the property's inheritance hierarchy.
	/// </summary>
	/// <typeparam name="TAttribute">Attribute type to find.</typeparam>
	/// <param name="property">Starting property.</param>
	/// <returns>
	/// First found attribute in base classes, or <see langword="null"/> if not found.
	/// </returns>
	private static TAttribute? FindInBaseClasses<TAttribute>(PropertyInfo property) where TAttribute : Attribute
	{
		var currentType = property.DeclaringType;
		var propertyName = property.Name;

		// Traverse base classes upward.
		while (currentType is not null)
		{
			// Get property declared in current type.
			var currentProperty = currentType.GetProperty(propertyName, PropertyBindingFlags);
			if (currentProperty?.GetCustomAttribute<TAttribute>(false) is { } attribute)
			{
				return attribute;
			}

			// Move to base type.
			currentType = currentType.BaseType;
		}

		return null; // Attribute not found.
	}
}
