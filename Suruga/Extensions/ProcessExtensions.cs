using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

#pragma warning disable SA1602

// Source: https://tulisanlain.blogspot.com/2016/08/kill-child-process-when-parent-exit-c.html
namespace Suruga.Extensions
{
    public enum JobObjectInfoType
    {
        AssociateCompletionPortInformation = 7,
        BasicLimitInformation = 2,
        BasicUIRestrictions = 4,
        EndOfJobTimeInformation = 6,
        ExtendedLimitInformation = 9,
        SecurityLimitInformation = 5,
        GroupInformation = 11,
    }

    [Flags]
    public enum JOBOBJECTLIMIT : uint
    {
        /// <summary>
        /// Causes all processes associated with the job to terminate when the last handle to the job is closed. This limit requires use of a <see cref="JOBOBJECT_EXTENDED_LIMIT_INFORMATION"/> structure.
        /// </summary>
        JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x00002000,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_BASIC_LIMIT_INFORMATION
    {
        public long PerProcessUserTimeLimit;
        public long PerJobUserTimeLimit;
        public JOBOBJECTLIMIT LimitFlags;
        public UIntPtr MinimumWorkingSetSize;
        public UIntPtr MaximumWorkingSetSize;
        public uint ActiveProcessLimit;
        public long Affinity;
        public uint PriorityClass;
        public uint SchedulingClass;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct IO_COUNTERS
    {
        public ulong ReadOperationCount;
        public ulong WriteOperationCount;
        public ulong OtherOperationCount;
        public ulong ReadTransferCount;
        public ulong WriteTransferCount;
        public ulong OtherTransferCount;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct JOBOBJECT_EXTENDED_LIMIT_INFORMATION
    {
        public JOBOBJECT_BASIC_LIMIT_INFORMATION BasicLimitInformation;
        public IO_COUNTERS IoInfo;
        public UIntPtr ProcessMemoryLimit;
        public UIntPtr JobMemoryLimit;
        public UIntPtr PeakProcessMemoryUsed;
        public UIntPtr PeakJobMemoryUsed;
    }

    public class ProcessExtensions
    {
        // Windows will automatically close any open job handles when our process terminates. This can be verified by using SysInternals' Handle utility. When the job handle is closed, the child processes will be killed.
        private static readonly IntPtr JobHandle;

        static ProcessExtensions()
        {
            // This feature requires Windows 8 or later. To support Windows 7 requires registry settings to be added if you are using Visual Studio plus an app.manifest change.
            // Examples: http://stackoverflow.com/a/4232259/386091 & http://stackoverflow.com/a/9507862/386091
            if (Environment.OSVersion.Version < new Version(6, 2))
            {
                return;
            }

            // The job name is optional (and can be null) but it helps with diagnostics. If it's not null, it has to be unique. Use SysInternals' Handle command-line utility: handle -a ChildProcessTracker.
            string jobName = "ChildProcessTracker" + Environment.ProcessId;
            JobHandle = CreateJobObject(IntPtr.Zero, jobName);

            JOBOBJECT_BASIC_LIMIT_INFORMATION info = new JOBOBJECT_BASIC_LIMIT_INFORMATION
            {
                // This is the key flag. When our process is killed, Windows will automatically close the job handle, and when that happens, we want the child processes to be killed, too.
                LimitFlags = JOBOBJECTLIMIT.JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE,
            };

            JOBOBJECT_EXTENDED_LIMIT_INFORMATION extendedInfo = new JOBOBJECT_EXTENDED_LIMIT_INFORMATION
            {
                BasicLimitInformation = info,
            };

            int length = Marshal.SizeOf(typeof(JOBOBJECT_EXTENDED_LIMIT_INFORMATION));
            IntPtr extendedInfoPtr = Marshal.AllocHGlobal(length);

            try
            {
                Marshal.StructureToPtr(extendedInfo, extendedInfoPtr, false);

                if (!SetInformationJobObject(JobHandle, JobObjectInfoType.ExtendedLimitInformation, extendedInfoPtr, (uint)length))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                Marshal.FreeHGlobal(extendedInfoPtr);
            }
        }

        /// <summary>
        /// Adds the specified process as a child of the parent application.
        /// </summary>
        /// <param name="process">The process that is going to be run as a child process of this program.</param>
        public static void AddProcessAsChild(Process process)
        {
            if (JobHandle != IntPtr.Zero)
            {
                bool success = AssignProcessToJobObject(JobHandle, process.Handle);

                if (!success)
                {
                    throw new Win32Exception("Failed to add Lavalink as a child of this process.");
                }
            }
        }

        public static string EncodeParameterArgument(string original)
        {
            if (string.IsNullOrEmpty(original))
            {
                return original;
            }

            string value = Regex.Replace(original, @"(\\*)" + "\"", @"$1\$0");
            value = Regex.Replace(value, @"^(.*\s.*?)(\\*)$", "\"$1$2$2\"");

            return value;
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

        [DllImport("kernel32.dll")]
        private static extern bool SetInformationJobObject(IntPtr job, JobObjectInfoType infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);
    }
}