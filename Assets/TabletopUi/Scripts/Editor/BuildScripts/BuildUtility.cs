﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Facepunch.Editor;
using Galaxy;
using Noon;
using OrbCreationExtensions;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.WSA;

namespace Assets.Core.Utility
{
    public class BuildUtility
    {
        private const string CONST_ELEMENTS = "elements";
        private const string CONST_RECIPES = "recipes";
        private const string CONST_VERBS = "verbs";
        private const string CONST_DECKS = "decks";
        private const string CONST_LEGACIES = "legacies";
        private const string CONST_ENDINGS = "endings";
        private const string CONST_DLC = "DLC";
        private const string CONST_PERPETUALEDITIONLOCATION = "PERPETUAL_ALLDLC";
        private const string CONST_CORE_CONTENT_LOCATION = "StreamingAssets/content/core";
        private const string CONST_DATA_FOLDER_SUFFIX = "_Data";
        private const char CONST_NAME_SEPARATOR_CHAR = '_';
        private const char CONST_SLASH_CHAR = '/';
        
        

        private static string[] GetScenes()
        {
            string[] scenes =
            {
                "Assets/TabletopUi/Logo.unity",
                "Assets/TabletopUi/Quote.unity",
                "Assets/TabletopUi/Menu.unity",
                "Assets/TabletopUi/Tabletop.unity",
                "Assets/TabletopUi/GameOver.unity",
                "Assets/TabletopUi/NewGame.unity",
                "Assets/TabletopUi/Global.unity",
            };

            return scenes;
        }


        private static void MoveDLCContent(BuildTarget target, string contentOfType)
        {
            //for every folder in [outputpath]/[datafolder]/StreamingAssets/content/core/


            var contentPathThisFolder = GetCoreContentPath(target) + CONST_SLASH_CHAR + contentOfType;
            Console.WriteLine(">>>>>> Searching for DLC in " + contentPathThisFolder);

            var contentFiles = Directory.GetFiles(contentPathThisFolder).ToList().FindAll(f => f.EndsWith(".json"));


            foreach (var contentFileNameWithPath in contentFiles)
            {
                Console.WriteLine("checking " + contentFileNameWithPath);

                int dlcMarkerIndex=contentFileNameWithPath.IndexOf(CONST_DLC + CONST_NAME_SEPARATOR_CHAR);

                //does it begin DLC_
                if (dlcMarkerIndex>-1)
                {
                //if so, get its title (DLC_[title]_)
                    string dlcFilenameWithoutPath = contentFileNameWithPath.Substring(dlcMarkerIndex);
                    Console.WriteLine("DLC file found! -" + dlcFilenameWithoutPath);

                    string dlctitle = dlcFilenameWithoutPath.Split(CONST_NAME_SEPARATOR_CHAR)[1];
                    if (string.IsNullOrEmpty(dlctitle))
                        throw new ApplicationException("Couldn't find DLC title for file " + contentFileNameWithPath );

                    //get the DLC location (../DLC/[title]/[platform]/[datafolder]/StreamingAssets/content/core/[contentOfType]/[thatfile]

                    string dlcDestinationDir =
                        getBuildRootPath() + CONST_SLASH_CHAR + CONST_DLC + CONST_SLASH_CHAR +
                        dlctitle + CONST_SLASH_CHAR +
                        getPlatformFolderForTarget(target) + CONST_SLASH_CHAR +
                        dataFolderForTarget(exeNameForTarget(target)) + CONST_SLASH_CHAR +
                        CONST_CORE_CONTENT_LOCATION + CONST_SLASH_CHAR +
                        contentOfType;

                        String dlcFileDestination=dlcDestinationDir + CONST_SLASH_CHAR + dlcFilenameWithoutPath;
                    if (!Directory.Exists(dlcDestinationDir))
                    {
                        Console.WriteLine(">>>>>> creating directory: " + dlcFileDestination);
                        Directory.CreateDirectory(dlcDestinationDir);
                    }

                    if (File.Exists(dlcFileDestination))
                    {
                        Console.WriteLine(">>>>>> deleting old file at this destination");
                        File.Delete(dlcFileDestination);
                    }
                        

                    Console.WriteLine(">>>>>> moving file " + contentFileNameWithPath + " to " + dlcFileDestination);
                    File.Move(contentFileNameWithPath, dlcFileDestination);
                    
                    //throw an error if it doesn't exist
                    //move (don't copy) the file to that location.
                }

            }

        }

