namespace Sudoku.Analytics.Keywords;

/// <summary>
/// Represents an attribute type that describes the property is marked as special property to be filtered in reflection.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false)]
public sealed class FilteringPropertyAttribute : Attribute;
