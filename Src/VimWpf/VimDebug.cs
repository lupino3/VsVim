using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Vim.UI.Wpf
{
    public static class VimDebug
    {
        public static void Assert([DoesNotReturnIf(false)] bool condition)
        {
            Debug.Assert(condition);
        }
    }
}