        public static void AddVersionNumber(string exeFolder)
        {
            
            string versionPath = exeFolder + "/version.txt";
            Console.WriteLine(">>>>> Writing version to " + versionPath);

            File.WriteAllText(versionPath, NoonUtility.VersionNumber.ToString());


        }

        private static string exeNameForTarget(BuildTarget target)
        {
            if (target == BuildTarget.StandaloneWindows)
                return "cultistsimulator.exe";

            if (target == BuildTarget.StandaloneOSX)
                return "OSX.app";

            if (target == BuildTarget.StandaloneLinuxUniversal)
                return "CS.x86";

            throw new ApplicationException("We don't know how to handle this build target: " + target);
        }

        private static string dataFolderForTarget(string exeName)
        {
            if (exeName.Contains("OSX")) //OSX is cray
                return "OSX.app/Contents/Resources/Data";
            else
            return exeName.Split('.')[0] + CONST_DATA_FOLDER_SUFFIX;
        }

        private static string getPlatformFolderForTarget(BuildTarget target)
        {
            if (target == BuildTarget.StandaloneWindows)
                return "Windows";

            if (target == BuildTarget.StandaloneOSX)
                return "OSX";

            if (target == BuildTarget.StandaloneLinuxUniversal)
                return "Linux";

            throw new ApplicationException("We don't know how to handle this build target: " + target);
        }

        private static void PostBuildFileTasks(BuildTarget target, string pathToBuiltProject)
        {
            Console.WriteLine("pathToBuiltProject in postfilebuildtasks: " + pathToBuiltProject);

            Console.WriteLine(">>>>> Copying Steam libraries");
            CopySteamLibraries.Copy(target, pathToBuiltProject);
            Console.WriteLine(">>>>> Copying Galaxy libraries");
            CopyGalaxyLibraries.Copy(target, pathToBuiltProject + "/"); //we need the extra slash; we've been avoiding using it elsewhere in this code, but the GOG code expects it, and I don't want to edit it
                                                                        //It belatedly occurs to me that there's a reason for using trailing slashes to indicate folders.

            Console.WriteLine(">>>>> Adding version number");
            AddVersionNumber(pathToBuiltProject);

            string perpetualEditionLocation = getBuildRootPath() + CONST_SLASH_CHAR + CONST_PERPETUALEDITIONLOCATION + CONST_SLASH_CHAR + getPlatformFolderForTarget(target);

            Console.WriteLine(">>>>> Copying whole project with DLC from " + pathToBuiltProject + " to Perpetual Edition location at " + perpetualEditionLocation);

            DeleteContentsOfDirectory(perpetualEditionLocation);

            CopyDirectoryWithContents(pathToBuiltProject, perpetualEditionLocation, true);

            Console.WriteLine(">>>>> Moving DLC content to individual DLC locations");

            MoveDLCContent(target, CONST_ELEMENTS);
            MoveDLCContent(target, CONST_RECIPES);
            MoveDLCContent(target, CONST_VERBS);
            MoveDLCContent(target, CONST_DECKS);
            MoveDLCContent(target, CONST_LEGACIES);
            MoveDLCContent(target, CONST_ENDINGS);
        }

        private static string GetCoreContentPath(BuildTarget target)
        {
            return getBuildOutputPathForPlatform(target) + CONST_SLASH_CHAR +  dataFolderForTarget(exeNameForTarget(target)) +  CONST_SLASH_CHAR +  CONST_CORE_CONTENT_LOCATION;
        }

        private static string getBuildRootPath()
        {
            return System.Environment.GetCommandLineArgs()[1];

        }

        private static string getBuildOutputPathForPlatform(BuildTarget target)
        {

                return getBuildRootPath() + CONST_SLASH_CHAR + getPlatformFolderForTarget(target);
        }

