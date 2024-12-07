using System;
using System.IO;

using ExFat;
using ExFat.IO;
using ExFat.Partition;
using ExFat.Partition.Entries;

class Program
{
	static int Main(string[] args)
	{
		if (args.Length < 2)
		{
			Console.WriteLine("usage: FindAffectedFilesExfat <device or image> <map file>");
			return 1;
		}

		string imageFile = args[0];
		string mapFile = args[1];

		using (var image = File.OpenRead(imageFile))
		using (var partition = new ExFatPartition(image))
		{
			var map = Map.Parse(mapFile);

			RecurseFindMapEntries(partition, partition.RootDirectoryDataDescriptor, "", map);

			bool notFoundHeading = false;

			foreach (var entry in map.Entries)
			{
				if (!entry.Found)
				{
					if (!notFoundHeading)
					{
						Console.WriteLine("The following regions were not found linked to any file:");
						notFoundHeading = true;
					}

					Console.WriteLine("- 0x{0:X12}: {1} bytes", entry.Offset, entry.Length);
				}
			}
		}

		return 0;
	}

	static void RecurseFindMapEntries(ExFatPartition partition, DataDescriptor directoryDescriptor, string path, Map map)
	{
		var entries = partition.GetMetaEntries(directoryDescriptor);

		foreach (var entry in entries)
		{
			if (entry.Primary is FileExFatDirectoryEntry fileEntry)
			{
				string subpath = path + "/" + entry.ExtensionsFileName;

				var report = EnumerateClustersAndCompareWithMap(partition, entry, subpath, map);

				if (report != null)
				{
					Console.WriteLine(report.Path);

					foreach (var region in report.MissingRegions)
						Console.WriteLine("* {0} bytes at offset {1} (0x{1:X12})", region.LocalLength, region.LocalOffset);
					
					Console.WriteLine();
				}

				if (fileEntry.FileAttributes.Value.HasFlag(ExFatFileAttributes.Directory))
					RecurseFindMapEntries(partition, entry.DataDescriptor, subpath, map);
			}
		}
	}

	static Report? EnumerateClustersAndCompareWithMap(ExFatPartition partition, ExFatMetaDirectoryEntry entry, string path, Map map)
	{
		Report? report = null;

		int clusterLength = partition.BytesPerCluster;

		long localOffset = 0;
		long remainingBytes = (long)entry.DataDescriptor.LogicalLength;

		Cluster cluster = entry.DataDescriptor.FirstCluster;

		while (remainingBytes > 0)
		{
			long clusterOffset = partition.GetClusterOffset(cluster);

			if (clusterLength > remainingBytes)
				clusterLength = (int)remainingBytes;

			var mapEntry = map.Find(clusterOffset, clusterLength);

			if (mapEntry != null)
			{
				if (mapEntry.Found)
					Console.Error.WriteLine("SANITY FAILURE: Multiple links found to cluster at offset {0}", clusterOffset);

				map.SplitIfNecessary(mapEntry, clusterOffset, clusterLength);

				mapEntry.Found = true;
				mapEntry.LocalOffset = localOffset + (mapEntry.Offset - clusterOffset);
				mapEntry.LocalLength = (int)Math.Min(remainingBytes, mapEntry.Length);

				report ??= new Report(path);
				report.MissingRegions.Add(mapEntry);
			}

			if (cluster.IsLast)
				break;

			localOffset += clusterLength;
			remainingBytes -= clusterLength;

			if (entry.DataDescriptor.Contiguous)
				cluster = cluster + 1;
			else
				cluster = partition.GetNextCluster(cluster);
		}

		return report;
	}
}
