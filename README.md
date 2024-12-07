# FindAffectedFilesExfat

After running `ddrescue` on a failing device, the result is a copied image of everything recoverable and a map file that lists the regions. That map file tells you physical offsets that couldn't be read, but how do you know which files were affected? This utility enumerates files on the volume, walks through their allocated disk space and outputs intersections with bad regions identified by a corresponding map file.

This only works if the directory structure is not itself affected by bad sectors. Hopefully this is the case :-)

## Usage

The tool takes two paths on the command-line. The first is the path to the image file to inspect. This can be any readable path whose content is an exFAT filesystem, but generally speaking it should probably not be the actual failing device itself. The second is the map file from `ddrescue` that includes the listing of bad regions.

```
dotnet run /sdrecover/card.img /sdrecover/card.map
```

## Output

There are two sections to the output from this utility. First, each matching file is output, with a list of the regions within the file that correspond to bad regions from the map file:

```
/Download/1460690483513_Drabblecast-261-The-People-of-Sand-and-Slag.mp3
* 4096 bytes at offset 34660352 (0x00000210E000)
* 4096 bytes at offset 35844096 (0x00000222F000)
* 4096 bytes at offset 36356096 (0x0000022AC000)
* 4096 bytes at offset 38989824 (0x00000252F000)
* 4096 bytes at offset 42909696 (0x0000028EC000)
* 4096 bytes at offset 44486656 (0x000002A6D000)
* 8192 bytes at offset 45268992 (0x000002B2C000)
...
```

Then, once file enumeration is complete, any remaining bad regions that didn't match anything are listed. These are _probably_ in unused space on the device.

```
The following regions were not found linked to any file:
- 0x0004C115F000: 4096 bytes
- 0x0004C119C000: 4096 bytes
- 0x0004C119E000: 4096 bytes
- 0x0004C11AE000: 4096 bytes
- 0x0004C11BC000: 16384 bytes
- 0x0004C11CF000: 4096 bytes
- 0x0004C11DC000: 512 bytes
...
```
