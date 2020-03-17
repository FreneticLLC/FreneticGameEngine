//
// This file is part of the Frenetic Game Engine, created by Frenetic LLC.
// This code is Copyright (C) Frenetic LLC under the terms of a strict license.
// See README.md or LICENSE.txt in the FreneticGameEngine source root for the contents of the license.
// If neither of these are available, assume that neither you nor anyone other than the copyright holder
// hold any right or permission to use this software until such time as the official license is identified.
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using FGECore.CoreSystems;
using FreneticUtilities.FreneticExtensions;
using FreneticUtilities.FreneticFilePackage;
using FreneticUtilities.FreneticToolkit;

namespace FGECore.FileSystems
{
    /// <summary>
    /// Primary handler for files within the Frenetic Game Engine.
    /// Must call <see cref="Init(string, string, string)"/> before using.
    /// </summary>
    public class FileEngine
    {
        #region Static utilities
        /// <summary>
        /// The file extension for package files: "ffp"
        /// </summary>
        public const string PACKAGE_EXTENSION = "ffp";

        /// <summary>
        /// The search pattern for package files: "*.ffp"
        /// </summary>
        public const string PACKAGE_SEARCH_PATTERN = "*." + PACKAGE_EXTENSION;

        /// <summary>
        /// Cleans a string to only valid symbols for a file name to contain.
        /// </summary>
        /// <param name="filename">The input filename.</param>
        /// <returns>The cleaned file name.</returns>
        public static string CleanFileName(string filename)
        {
            return FFPUtilities.CleanFileName(filename);
        }

        private static void PackageWarningMethod(string warning)
        {
            SysConsole.Output(OutputType.WARNING, "[FileEngine/PackageHandler] " + warning);
        }
        #endregion

        #region Current data
        /// <summary>
        /// The internal data within this <see cref="FileEngine"/>.
        /// </summary>
        public struct InternalData
        {
            /// <summary>
            /// A mapping of all loaded packages by their file names.
            /// </summary>
            public Dictionary<string, FFPackage> Packages;

            /// <summary>
            /// A list of packages in order of being added.
            /// </summary>
            public List<FFPackage> PackageList;

            /// <summary>
            /// A mapping of all available packaged files by their file names.
            /// </summary>
            public Dictionary<string, FFPFile> Files;

            /// <summary>
            /// A root folder for path management.
            /// </summary>
            public FFPFolder RootFolder;

            /// <summary>
            /// A set of all raw data folders used by the system.
            /// </summary>
            public HashSet<string> RawFolders;

            /// <summary>
            /// Names of all files in the saves folder.
            /// </summary>
            public HashSet<string> SavedFiles;

            /// <summary>
            /// The saves folder.
            /// </summary>
            public string SavesFolder;
        }

        /// <summary>
        /// The internal data within this <see cref="FileEngine"/>.
        /// </summary>
        public InternalData Internal = new InternalData()
        {
            Packages = new Dictionary<string, FFPackage>(64),
            PackageList = new List<FFPackage>(64),
            Files = new Dictionary<string, FFPFile>(1024),
            RootFolder = new FFPFolder(),
            RawFolders = new HashSet<string>(),
            SavedFiles = new HashSet<string>()
        };
        #endregion

        #region Properties
        /// <summary>
        /// Gets the saves folder.
        /// This handles both where files are saved and what folder has highest priority for loads.
        /// </summary>
        public string SavesFolder
        {
            get
            {
                return Internal.SavesFolder;
            }
        }
        #endregion