        public static void PerformWindowsBuild()
        {

            BuildTarget thisBuildTarget = BuildTarget.StandaloneWindows;
            try
            {


            BuildPlayerOptions windowsBuildPlayerOptions =
                new BuildPlayerOptions
                {
                    target = BuildTarget.StandaloneWindows,
                    locationPathName = getBuildOutputPathForPlatform(thisBuildTarget) + CONST_SLASH_CHAR + exeNameForTarget(thisBuildTarget)
                };

                Console.WriteLine(">>>>>> Building Windows version to " + windowsBuildPlayerOptions.locationPathName);

            windowsBuildPlayerOptions.scenes = GetScenes();

            BuildPipeline.BuildPlayer(windowsBuildPlayerOptions);

            PostBuildFileTasks(thisBuildTarget, getBuildOutputPathForPlatform(thisBuildTarget));
            }
            catch (Exception e)
            {
                Debug.Log(">>>>>>ERROR: " + e.Message);
            }

        }

        public static void PerformOsxBuild()
        {
            BuildTarget thisBuildTarget = BuildTarget.StandaloneOSX;

            try
            {

            BuildPlayerOptions osxBuildPlayerOptions = new BuildPlayerOptions
            {
                target = BuildTarget.StandaloneOSX,
                locationPathName = getBuildOutputPathForPlatform(thisBuildTarget) + CONST_SLASH_CHAR + exeNameForTarget(thisBuildTarget),
                scenes = GetScenes()
            };

           Console.WriteLine(">>>>>> Building OSX version to " + osxBuildPlayerOptions.locationPathName);

            BuildPipeline.BuildPlayer(osxBuildPlayerOptions);

            //for some reason, the OSX build barfs when copying the steam libraries in PostProcessHook...but not in here.
            //So I've moved it to here.
            //does the folder not exist at that point?


        PostBuildFileTasks(thisBuildTarget, getBuildOutputPathForPlatform(thisBuildTarget));

            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e.Message);
            }
        }

        public static void PerformLinuxBuild()
        {
            BuildTarget thisBuildTarget = BuildTarget.StandaloneLinuxUniversal;


            try
            {

            BuildPlayerOptions linuxBuildPlayerOptions =
                new BuildPlayerOptions
                {
                    target = BuildTarget.StandaloneLinuxUniversal,
                    locationPathName =  getBuildOutputPathForPlatform(thisBuildTarget) + CONST_SLASH_CHAR + exeNameForTarget(thisBuildTarget),
                scenes = GetScenes()
                };
                Console.WriteLine(">>>>>> Building Linux version to " + linuxBuildPlayerOptions.locationPathName);

            BuildPipeline.BuildPlayer(linuxBuildPlayerOptions);

            PostBuildFileTasks(thisBuildTarget, getBuildOutputPathForPlatform(thisBuildTarget));
            }
            catch (Exception e)
            {
                Debug.Log("ERROR: " + e.Message);
            }
        }


        private static void DeleteContentsOfDirectory(string perpetualEditionLocation)
        {

            System.IO.DirectoryInfo di = new DirectoryInfo(perpetualEditionLocation);
            if (!di.Exists)
            {
                Console.WriteLine(">>>>> Couldn't find " + perpetualEditionLocation + " to delete");
                return;
                ;
            }

            foreach (FileInfo file in di.GetFiles())
            {
                file.Delete();
            }
            foreach (DirectoryInfo dir in di.GetDirectories())
            {
                dir.Delete(true);
            }
        }

        private static void CopyDirectoryWithContents(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (isPermittedFileToCopy(file))
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    CopyDirectoryWithContents(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        private static bool isPermittedFileToCopy(FileInfo file)
        {
            return file.Name != ".dropbox";
        }


        //[PostProcessBuild]
        //public static void PostProcessHook(BuildTarget target, string pathToBuiltProject)
        //{
        //    try
        //    {

        //    CopySteamLibraries.Copy(target, pathToBuiltProject);
        //    CopyGalaxyLibraries.Copy(target, pathToBuiltProject);

        //    string exeFolder = Path.GetDirectoryName(pathToBuiltProject);
        //    AddVersionNumber(exeFolder);
        //    }
        //    catch (Exception e)
        //    {
        //        Debug.Log("ERROR: " + e.Message);
        //    }


        //}


    }
    }

