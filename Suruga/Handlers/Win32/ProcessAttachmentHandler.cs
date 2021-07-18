using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

#pragma warning disable CA1401

// Source: https://tulisanlain.blogspot.com/2016/08/kill-child-process-when-parent-exit-c.html (This code is a bit simplified though, but it does the same thing as the source).
namespace Suruga.Handlers.Win32
{
    public class ProcessAttachmentHandler
    {
        // Windows will automatically close any open job handles when our process terminates. This can be verified by using SysInternals' Handle utility. When the job handle is closed, the child processes will be killed.
        public static readonly IntPtr JobHandle;

        static ProcessAttachmentHandler()
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
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string name);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AssignProcessToJobObject(IntPtr job, IntPtr process);

        /// <summary>
        /// Adds the specified process as a child of the parent application.
        /// </summary>
        /// <param name="process">The process that is going to be run as a child process of this program.</param>
        public void AddProcessAsChild(Process process)
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
    }
}
