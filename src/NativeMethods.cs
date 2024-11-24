using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;

namespace SingleInstanceCore
{
	[SuppressUnmanagedCodeSecurity]
	internal static partial class NativeMethods
	{
		/// <summary>
		/// Delegate declaration that matches WndProc signatures.
		/// </summary>
		public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

		[LibraryImport("shell32.dll", EntryPoint = "CommandLineToArgvW", StringMarshalling = StringMarshalling.Utf16)]
		private static partial IntPtr CommandLineToArgvW([MarshalAs(UnmanagedType.LPWStr)] string cmdLine, out int numArgs);

		[LibraryImport("kernel32.dll", EntryPoint = "LocalFree", SetLastError = true)]
		private static partial IntPtr LocalFree(IntPtr hMem);


		public static string[] CommandLineToArgvW(string cmdLine)
		{
			IntPtr argv = IntPtr.Zero;
			try
			{
				argv = CommandLineToArgvW(cmdLine, out var numArgs);
				if (argv == IntPtr.Zero)
					throw new Win32Exception();

				var result = new string[numArgs];

				for (var i = 0; i < numArgs; i++)
				{
					IntPtr currArg = Marshal.ReadIntPtr(argv, i * Marshal.SizeOf<nint>());
					result[i] = Marshal.PtrToStringUni(currArg);
				}

				return result;
			}
			finally
			{
				_ = LocalFree(argv);
			}
		}
	}
}