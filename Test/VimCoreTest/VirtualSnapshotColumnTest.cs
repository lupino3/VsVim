﻿using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Vim.EditorHost;
using Vim.Extensions;

namespace Vim.UnitTest
{
    public abstract class VirtualSnapshotColumnTest : VimTestBase
    {
        private ITextBuffer _textBuffer;

        private void Create(params string[] lines)
        {
            _textBuffer = CreateTextBuffer(lines);
        }

        private void CreateRaw(string content)
        {
            _textBuffer = CreateTextBufferRaw(content);
        }

        public sealed class CreateForColumnTest : VirtualSnapshotColumnTest
        {
            [WpfFact]
            public void Simple()
            {
                Create("cat", "dog");
                var column = VirtualSnapshotColumn.CreateForColumnNumber(_textBuffer.GetLine(0), 3);
                Assert.Equal(0, column.VirtualSpaces);
                Assert.True(column.Column.IsLineBreak);
            }
        }

        public sealed class VirtualStartPointTest : VirtualSnapshotColumnTest
        {
            [WpfFact]
            public void Middle()
            {
                Create("cat", "dog");
                var column = VirtualSnapshotColumn.CreateForColumnNumber(_textBuffer.GetLine(0), 1);
                Assert.Equal(1, column.VirtualStartPoint.Position);
                Assert.Equal(0, column.VirtualSpaces);
                Assert.False(column.IsInVirtualSpace);
            }

            [WpfFact]
            public void End()
            {
                Create("cat", "dog");
                var column = VirtualSnapshotColumn.CreateForColumnNumber(_textBuffer.GetLine(0), 3);
                Assert.Equal(3, column.VirtualStartPoint.Position);
                Assert.Equal(0, column.VirtualSpaces);
                Assert.False(column.IsInVirtualSpace);
            }

            [WpfFact]
            public void EndBig()
            {
                Create("cat", "dog");
                var column = VirtualSnapshotColumn.CreateForColumnNumber(_textBuffer.GetLine(0), 10);
                Assert.Equal(3, column.VirtualStartPoint.Position);
                Assert.Equal(7, column.VirtualSpaces);
                Assert.True(column.IsInVirtualSpace);
            }
        }
    }
}
