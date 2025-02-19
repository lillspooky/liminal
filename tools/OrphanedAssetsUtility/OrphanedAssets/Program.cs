using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OrphanedAssets
{
    [Serializable]
    class Meta
    {
#pragma warning disable 649
        // ReSharper disable once InconsistentNaming
        public string guid;
#pragma warning restore 649
    }

    class Program
    {
        const string k_MetaExtension = ".meta";
        static readonly string[] k_Base2Suffixes =
        {
            // EiB is the largest unit that can be described by a long count of bytes
            "B", "KiB", "MiB", "GiB", "TiB", "PiB", "EiB"
        };

        static readonly string[] k_IgnoredFolders =
        {
            ".git",
            "Library",
            ".vs",
            "Builds",
            "Temp",
            "obj",
            "Logs",
            "OrphanedAssetsTask"
        };

        static readonly string[] k_IgnoredAssetExtensions =
        {
            ".cs",
            ".json",
            ".asmdef",
            ".api",
            ".md",
            ".unity",
            ".dll",
            ".mdb",
            ".cginc",
            ".preset"
        };

        static readonly string[] k_IgnoredExtensions =
        {
            ".csproj",
            ".png",
            ".fbx",
            ".exr",
            ".mp4",
            ".avi",
            ".tga",
            ".svg",
            ".md",
            ".cs",
            ".ttf"
        };

        static void Main(string[] args)
        {
            var path = Directory.GetCurrentDirectory();
            if (args.Length > 0)
                path = args[0];

            Console.WriteLine($"Searching for orphaned assets in {path}...");

            var metaPaths = new ConcurrentBag<(string, string)>();
            FindMetaFilesRecursively(path, metaPaths);
            var guidToFile = new ConcurrentDictionary<string, string>();
            var results = new ConcurrentDictionary<string, bool>();
            GetGuidsFromMetaPaths(metaPaths, guidToFile, results);
            FindGuidReferencesRecursively(path, results);

            Console.WriteLine($"Searching for assets in {results.Count} meta files...");
            var basePathLength = path.Length;
            var fileResults = new List<(string, long)>();
            foreach (var result in results)
            {
                if (result.Value)
                    continue;

                var guid = result.Key;
                guidToFile.TryGetValue(guid, out var file);
                if (string.IsNullOrEmpty(file))
                    continue;

                var relative = file.Substring(basePathLength, file.Length - basePathLength);
                var fileSize = GetFileSize(file);
                fileResults.Add(($"{relative} ({guid}) - {GetReadableFileSize(fileSize)}", fileSize));
            }

            fileResults.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            Console.WriteLine("Writing results to OrphanedAssets.txt...");

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"{fileResults.Count} assets with no references");
            foreach (var (file, _) in fileResults)
            {
                stringBuilder.AppendLine($"{file}");
            }

            File.WriteAllText("OrphanedAssets.txt", stringBuilder.ToString());
        }

        static long GetFileSize(string path)
        {
            try
            {
                var fileInfo = new FileInfo(path);
                return fileInfo.Length;
            }
            catch
            {
                // ignored
            }

            return 0;
        }

        public static string GetReadableFileSize(long fileSize)
        {
            var number = (decimal)fileSize;

            var i = 0;
            while (number / 1024 >= 1)
            {
                number = number / 1024;
                i++;
            }

            if (number >= 100)
                return $"{number:#0} {k_Base2Suffixes[i]}";

            if (number >= 10)
                return $"{number:#0.#} {k_Base2Suffixes[i]}";

            return $"{number:0.##} {k_Base2Suffixes[i]}";
        }

        static void GetGuidsFromMetaPaths(ConcurrentBag<(string, string)> metaPaths, ConcurrentDictionary<string, string> guidToFile, ConcurrentDictionary<string, bool> results)
        {
            Parallel.ForEach(metaPaths, tuple =>
            {
                var metaPath = tuple.Item1;
                var assetPath = tuple.Item2;
                var guid = GetGuidFromMetaFile(metaPath);
                if (string.IsNullOrEmpty(guid))
                {
                    Console.WriteLine($"Got null guid from {metaPath}");
                    return;
                }

                results[guid] = false;
                guidToFile[guid] = assetPath;
            });
        }

        static void FindGuidReferencesRecursively(string searchPath, ConcurrentDictionary<string, bool> results)
        {
            if (!Directory.Exists(searchPath))
                return;

            // If we have found references to everything, we are done searching
            var earlyOut = true;
            foreach (var kvp in results)
            {
                if (!kvp.Value)
                {
                    earlyOut = false;
                    break;
                }
            }

            if (earlyOut)
                return;

            Parallel.ForEach(Directory.GetFiles(searchPath), file =>
            {
                var extension = Path.GetExtension(file);
                if (extension == k_MetaExtension)
                    return;

                if (k_IgnoredExtensions.Contains(extension))
                    return;

                try
                {
                    foreach (var line in File.ReadAllLines(file))
                    {
                        // If we complete a full pass where all guids are found, early out
                        var early = true;
                        foreach (var kvp in results)
                        {
                            var guid = kvp.Key;
                            if (kvp.Value)
                                continue;

                            if (line.Contains(guid))
                            {
                                results[guid] = true;
                                continue;
                            }

                            early = false;
                        }

                        if (early)
                            return;
                    }
                }
                catch
                {
                    // ignored
                }
            });

            Parallel.ForEach(Directory.GetDirectories(searchPath), directory =>
            {
                var relativePath = Path.GetRelativePath(searchPath, directory);
                if (k_IgnoredFolders.Contains(relativePath))
                    return;

                FindGuidReferencesRecursively(directory, results);
            });
        }

        static string GetGuidFromMetaFile(string metaPath)
        {
            try
            {
                var text = File.ReadAllText(metaPath);
                var builder = new DeserializerBuilder();
                builder.IgnoreUnmatchedProperties();
                var deserializer = builder.Build();
                var meta = deserializer.Deserialize<Meta>(text);
                return meta.guid;
            }
            catch
            {
                // ignored
            }

            return null;
        }

        static void FindMetaFilesRecursively(string searchPath, ConcurrentBag<(string, string)> metas)
        {
            if (!Directory.Exists(searchPath))
                return;

            foreach (var file in Directory.GetFiles(searchPath))
            {
                var extension = Path.GetExtension(file);
                if (extension == k_MetaExtension)
                {
                    // Resources folders are implicitly included
                    if (file.Contains("Resources"))
                        continue;

                    var assetPath = file.Substring(0, file.Length - extension.Length);

                    // Skip folder metas
                    if (Directory.Exists(assetPath))
                        continue;

                    var assetExtension = Path.GetExtension(assetPath);

                    // If there is no extension, we probably don't care about this asset
                    if (string.IsNullOrEmpty(assetExtension))
                        continue;
                    if (k_IgnoredAssetExtensions.Contains(assetExtension))
                        continue;

                    metas.Add((file, assetPath));
                }
            }

            Parallel.ForEach(Directory.GetDirectories(searchPath), directory =>
            {
                var relativePath = Path.GetRelativePath(searchPath, directory);
                if (k_IgnoredFolders.Contains(relativePath))
                    return;

                FindMetaFilesRecursively(directory, metas);
            });
        }
    }
}
