#nullable enable
using Microsoft.VisualStudio.Threading;
using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Vim.UI.Wpf;

namespace Vim.UI.Wpf.Implementation.Misc
{
    [Export(typeof(IJoinableTaskFactoryProvider))]
    internal sealed class VimJoinableTaskFactoryProvider : IJoinableTaskFactoryProvider
    {
        private static JoinableTaskFactory? s_joinableTaskFactory;

        internal static bool IsJoinableTaskFactoryCreated => s_joinableTaskFactory is object;

        public VimJoinableTaskFactoryProvider()
        {
        }

        internal JoinableTaskFactory GetOrCreateJoinableTaskFactory()
        {
            if (s_joinableTaskFactory is null)
            {
#if VIM_VSIX
                // See:
                // https://github.com/microsoft/vs-threading/blob/master/doc/library_with_jtf.md
                // https://github.com/microsoft/vs-threading/blob/master/doc/testing_vs.md
                s_joinableTaskFactory = Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory;
#else
                var context = new JoinableTaskContext(Thread.CurrentThread, SynchronizationContext.Current);
                s_joinableTaskFactory = new JoinableTaskFactory(context);
#endif
            }

            return s_joinableTaskFactory;
        }

        /// <summary>
        /// This is a test only helper that allows us to specify a custom <see cref="JoinableTaskFactory"/>
        /// </summary>
        internal static void SetCustomJoinableTaskFactory(JoinableTaskFactory joinableTaskFactory)
        {
            if (s_joinableTaskFactory is object)
            {
                throw new InvalidOperationException($"Cannot customize once there is a valid {typeof(JoinableTaskFactory).Name}");
            }

            s_joinableTaskFactory = joinableTaskFactory;
        }

#region IJoinableTaskFactoryProvider

        JoinableTaskFactory IJoinableTaskFactoryProvider.JoinableTaskFactory => GetOrCreateJoinableTaskFactory();

#endregion
    }
}
