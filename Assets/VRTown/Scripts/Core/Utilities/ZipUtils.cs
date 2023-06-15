

using System.IO.Compression;

public static class ZipUtils
{
    public static byte[] GetEntryData(this ZipArchive zip, string entryPath)
    {
        var entry = zip.GetEntry(entryPath);
        if (entry == null)
        {
            return null;
        }
        var data = new byte[entry.Length];
        entry.Open().Read(data, 0, data.Length);
        return data;
    }
}