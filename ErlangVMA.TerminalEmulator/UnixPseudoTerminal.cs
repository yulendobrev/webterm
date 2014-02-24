using System;
using Mono.Unix.Native;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Mono.Unix;
using System.Text;
using System.Collections.Generic;
using System.IO;

namespace ErlangVMA.TerminalEmulation
{
	public class UnixPseudoTerminal : IPseudoTerminal
	{
		public UnixPseudoTerminal()
		{
		}

		public PseudoTerminalStreams CreatePseudoTerminal(string executablePath, string[] arguments)
		{
			ProcessStartInfo startInfo = new ProcessStartInfo(
				                             "../ErlangVMA.TerminalEmulation.PseudoTerminalWrapper/bin/Debug/ErlangVMA.TerminalEmulation",
				                             executablePath + string.Join(" ", arguments));

			startInfo.RedirectStandardInput = true;
			startInfo.RedirectStandardOutput = true;
			startInfo.RedirectStandardError = true;
			startInfo.UseShellExecute = false;

			Process process = new Process();
			process.StartInfo = startInfo;

			process.Start();

			var streams = new PseudoTerminalStreams();
			streams.InputStream = process.StandardInput.BaseStream;
			streams.OutputStream = process.StandardOutput.BaseStream;

			return streams;
		}

		private static class NativeSyscall
		{
			public static readonly uint TIOCPTYGNAME = 0x40807453;
			public static readonly uint TIOCPTYGRANT = 0x20007454;
			public static readonly uint TIOCPTYUNLK = 0x20007452;
			
			public static int Ioctl(int fd, uint request)
			{
				int result = ioctl(fd, request);

				ThrowOnErrorResult(result);
				return result;
			}

			public static int Ioctl(int fd, uint request, StringBuilder sb)
			{
				int result = ioctl(fd, request, sb);

				ThrowOnErrorResult(result);
				return result;
			}

			public static int OpenPty(Termios termios, WinSize winsize, out int masterFd, out int slaveFd)
			{
				masterFd = -1;
				slaveFd = -1;

				int result = openpty(ref masterFd, ref slaveFd, null, ref termios, ref winsize);

				ThrowOnErrorResult(result);
				return result;
			}

			public static long ForkPty(Termios term, WinSize winsize, out int masterFd)
			{
				masterFd = -1;

				long childPid = forkpty(ref masterFd, null, ref term, ref winsize);

				return childPid;
			}

			public static int ExecPty(string path, string[] args, Termios term, WinSize winsize, out int masterFd)
			{
				masterFd = -1;
				int result = execpty(ref masterFd, path, args, ref term, ref winsize);

				ThrowOnErrorResult(result);
				return result;
			}

			private static void ThrowOnErrorResult(int result)
			{
				if (result == -1)
				{
					int errno = Marshal.GetLastWin32Error();
					throw new Exception(Errno.GetErrorMessage(errno));
				}
			}

			[StructLayout(LayoutKind.Sequential)]
			public struct Termios
			{
				public uint c_iflag;		/* input mode flags */
				public uint c_oflag;		/* output mode flags */
				public uint c_cflag;		/* control mode flags */
				public uint c_lflag;		/* local mode flags */
				public byte c_line;			/* line discipline */
				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
				public byte[] c_cc;		/* control characters */
				public uint c_ispeed;		/* input speed */
				public uint c_ospeed;		/* output speed */

//				public ushort c_iflag;		/* input mode flags */
//				public ushort c_oflag;		/* output mode flags */
//				public ushort c_cflag;		/* control mode flags */
//				public ushort c_lflag;		/* local mode flags */
//				public byte c_line;		/* line discipline */
//
//				[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
//				public byte[] c_cc;	/* control characters */
			}



			[StructLayout(LayoutKind.Sequential)]
			public struct WinSize
			{
				public ushort ws_row;
				public ushort ws_col;
				public ushort ws_xpixel;
				public ushort ws_ypixel;
			}

			public static long Fork()
			{
				long result = fork();
				if (result == -1L)
				{
					int errno = Marshal.GetLastWin32Error();
					throw new Exception(Errno.GetErrorMessage(errno));
				}
				return result;
			}

