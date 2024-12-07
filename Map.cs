using System.Collections.Generic;
using System.IO;

public class Map
{
	public List<MapEntry> Entries = new List<MapEntry>();

	public static Map Parse(string filePath)
	{
		using (var reader = new StreamReader(filePath))
			return Parse(reader);
	}

	public static Map Parse(TextReader reader)
	{
		Map map = new Map();

		while (true)
		{
			string? line = reader.ReadLine();

			if (line == null)
				break;

			if (line.EndsWith("-"))
				map.Entries.Add(MapEntry.Parse(line));
		}

		return map;
	}

	public MapEntry? Find(long offset, int length)
	{
		foreach (var entry in Entries)
			if (entry.Intersects(offset, length))
				return entry;

		return null;
	}

	public void SplitIfNecessary(MapEntry entry, long offset, int length)
	{
		if (offset >= entry.Offset + entry.Length)
			return;
		if (offset + length <= entry.Offset)
			return;

		if (offset > entry.Offset)
		{
			var newEntry = new MapEntry();

			newEntry.Offset = entry.Offset;
			newEntry.Length = (int)(offset - entry.Offset);
			newEntry.Found = entry.Found;

			Entries.Add(newEntry);

			entry.Offset = offset;
			entry.Length -= newEntry.Length;
		}

		if (offset + length < entry.Length)
		{
			var newEntry = new MapEntry();

			newEntry.Offset = offset + length;
			newEntry.Length = (int)(entry.Length - length);
			newEntry.Found = entry.Found;

			Entries.Add(newEntry);

			entry.Length = length;
		}
	}
}
