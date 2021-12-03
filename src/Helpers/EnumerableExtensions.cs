namespace AdventOfCode;

internal static class EnumerableExtensions
{
	public static T[] AsArray<T>(this IEnumerable<T> source)
	{
		return source is T[] array ? array : source.ToArray();
	}
}