        #region Setup
        /// <summary>
        /// Initializes the file engine, with the given data/ and mods/ folder paths.
        /// <para>
        /// An example of a sufficient call is: Init("data", "mods", "saves");
        /// Or: Init("data", "mods", "data");
        /// </para>
        /// <para>
        /// The data folder path should be where basic engine data is stored, including packages of core data, and raw data directly within the folder.
        /// This means there are paths of the form "(data)/textures/example.png"
        /// </para>
        /// <para>
        /// The mods folder should contain user-customizable alternate data, including packages of custom data, and subfolders containing raw custom data.
        /// This means there are paths of the form "(mods)/(subfolder)/textures/example.png"
        /// </para>
        /// <para>
        /// The saves folder can be an otherwise empty folder, or equal to the data folder (or any raw data folder). It cannot be equal to the mods folder (or any other meta-folder).
        /// This means it should be valid to contain paths of the form "(saves)/textures/example.png"
        /// </para>
        /// <para>
        /// Note that raw (unpackaged) files should be avoided whenever possible.
        /// </para>
        /// <para>
        /// The priority order of files is:
        /// First, raw files in the saves folder.
        /// Then, packaged files in reverse of the order packages were loaded (so a package loaded last takes priority over the rest).
        /// Last, raw files in reverse order of raw folder adding.
        /// Note also that the contents of the data folder are added first, followed by the contents of the mods folder.
        /// </para>
        /// </summary>
        /// <param name="dataFolderPath">The data folder path.</param>
        /// <param name="modsFolderPath">The mods folder path.</param>
        /// <param name="savesFolderPath">The saves folder path.</param>
        public void Init(string dataFolderPath, string modsFolderPath, string savesFolderPath)
        {
            AddPackagesFromFolder(dataFolderPath);
            AddRawDataFolder(dataFolderPath);
            AddPackagesFromFolder(modsFolderPath);
            AddRawDataSubfoldersFrom(modsFolderPath);
            SetSavesFolder(savesFolderPath);
        }

        /// <summary>
        /// Sets the saves folder to a new folder.
        /// This handles both where files are saved and what folder has highest priority for loads.
        /// </summary>
        /// <param name="folder">The folder.</param>
        public void SetSavesFolder(string folder)
        {
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            folder = Path.GetFullPath(folder);
            Internal.SavedFiles.Clear();
            Internal.SavesFolder = folder;
            Internal.SavedFiles.UnionWith(Directory.EnumerateFiles(folder, "*.*", SearchOption.AllDirectories));
        }

        /// <summary>
        /// Cleans up the <see cref="FileEngine"/>, closing all streams and any other data.
        /// </summary>
        public void Cleanup()
        {
            Internal.SavedFiles.Clear();
            Internal.SavesFolder = null;
            Internal.Files.Clear();
            Internal.RootFolder.Contents.Clear();
            Internal.RawFolders.Clear();
            foreach (FFPackage package in Internal.PackageList)
            {
                package.FileStream.Dispose();
            }
            Internal.PackageList.Clear();
            Internal.Packages.Clear();
        }
        #endregion

        #region Extra folders
        /// <summary>
        /// Adds all packages in a folder (but not subfolders) to the engine.
        /// This is akin to the default "data" and "mods" folders.
        /// </summary>
        /// <param name="folder">The folder path that contains packages.</param>
        public void AddPackagesFromFolder(string folder)
        {
            folder = Path.GetFullPath(folder);
            if (!Directory.Exists(folder))
            {
                return;
            }
            foreach (string file in Directory.EnumerateFiles(folder, PACKAGE_SEARCH_PATTERN))
            {
                AddPackageFile(file);
            }
        }

        /// <summary>
        /// Returns whether a package with the given package file name is loaded.
        /// </summary>
        /// <param name="package">The package file name.</param>
        /// <returns>Whether it is already loaded.</returns>
        public bool IsPackageLoaded(string package)
        {
            return Internal.Packages.ContainsKey(package);
        }

        /// <summary>
        /// Adds a package to the handler.
        /// </summary>
        /// <param name="filename">The package's file name on disk.</param>
        public void AddPackageFile(string filename)
        {
            if (IsPackageLoaded(filename))
            {
                return;
            }
            FFPackage package = new FFPackage(File.OpenRead(filename), PackageWarningMethod);
            AddPackage(filename, package);
        }

        /// <summary>
        /// Adds a package to the handler.
        /// </summary>
        /// <param name="packageName">The name of the package - usually a full filename.</param>
        /// <param name="package">The package to add.</param>
        public void AddPackage(string packageName, FFPackage package)
        {
            Internal.Packages.Add(packageName, package);
            Internal.PackageList.Add(package);
            IncludeFilesFrom(package);
        }

        /// <summary>
        /// Adds all subfolders containing raw data from the base folder.
        /// This is akin to the default "mods" folder.
        /// This means there are paths of the form "(folder)/(subfolder)/textures/example.png"
        /// <para>
        /// Note that raw (unpackaged) files should be avoided whenever possible.
        /// </para>
        /// </summary>
        /// <param name="folder">The folder containing subfolders of raw data.</param>
        public void AddRawDataSubfoldersFrom(string folder)
        {
            folder = Path.GetFullPath(folder);
            if (!Directory.Exists(folder))
            {
                return;
            }
            Internal.RawFolders.UnionWith(Directory.EnumerateDirectories(folder));
        }

