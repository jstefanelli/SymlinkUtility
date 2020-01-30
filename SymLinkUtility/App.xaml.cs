using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;

namespace SymLinkUtility
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public const string FromStartLongParam = "--fromStart";
		public const string FromStartShortParam = "-s";

		public const string FromEndLongParam = "--fromEnd";
		public const string FromEndShortParam = "-e";

		public const string FileModeLongParam = "--file";
		public const string FileModeShortParam = "-f";

		public const string HelpParam = "--help";

		private void ShowHelpmessage(bool error = false)
		{

			string str = "Usage:\n" +
			$"\tslu.exe [{FileModeLongParam}|{FileModeShortParam}] [" + FromStartLongParam + "|" + FromStartShortParam + "] [source_path]\n" +
			$"\tslu.exe [{FileModeLongParam}|{FileModeShortParam}] [" + FromEndLongParam + "|" + FromEndShortParam + "] [target_path]\n" +
			"\tslu.exe " + HelpParam;
			MessageBox.Show(str, "SymLinkUtility", MessageBoxButton.OK, error ? MessageBoxImage.Error : MessageBoxImage.Information);
		}

		private void Application_Startup(object sender, StartupEventArgs e)
		{
			bool fileMode = false;
			bool fromStartMode = true;

			string fileName = null;
			foreach(string s in e.Args){
				switch (s)
				{
					case FileModeLongParam:
					case FileModeShortParam:
						fileMode = true;
						break;
					case FromEndLongParam:
					case FromEndShortParam:
						fromStartMode = false;
						break;
					case FromStartLongParam:
					case FromStartShortParam:
						fromStartMode = true;
						break;
					case HelpParam:
						ShowHelpmessage();
						Shutdown();
						return;
					default:
						if(fileName != null){
							MessageBox.Show("Too many parameters given.", "SymLinkUtility Error", MessageBoxButton.OK, MessageBoxImage.Error);
							Shutdown();
							return;
						}
						fileName = s;
						break;
				}
			}

			string itemName = fileMode ? "file" : "directory";

			string start = "";
			string target = "";

			if(fromStartMode){
				if(!string.IsNullOrWhiteSpace(fileName)){
					bool exists = fileMode ? File.Exists(fileName) : Directory.Exists(fileName);
					if (exists)
						start = fileName;
					else
					{
						string initialDir = Environment.CurrentDirectory;
						if (fileMode && Directory.Exists(fileName))
						{
							initialDir = fileName;
						}
						if(PickStart(initialDir, null, out string res)){
							start = res;
						}else{
							Shutdown();
							return;
						}
					}
				}
				if(PickEnd(null, fileMode, out string result)){
					target = result;
				}else{
					Shutdown();
					return;
				}
			}else
			{
				if (!string.IsNullOrWhiteSpace(fileName))
				{
					bool exists = fileMode ? File.Exists(fileName) : Directory.Exists(fileName);
					if (exists)
						target = fileName;
					else{
						string initialDir = Environment.CurrentDirectory;
						if(fileMode && Directory.Exists(fileName)){
							initialDir = fileName;
						}
						if(PickEnd(initialDir, fileMode, out string res)){
							target = res;
						}else{
							Shutdown();
							return;
						}
					}
				}
				if (PickStart(null, Path.GetFileName(target), out string result))
				{
					start = result;
				}
				else
				{
					Shutdown();
					return;
				}
			}

			bool endExists = fileMode ? File.Exists(target) : Directory.Exists(target);
			if(!endExists){
				MessageBox.Show($"Symlink target {target} does not exists", "SymLink Utility Error", MessageBoxButton.OK, MessageBoxImage.Error);
				Shutdown();
				return;
			}

			try
			{
				ShellInterop.CreateSymbolicLink(start, target, !fileMode);
			}
			catch (Exception ex)
			{
#if DEBUG
				MessageBox.Show("Operation Failed: " + ex.Message + " (" + ex.GetType() + ")\nStart: " + start + "\nEnd: " + target);
#else
				MessageBox.Show("Operation Failed.");
#endif
			}
			Shutdown();
		}

		private bool PickStart(string baseDirectory, string initialFileName, out string result){
			result = null;
			CommonSaveFileDialog sfd = new CommonSaveFileDialog();
			if (!string.IsNullOrWhiteSpace(initialFileName))
				sfd.DefaultFileName = initialFileName;
			sfd.Title = "Select SymLink start path";
			if (baseDirectory != null)
				baseDirectory = Path.GetFullPath(baseDirectory);
			sfd.InitialDirectory = baseDirectory;
			CommonFileDialogResult rs = sfd.ShowDialog();
			if(rs == CommonFileDialogResult.Ok){
				result = sfd.FileName;
				return true;
			}
			return false;
		}

		private bool PickEnd(string baseDirectory, bool fileMode, out string result){
			result = null;
			CommonOpenFileDialog dlg = new CommonOpenFileDialog();
			if (baseDirectory != null)
				baseDirectory = Path.GetFullPath(baseDirectory);
			dlg.InitialDirectory = baseDirectory;
			dlg.Title = "Select SymLink end (target) path";
			dlg.IsFolderPicker = !fileMode;
			dlg.EnsureFileExists = true;
			CommonFileDialogResult rs = dlg.ShowDialog();
			if(rs == CommonFileDialogResult.Ok){
				result = dlg.FileName;
				return true;
			}
			return false;
		}
	}
}
