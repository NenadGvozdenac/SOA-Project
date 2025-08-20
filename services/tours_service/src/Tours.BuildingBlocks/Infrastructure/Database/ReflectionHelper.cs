namespace tours_service.src.Tours.BuildingBlocks.Infrastructure.Database;
public static class ReflectionHelper
{
  public static bool IsPrimitive(Type type)
  {
    return type.IsPrimitive || type.IsValueType || type == typeof(string);
  }

  public static Type GetElementTypeIfGenericList(Type type)
  {
    // Check if the type is a generic list and get the element type if true
    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
    {
      return type.GetGenericArguments()[0];
    }
    return type;
  }

  public static bool ShouldSkipProperty(Type type)
  {
    // Get the element type if the type is a generic list
    var elementType = GetElementTypeIfGenericList(type);

    // Skip if the type or element type is assignable from ValueObject
    return IsPrimitive(elementType);
  }
}