        /// <summary>
        /// Adds a folder containing raw data to the engine.
        /// This is akin to the default "data" folder.
        /// This means there are paths of the form "(folder)/textures/example.png"
        /// <para>
        /// Note that raw (unpackaged) files should be avoided whenever possible.
        /// </para>
        /// </summary>
        /// <param name="folder">The folder of raw data.</param>
        public void AddRawDataFolder(string folder)
        {
            folder = Path.GetFullPath(folder);
            Internal.RawFolders.Add(folder);
        }

        /// <summary>
        /// Removes a package from the engine, based on the package filename.
        /// <para>
        /// This is a slow operation, that involves reprocessing the packaged files list.
        /// </para>
        /// </summary>
        /// <param name="filename">The package filename to remove.</param>
        public void RemovePackage(string filename)
        {
            if (!Internal.Packages.TryGetValue(filename, out FFPackage package))
            {
                return;
            }
            package.FileStream.Dispose();
            Internal.Packages.Remove(filename);
            Internal.PackageList.Remove(package);
            ReprocessFilesList();
        }

        private void ReprocessFilesList()
        {
            Internal.Files.Clear();
            Internal.RootFolder.Contents.Clear();
            foreach (FFPackage package in Internal.PackageList)
            {
                IncludeFilesFrom(package);
            }
        }

        private void IncludeFilesFrom(FFPackage package)
        {
            Internal.Files.UnionWith(package.Files);
            foreach (KeyValuePair<string, FFPFile> file in package.Files)
            {
                Internal.RootFolder.AddFile(file.Key, file.Value, true);
            }
        }

        /// <summary>
        /// Removes a raw data folder from the engine, based on the folder path.
        /// </summary>
        /// <param name="folder">The folder path to remove.</param>
        public void RemoveRawDataFolder(string folder)
        {
            folder = Path.GetFullPath(folder);
            Internal.RawFolders.Remove(folder);
        }
        #endregion