			[DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
			private static extern int ioctl(int fd, uint request);

			[DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
			private static extern int ioctl(int fd, uint request, StringBuilder sb);

//			int openpty(int *amaster, int *aslave, char *name,
//                   const struct termios *termp,
//                   const struct winsize *winp);
			[DllImport("libutil", EntryPoint = "openpty", SetLastError = true)]
			private static extern int openpty(ref int masterFd, ref int slaveFd, StringBuilder name,
			                                  ref Termios term,
			                                  ref WinSize winsize);

			[DllImport("libutil", EntryPoint = "forkpty", SetLastError = true)]
			private static extern long forkpty(ref int masterFd, StringBuilder name, ref Termios term, ref WinSize winsize);

			[DllImport("/home/jvdobrev/Дипломна работа/ErlangVMA/ErlangVMA/libpty.so", EntryPoint = "execpty", SetLastError = true)]
			private static extern int execpty(ref int masterFd, string path, string[] args, ref Termios term, ref WinSize winsize);

			[DllImport("libc", EntryPoint = "ioctl", SetLastError = true)]
			private static extern long fork();
		}

		private static class Errno
		{
			public static readonly int EPERM   =     1;  /* Operation not permitted */
			public static readonly int ENOENT  =     2;  /* No such file or directory */
			public static readonly int ESRCH   =     3;  /* No such process */
			public static readonly int EINTR   =     4;  /* Interrupted system call */
			public static readonly int EIO     =     5;  /* I/O error */
			public static readonly int ENXIO   =     6;  /* No such device or address */
			public static readonly int E2BIG   =     7;  /* Argument list too long */
			public static readonly int ENOEXEC =     8;  /* Exec format error */
			public static readonly int EBADF   =     9;  /* Bad file number */
			public static readonly int ECHILD  =    10;  /* No child processes */
			public static readonly int EAGAIN  =    11;  /* Try again */
			public static readonly int ENOMEM  =    12;  /* Out of memory */
			public static readonly int EACCES  =    13;  /* Permission denied */
			public static readonly int EFAULT  =    14;  /* Bad address */
			public static readonly int ENOTBLK =    15;  /* Block device required */
			public static readonly int EBUSY   =    16;  /* Device or resource busy */
			public static readonly int EEXIST  =    17;  /* File exists */
			public static readonly int EXDEV   =    18;  /* Cross-device link */
			public static readonly int ENODEV  =    19;  /* No such device */
			public static readonly int ENOTDIR =    20;  /* Not a directory */
			public static readonly int EISDIR  =    21;  /* Is a directory */
			public static readonly int EINVAL  =    22;  /* Invalid argument */
			public static readonly int ENFILE  =    23;  /* File table overflow */
			public static readonly int EMFILE  =    24;  /* Too many open files */
			public static readonly int ENOTTY  =    25;  /* Not a typewriter */
			public static readonly int ETXTBSY =    26;  /* Text file busy */
			public static readonly int EFBIG   =    27;  /* File too large */
			public static readonly int ENOSPC  =    28;  /* No space left on device */
			public static readonly int ESPIPE  =    29;  /* Illegal seek */
			public static readonly int EROFS   =    30;  /* Read-only file system */
			public static readonly int EMLINK  =    31;  /* Too many links */
			public static readonly int EPIPE   =    32;  /* Broken pipe */
			public static readonly int EDOM    =    33;  /* Math argument out of domain of func */
			public static readonly int ERANGE  =    34;  /* Math result not representable */

			public static readonly int EDEADLK    = 35; /* Resource deadlock would occur */
            public static readonly int ENAMETOOLONG =  36; /* File name too long */
            public static readonly int ENOLCK     = 37;  /* No record locks available */
            public static readonly int ENOSYS     = 38;  /* Function not implemented */
			public static readonly int ENOTEMPTY  = 39;  /* Directory not empty */
			public static readonly int ELOOP      = 40;  /* Too many symbolic links encountered */
			public static readonly int EWOULDBLOCK = EAGAIN;  /* Operation would block */
            public static readonly int ENOMSG     = 42;  /* No message of desired type */
            public static readonly int EIDRM      = 43;  /* Identifier removed */
            public static readonly int ECHRNG     = 44;  /* Channel number out of range */
            public static readonly int EL2NSYNC   = 45;  /* Level 2 not synchronized */
            public static readonly int EL3HLT     = 46;  /* Level 3 halted */
            public static readonly int EL3RST     = 47;  /* Level 3 reset */
            public static readonly int ELNRNG     = 48;  /* Link number out of range */
            public static readonly int EUNATCH    = 49;  /* Protocol driver not attached */
            public static readonly int ENOCSI     = 50;  /* No CSI structure available */
            public static readonly int EL2HLT     = 51;  /* Level 2 halted */
            public static readonly int EBADE      = 52;  /* Invalid exchange */
            public static readonly int EBADR      = 53;  /* Invalid request descriptor */
            public static readonly int EXFULL     = 54;  /* Exchange full */
            public static readonly int ENOANO     = 55;  /* No anode */
            public static readonly int EBADRQC    = 56;  /* Invalid request code */
            public static readonly int EBADSLT    = 57;  /* Invalid slot */

			public static readonly int EDEADLOCK  = EDEADLK;

            public static readonly int EBFONT     = 59;  /* Bad font file format */
            public static readonly int ENOSTR     = 60;  /* Device not a stream */
            public static readonly int ENODATA    = 61;  /* No data available */
            public static readonly int ETIME      = 62;  /* Timer expired */
            public static readonly int ENOSR      = 63;  /* Out of streams resources */
			public static readonly int ENONET     = 64;  /* Machine is not on the network */
            public static readonly int ENOPKG     = 65;  /* Package not installed */
            public static readonly int EREMOTE    = 66;  /* Object is remote */
            public static readonly int ENOLINK    = 67;  /* Link has been severed */
            public static readonly int EADV       = 68;  /* Advertise error */
            public static readonly int ESRMNT     = 69;  /* Srmount error */
            public static readonly int ECOMM      = 70;  /* Communication error on send */
            public static readonly int EPROTO     = 71;  /* Protocol error */
            public static readonly int EMULTIHOP  = 72;  /* Multihop attempted */
            public static readonly int EDOTDOT    = 73;  /* RFS specific error */
            public static readonly int EBADMSG    = 74;  /* Not a data message */
            public static readonly int EOVERFLOW  = 75;  /* Value too large for defined data type */
            public static readonly int ENOTUNIQ   = 76;  /* Name not unique on network */
            public static readonly int EBADFD     = 77;  /* File descriptor in bad state */
            public static readonly int EREMCHG    = 78;  /* Remote address changed */
            public static readonly int ELIBACC    = 79;  /* Can not access a needed shared library */
            public static readonly int ELIBBAD    = 80;  /* Accessing a corrupted shared library */
            public static readonly int ELIBSCN    = 81;  /* .lib section in a.out corrupted */
            public static readonly int ELIBMAX    = 82;  /* Attempting to link in too many shared libraries */
            public static readonly int ELIBEXEC   = 83;  /* Cannot exec a shared library directly */
            public static readonly int EILSEQ     = 84;  /* Illegal byte sequence */
            public static readonly int ERESTART   = 85;  /* Interrupted system call should be restarted */
            public static readonly int ESTRPIPE   = 86;  /* Streams pipe error */
            public static readonly int EUSERS     = 87;  /* Too many users */
            public static readonly int ENOTSOCK   = 88;  /* Socket operation on non-socket */
            public static readonly int EDESTADDRREQ   = 89;  /* Destination address required */
			public static readonly int EMSGSIZE   = 90;  /* Message too long */ 
            public static readonly int EPROTOTYPE = 91;  /* Protocol wrong type for socket */
            public static readonly int ENOPROTOOPT = 92;  /* Protocol not available */
            public static readonly int EPROTONOSUPPORT = 93;  /* Protocol not supported */
            public static readonly int ESOCKTNOSUPPORT = 94;  /* Socket type not supported */
            public static readonly int EOPNOTSUPP  = 95;  /* Operation not supported on transport endpoint */
            public static readonly int EPFNOSUPPORT   = 96;  /* Protocol family not supported */
            public static readonly int EAFNOSUPPORT   = 97;  /* Address family not supported by protocol */
            public static readonly int EADDRINUSE = 98;  /* Address already in use */
            public static readonly int EADDRNOTAVAIL  = 99;  /* Cannot assign requested address */
            public static readonly int ENETDOWN   = 100; /* Network is down */ 
            public static readonly int ENETUNREACH = 101; /* Network is unreachable */
            public static readonly int ENETRESET  = 102; /* Network dropped connection because of reset */
            public static readonly int ECONNABORTED  = 103; /* Software caused connection abort */
            public static readonly int ECONNRESET = 104; /* Connection reset by peer */ 
			public static readonly int ENOBUFS    = 105; /* No buffer space available */
			public static readonly int EISCONN    = 106; /* Transport endpoint is already connected */
			public static readonly int ENOTCONN   = 107; /* Transport endpoint is not connected */
			public static readonly int ESHUTDOWN  = 108; /* Cannot send after transport endpoint shutdown */
            public static readonly int ETOOMANYREFS   = 109; /* Too many references: cannot splice */
            public static readonly int ETIMEDOUT  = 110; /* Connection timed out */
            public static readonly int ECONNREFUSED   = 111; /* Connection refused */
            public static readonly int EHOSTDOWN  = 112; /* Host is down */ 
            public static readonly int EHOSTUNREACH   = 113; /* No route to host */ 
            public static readonly int EALREADY   = 114; /* Operation already in progress */
            public static readonly int EINPROGRESS= 115; /* Operation now in progress */
            public static readonly int ESTALE     = 116; /* Stale NFS file handle */
            public static readonly int EUCLEAN    = 117; /* Structure needs cleaning */
            public static readonly int ENOTNAM    = 118; /* Not a XENIX named type file */ 
            public static readonly int ENAVAIL    = 119; /* No XENIX semaphores available */
            public static readonly int EISNAM     = 120; /* Is a named type file */ 
            public static readonly int EREMOTEIO  = 121; /* Remote I/O error */
            public static readonly int EDQUOT     = 122; /* Quota exceeded */
                
            public static readonly int ENOMEDIUM  = 123; /* No medium found */
            public static readonly int EMEDIUMTYPE= 124; /* Wrong medium type */ 
            public static readonly int ECANCELED  = 125; /* Operation Canceled */
            public static readonly int ENOKEY     = 126; /* Required key not available */
            public static readonly int EKEYEXPIRED= 127; /* Key has expired */
            public static readonly int EKEYREVOKED= 128; /* Key has been revoked */
            public static readonly int EKEYREJECTED   = 129; /* Key was rejected by service */
                
            /* for robust mutexes */
            public static readonly int EOWNERDEAD = 130; /* Owner died */ 
            public static readonly int ENOTRECOVERABLE = 131; /* State not recoverable */

			public static readonly Dictionary<int, string> messages = new Dictionary<int, string>
			{
{ EPERM, "Operation not permitted" },
{ ENOENT, "No such file or directory" },
{ ESRCH, "No such process" },
{ EINTR, "Interrupted system call" },
{ EIO, "I/O error" },
{ ENXIO, "No such device or address" },
{ E2BIG, "Argument list too long" },
{ ENOEXEC, "Exec format error" },
{ EBADF, "Bad file number" },
{ ECHILD, "No child processes" },
{ EAGAIN, "Try again" },
{ ENOMEM, "Out of memory" },
{ EACCES, "Permission denied" },
{ EFAULT, "Bad address" },
{ ENOTBLK, "Block device required" },
{ EBUSY, "Device or resource busy" },
{ EEXIST, "File exists" },
{ EXDEV, "Cross-device link" },
{ ENODEV, "No such device" },
{ ENOTDIR, "Not a directory" },
{ EISDIR, "Is a directory" },
{ EINVAL, "Invalid argument" },
{ ENFILE, "File table overflow" },
{ EMFILE, "Too many open files" },
{ ENOTTY, "Not a typewriter" },
{ ETXTBSY, "Text file busy" },
{ EFBIG, "File too large" },
{ ENOSPC, "No space left on device" },
{ ESPIPE, "Illegal seek" },
{ EROFS, "Read-only file system" },
{ EMLINK, "Too many links" },
{ EPIPE, "Broken pipe" },
{ EDOM, "Math argument out of domain of func" },
{ ERANGE, "Math result not representable" },
{ EDEADLK,  "Resource deadlock would occur" },
{ ENAMETOOLONG, "File name too long" },
{ ENOLCK, "No record locks available" },
{ ENOSYS, "Function not implemented" },
{ ENOTEMPTY, "Directory not empty" },
{ ELOOP, "Too many symbolic links encountered" },
//{ EWOULDBLOCK, "Operation would block" },
{ ENOMSG, "No message of desired type" },
//{ EIDRM, "Identifier removed" },
//{ ECHRNG, "Channel number out of range" },
//{ EL2NSYNC, "Level 2 not synchronized" },
//{ EL3HLT, "Level 3 halted" },
//{ EL3RST, "Level 3 reset" },
//{ ELNRNG, "Link number out of range" },
//{ EUNATCH, "Protocol driver not attached" },
//{ ENOCSI, "No CSI structure available" },
//{ EL2HLT, "Level 2 halted" },
//{ EBADE, "Invalid exchange" },
//{ EBADR, "Invalid request descriptor" },
//{ EXFULL, "Exchange full" },
//{ ENOANO, "No anode" },
//{ EBADRQC, "Invalid request code" },
//{ EBADSLT, "Invalid slot" },
//{ EBFONT, "Bad font file format" },
//{ ENOSTR, "Device not a stream" },
//{ ENODATA, "No data available" },
//{ ETIME, "Timer expired" },
//{ ENOSR, "Out of streams resources" },
//{ ENONET, "Machine is not on the network" },
//{ ENOPKG, "Package not installed" },
//{ EREMOTE, "Object is remote" },
//{ ENOLINK, "Link has been severed" },
//{ EADV, "Advertise error" },
//{ ESRMNT, "Srmount error" },
//{ ECOMM, "Communication error on send" },
//{ EPROTO, "Protocol error" },
//{ EMULTIHOP, "Multihop attempted" },
//{ EDOTDOT, "RFS specific error" },
//{ EBADMSG, "Not a data message" },
//{ EOVERFLOW, "Value too large for defined data type" },
//{ ENOTUNIQ, "Name not unique on network" },
//{ EBADFD, "File descriptor in bad state" },
//{ EREMCHG, "Remote address changed" },
//{ ELIBACC, "Can not access a needed shared library" },
//{ ELIBBAD, "Accessing a corrupted shared library" },
//{ ELIBSCN, ".lib section in a.out corrupted" },
//{ ELIBMAX, "Attempting to link in too many shared libraries" },
//{ ELIBEXEC, "Cannot exec a shared library directly" },
//{ EILSEQ, "Illegal byte sequence" },
//{ ERESTART, "Interrupted system call should be restarted" },
//{ ESTRPIPE, "Streams pipe error" },
//{ EUSERS, "Too many users" },
//{ ENOTSOCK, "Socket operation on non-socket" },
//{ EDESTADDRREQ, "Destination address required" },
//{ EMSGSIZE, "Message too long" },
//{ EPROTOTYPE, "Protocol wrong type for socket" },
//{ ENOPROTOOPT, "Protocol not available" },
//{ EPROTONOSUPPORT, "Protocol not supported" },
//{ ESOCKTNOSUPPORT, "Socket type not supported" },
//{ EOPNOTSUPP, "Operation not supported on transport endpoint" },
//{ EPFNOSUPPORT, "Protocol family not supported" },
//{ EAFNOSUPPORT, "Address family not supported by protocol" },
//{ EADDRINUSE, "Address already in use" },
//{ EADDRNOTAVAIL, "Cannot assign requested address" },
//{ ENETDOWN, "Network is down" },
//{ ENETUNREACH, "Network is unreachable" },
//{ ENETRESET, "Network dropped connection because of reset" },
//{ ECONNABORTED, "Software caused connection abort" },
//{ ECONNRESET, "Connection reset by peer" },
//{ ENOBUFS, "No buffer space available" },
//{ EISCONN, "Transport endpoint is already connected" },
//{ ENOTCONN, "Transport endpoint is not connected" },
//{ ESHUTDOWN, "Cannot send after transport endpoint shutdown" },
//{ ETOOMANYREFS,   "Too many references: cannot splice" },
//{ ETIMEDOUT,   "Connection timed out" },
//{ ECONNREFUSED, "Connection refused" },
//{ EHOSTDOWN, "Host is down" },
//{ EHOSTUNREACH,   "No route to host" },
//{ ECONNREFUSED, "Connection refused" },
//{ EHOSTDOWN, "Host is down" },
//{ EHOSTUNREACH,   "No route to host" },
//{ EALREADY, "Operation already in progress" },
//{ EINPROGRESS, "Operation now in progress" },
//{ ESTALE, "Stale NFS file handle" },
//{ EUCLEAN, "Structure needs cleaning" },
//{ ENOTNAM, "Not a XENIX named type file" },
//{ ENAVAIL, "No XENIX semaphores available" },
//{ EISNAM, "Is a named type file" },
//{ EREMOTEIO, "Remote I/O error" },
//{ EDQUOT, "Quota exceeded" },
//{ ENOMEDIUM, "No medium found" },
//{ EMEDIUMTYPE, "Wrong medium type" },
//{ ECANCELED, "Operation Canceled" },
//{ ENOKEY, "Required key not available" },
//{ EKEYEXPIRED, "Key has expired" },
//{ EKEYREVOKED, "Key has been revoked" },
//{ EKEYREJECTED, "Key was rejected by service" },
//{ EOWNERDEAD, "Owner died" },
//{ ENOTRECOVERABLE, "State not recoverable" },
			};

			public static string GetErrorMessage(int errno)
			{
				string message;
				if (!messages.TryGetValue(errno, out message))
				{
					message = string.Format("Unknown error {0} has occurred", errno);
				}

				return message;
			}
		}
	}
}
