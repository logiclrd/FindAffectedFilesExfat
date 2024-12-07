using System.Collections.Generic;

class Report
{
	public string Path;
	public long Length;
	public List<MapEntry> MissingRegions = new List<MapEntry>();

	public Report(string path)
	{
		Path = path;
	}
}