        #region File exist checks
        /// <summary>
        /// Returns whether a folder exists by the given name.
        /// </summary>
        /// <param name="folder">The name of the folder.</param>
        /// <returns>A boolean indicating whether the folder exists.</returns>
        public bool FolderExists(string folder)
        {
            folder = CleanFileName(folder);
            if (Internal.RootFolder.HasSubFolder(folder))
            {
                return true;
            }
            if (Directory.Exists(Internal.SavesFolder + "/" + folder))
            {
                return true;
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (Directory.Exists(rawFolder + "/" + folder))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether a file exists by the given name.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>A boolean indicating whether the file exists.</returns>
        public bool FileExists(string filename)
        {
            filename = CleanFileName(filename);
            if (Internal.Files.ContainsKey(filename))
            {
                return true;
            }
            if (File.Exists(Internal.SavesFolder + "/" + filename))
            {
                return true;
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (File.Exists(rawFolder + "/" + filename))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether a file exists within a package (overriding any raw files except the saves folder) by the given name.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>A boolean indicating whether the file exists.</returns>
        public bool FileIsPackaged(string filename)
        {
            filename = CleanFileName(filename);
            if (File.Exists(Internal.SavesFolder + "/" + filename))
            {
                return false;
            }
            return Internal.Files.ContainsKey(filename);
        }

        /// <summary>
        /// Returns whether a file exists as a raw file on disk (and NOT in any packages or the saves folder) by the given name.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>A boolean indicating whether the file exists.</returns>
        public bool FileIsRaw(string filename)
        {
            filename = CleanFileName(filename);
            if (Internal.Files.ContainsKey(filename))
            {
                return false;
            }
            if (File.Exists(Internal.SavesFolder + "/" + filename))
            {
                return false;
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (File.Exists(rawFolder + "/" + filename))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns whether a file exists within the saves folder (overriding any packages or other raw folders).
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>A boolean indicating whether the file exists.</returns>
        public bool FileIsSaved(string filename)
        {
            filename = CleanFileName(filename);
            return File.Exists(Internal.SavesFolder + "/" + filename);
        }
        #endregion

        #region File reading
        /// <summary>
        /// Reads the data in a file, returning the full data as a byte array.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>The file's raw data.</returns>
        /// <exception cref="FileNotFoundException">When the filename doesn't refer to a valid file.</exception>
        public byte[] ReadFileData(string filename)
        {
            if (TryReadFileData(filename, out byte[] data))
            {
                return data;
            }
            throw new FileNotFoundException("File not found", filename);
        }

        /// <summary>
        /// Reads the text in a file, returning the full data as a string.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <returns>The file's text.</returns>
        /// <exception cref="FileNotFoundException">When the filename doesn't refer to a valid file.</exception>
        public string ReadFileText(string filename)
        {
            if (TryReadFileText(filename, out string text))
            {
                return text;
            }
            throw new FileNotFoundException("File not found", filename);
        }

        private bool TryReadFileDataSingle(string filename, out byte[] data)
        {
            filename = CleanFileName(filename);
            if (Internal.Files.TryGetValue(filename, out FFPFile packagedFile))
            {
                data = packagedFile.ReadFileData();
                return true;
            }
            if (File.Exists(Internal.SavesFolder + "/" + filename))
            {
                data = File.ReadAllBytes(Internal.SavesFolder + "/" + filename);
                return true;
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (File.Exists(rawFolder + "/" + filename))
                {
                    data = File.ReadAllBytes(rawFolder + "/" + filename);
                    return true;
                }
            }
            data = null;
            return false;
        }
        
        /// <summary>
        /// Tries to read the data in a file, returning whether the read was successful (and if so, outputting the full data as a byte array).
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="data">The data in the file, if found.</param>
        /// <returns>Whether the file was found.</returns>
        public bool TryReadFileData(string filename, out byte[] data)
        {
            if (TryReadFileDataSingle(filename, out data))
            {
                return true;
            }
            else if (TryReadFileDataSingle(filename + "~2", out data))
            {
                return true;
            }
            // Note: ~1 are likely corrupted, so ignore them.
            return false;
        }

        /// <summary>
        /// Tries to read the text in a file, returning whether the read was successful (and if so, outputting the full data as a string).
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="data">The data in the file, if found.</param>
        /// <returns>Whether the file was found.</returns>
        public bool TryReadFileText(string filename, out string data)
        {
            if (TryReadFileData(filename, out byte[] rawData))
            {
                data = StringConversionHelper.UTF8Encoding.GetString(rawData);
                return true;
            }
            data = null;
            return false;
        }
        #endregion

        #region File listing
        private IEnumerable<IEnumerable<string>> EnumerateFileSets(string folder, bool deep)
        {
            folder = CleanFileName(folder);
            if (Internal.RootFolder.TryGetSubFolder(folder, out FFPFolder subfolder))
            {
                yield return subfolder.EnumerateFiles();
            }
            if (Directory.Exists(Internal.SavesFolder + "/" + folder))
            {
                string fullPath = Path.GetFullPath(Internal.SavesFolder + "/" + folder);
                yield return Directory.EnumerateFiles(fullPath).Select(s => CleanFileName(s.Substring(fullPath.Length)));
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (Directory.Exists(rawFolder + "/" + folder))
                {
                    string fullPath = Path.GetFullPath(rawFolder + "/" + folder);
                    yield return Directory.EnumerateFiles(fullPath).Select(s => CleanFileName(s.Substring(fullPath.Length)));
                }
            }
            if (deep)
            {
                foreach (string subFolder in ListFolders(folder))
                {
                    foreach (IEnumerable<string> files in EnumerateFileSets(folder + "/" + subFolder, true))
                    {
                        yield return files.Select(s => subFolder + "/" + s);
                    }
                }
            }
        }

        /// <summary>
        /// Returns a list of all files - does not include folders in return value.
        /// Does not necessarily preserve any given order.
        /// Returns only the file name, not full paths. If deep is specified, will only include the path starting folder name.
        /// </summary>
        /// <param name="folder">The folder path.</param>
        /// <param name="extension">The file extension to search for, or null. For example, ListFiles("path/", "txt") will include "example.txt" but exclude "example.png".</param>
        /// <param name="deep">Whether to search in subfolders.</param>
        /// <returns>A list of all contained files.</returns>
        /// <exception cref="DirectoryNotFoundException">When no folder exists by the given path.</exception>
        public string[] ListFiles(string folder, string extension = null, bool deep = false)
        {
            HashSet<string> files = new HashSet<string>();
            string fullExtension = extension == null ? null : "." + extension;
            foreach (IEnumerable<string> fileSet in EnumerateFileSets(folder, deep))
            {
                if (fullExtension != null)
                {
                    files.UnionWith(fileSet.Where(f => f.EndsWith(fullExtension)));
                }
                else
                {
                    files.UnionWith(fileSet);
                }
            }
            return files.ToArray();
        }

        private IEnumerable<IEnumerable<string>> EnumerateFolderSets(string folder)
        {
            folder = CleanFileName(folder);
            if (Internal.RootFolder.TryGetSubFolder(folder, out FFPFolder subfolder))
            {
                yield return subfolder.EnumerateFolders();
            }
            if (Directory.Exists(Internal.SavesFolder + "/" + folder))
            {
                string fullPath = Path.GetFullPath(Internal.SavesFolder + "/" + folder);
                yield return Directory.EnumerateDirectories(fullPath).Select(s => CleanFileName(s.Substring(fullPath.Length)));
            }
            foreach (string rawFolder in Internal.RawFolders)
            {
                if (Directory.Exists(rawFolder + "/" + folder))
                {
                    string fullPath = Path.GetFullPath(rawFolder + "/" + folder);
                    yield return Directory.EnumerateDirectories(fullPath).Select(s => CleanFileName(s.Substring(fullPath.Length)));
                }
            }
        }

        /// <summary>
        /// Returns a list of all folders within a folder path - does not search within subfolders, does not include files in return value.
        /// Does not necessarily preserve any given order.
        /// Returns only the folder name, not full paths.
        /// </summary>
        /// <param name="folder">The folder path.</param>
        /// <returns>A list of all contained folders.</returns>
        /// <exception cref="DirectoryNotFoundException">When no folder exists by the given path.</exception>
        public string[] ListFolders(string folder)
        {
            HashSet<string> folders = new HashSet<string>();
            foreach (IEnumerable<string> folderSet in EnumerateFolderSets(folder))
            {
                folders.UnionWith(folderSet);
            }
            return folders.ToArray();
        }
        #endregion

        #region File writing
        /// <summary>
        /// Writes a file to disk with the given filename containing the given data.
        /// This does not use journalling mode, and should be avoided. Use <see cref="WriteFileDataJournalling(string, byte[])"/> instead.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="data">The file's raw data.</param>
        public void WriteFileData(string filename, byte[] data)
        {
            filename = CleanFileName(filename);
            Internal.SavedFiles.Add(filename);
            string fullPath = Internal.SavesFolder + "/" + filename;
            string directoryName = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            File.WriteAllBytes(fullPath, data);
        }

        /// <summary>
        /// Writes a file to disk with the given filename containing the given text.
        /// This does not use journalling mode, and should be avoided. Use <see cref="WriteFileTextJournalling(string, string)"/> instead.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="text">The file's text.</param>
        public void WriteFileText(string filename, string text)
        {
            WriteFileData(filename, StringConversionHelper.UTF8Encoding.GetBytes(text));
        }

        /// <summary>
        /// Writes a file to disk with the given filename containing the given data, using journalling mode.
        /// This is a special helper to avoid unreadable files if the system crashes during a write.
        /// Note that all file reads check for journalling files.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="data">The file's raw data.</param>
        public void WriteFileDataJournalling(string filename, byte[] data)
        {
            filename = CleanFileName(filename);
            Internal.SavedFiles.Add(filename);
            string fullPath = Internal.SavesFolder + "/" + filename;
            string directoryName = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            File.WriteAllBytes(fullPath + "~1", data);
            if (File.Exists(fullPath))
            {
                File.Move(fullPath, fullPath + "~2");
            }
            File.Move(fullPath + "~1", fullPath);
            if (File.Exists(fullPath + "~2"))
            {
                File.Delete(fullPath + "~2");
            }
        }

        /// <summary>
        /// Writes a file to disk with the given filename containing the given text, using journalling mode.
        /// This is a special helper to avoid unreadable files if the system crashes during a write.
        /// Note that all file reads check for journalling files.
        /// </summary>
        /// <param name="filename">The name of the file.</param>
        /// <param name="text">The file's text.</param>
        public void WriteFileTextJournalling(string filename, string text)
        {
            WriteFileDataJournalling(filename, StringConversionHelper.UTF8Encoding.GetBytes(text));
        }
        #endregion
    }
}
