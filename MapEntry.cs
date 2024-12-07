using System;
using System.Globalization;

public class MapEntry
{
	public long Offset;
	public int Length;
	public bool Found;

	public long LocalOffset;
	public int LocalLength;

	public static MapEntry Parse(string line)
	{
		MapEntry entry = new MapEntry();

		string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

		entry.Offset = long.Parse(parts[0].Replace("0x", ""), NumberStyles.AllowHexSpecifier);
		entry.Length = int.Parse(parts[1].Replace("0x", ""), NumberStyles.AllowHexSpecifier);

		return entry;
	}

	public bool Intersects(long offset, int length)
	{
		if (offset + length <= this.Offset)
			return false;
		if (offset >= this.Offset + this.Length)
			return false;

		return true;
	}
}
