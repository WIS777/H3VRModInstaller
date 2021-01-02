﻿using System;
using System.Threading;
using System.Net;
using H3VRModInstaller.Filesys;
using H3VRModInstaller.Net;
using H3VRModInstaller.JSON;
using H3VRModInstaller.Common;

namespace H3VRModInstaller
{
	public class Downloader
	{

		private static readonly WebClient downloader = new();
		private static bool finished = false;

		/// <summary>
		/// Downloads the mod specified
		/// </summary>
		/// <param name="fileinfo">ModFile, specify here which mod to use</param>
		/// <param name="modnum">Checks if mod already installed</param>
		/// <param name="autoredownload">Auto redownload?</param>
		/// <param name="skipdl">Skips the download process, mostly used for debugging</param>
		/// <returns>Boolean, true</returns>
		public static bool DownloadMod(ModFile fileinfo, int modnum, bool autoredownload = false, bool skipdl = false)
		{
			if(skipdl == true) { Installer.installMod(fileinfo); return true; }
			ModFile[] installedmods = InstalledMods.GetInstalledMods();
			finished = false;
			if (fileinfo.RawName == "" || fileinfo.RawName == null) { return false; }

			string fileToDownload = fileinfo.RawName;
			string locationOfFile = fileinfo.Path;

			bool redownload = false;
			if (!autoredownload)
			{
				for (int i = 0; i < installedmods.Length; i++)
				{
					if (fileinfo.ModId == installedmods[i].ModId)
					{
						if (modnum == 0)
						{
						repeat:
							Console.WriteLine("Mod already installed Are you sure you want to reinstall? (y/n)");
							var input = Console.ReadLine();
							if (input == "y")
							{
								redownload = true;
								break;
							}
							else if (input == "n")
							{
								return false;
							}
							else goto repeat;
						}
						ModInstallerCommon.DebugLog(fileinfo.ModId + " is already installed!");
						return false;
					}
				}
			}
			else redownload = true;
			Uri fileloc = new Uri(locationOfFile + fileToDownload);

			if (ModInstallerCommon.enableDebugging) Console.WriteLine("Downloading {0} from {1}{0}", fileToDownload, locationOfFile);


			Console.WriteLine("");

			downloader.DownloadFileCompleted += dlcomplete;
			downloader.DownloadProgressChanged += dlprogress;
			downloader.DownloadFileAsync(fileloc, fileToDownload);
			while (!finished)
			{
				Thread.Sleep(50);
			}
			finished = false;

			Console.WriteLine("Successfully Downloaded {0}", fileToDownload, locationOfFile);
			if (ModInstallerCommon.enableDebugging) Console.Write("from {1}{0}", fileToDownload, locationOfFile);

			Installer.installMod(fileinfo);

			if(!redownload) InstalledMods.AddInstalledMods(fileinfo.ModId);
			return true;
		}
		public static void dlcomplete(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
		{
			finished = true;
			
		}

		
		/// <summary>
		/// Reports on download progress
		/// </summary>
		/// <param name="sender">Sender</param>
		/// <param name="e">Event Arguments</param>
		public static void dlprogress(object sender, DownloadProgressChangedEventArgs e)
		{
			if ((float)e.TotalBytesToReceive <= 10)
			{
				float mbs = (float)e.BytesReceived / 1000000f;
				string mbstext = String.Format("{0:00.00}", mbs);
				Console.Write("\r" + mbstext + "MBs downloaded!");
			}
			else
			{
				float percentage = ((float)e.BytesReceived / (float)e.TotalBytesToReceive) * 100;
				string percentagetext = String.Format("{0:00.00}", percentage);
				Console.Write("\r" + percentagetext + "% downloaded!");
			}
		}


		/// <summary>
		/// Directs the mod downloading
		/// </summary>
		/// <param name="mod">Mod to direct</param>
		/// <param name="skipdl">Skips the download process</param>
		/// <returns>Boolean, true</returns>
		public static bool DownloadModDirector(string mod, bool skipdl = false)
		{
			if (!NetCheck.isOnline(ModInstallerCommon.pingsite)) { Console.WriteLine("Not connected to internet, or " + ModInstallerCommon.pingsite + " is down!"); return false; }
			var result = ModParsing.getModInfo(mod);
			if (result == null) return false;
			for (var i = 0; i < result.Length; i++)
			{
/*				string[] fileinfo = new string[5];
				fileinfo[0] = result.Item1[i, 0]; //File name
				fileinfo[1] = result.Item1[i, 1]; //File loc
				fileinfo[2] = result.Item1[i, 2]; //args
				fileinfo[3] = i.ToString(); //modnumber
				fileinfo[4] = result.Item1[i, 3]; //File ID*/
				DownloadMod(result[i], i, false, skipdl);
			}
			return true;
		}
		
	}
}
