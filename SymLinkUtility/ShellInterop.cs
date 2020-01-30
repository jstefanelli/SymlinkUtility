using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SymLinkUtility
{
	internal static class ShellInterop
	{
		[DllImport("kernel32.dll", EntryPoint = "CreateSymbolicLink")]
		private static extern bool _createSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, SymbolicLink dwFlags);

		public enum SymbolicLink : int
		{
			File = 0,
			Directory = 1,
			EnableNonPrivileged = 2
		}

		public static void CreateSymbolicLink(string lpSymlinkFileName, string lpTargetFileName, bool directory){
			if (!_createSymbolicLink(lpSymlinkFileName, lpTargetFileName, (directory ? SymbolicLink.Directory : SymbolicLink.File) | SymbolicLink.EnableNonPrivileged))
			{
				throw Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
			}
		}
	}
}
