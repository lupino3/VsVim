using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;

namespace Vim.UI.Wpf.Implementation.Misc
{
    /// <summary>
    /// Standard implementation of the IDisplayWindowBroker interface.  This acts as a single
    /// interface for the various completion window possibilities
    /// </summary>
    internal sealed class DisplayWindowBroker : IDisplayWindowBroker
    {
        private readonly ITextView _textView;
        private readonly JoinableTaskFactory _joinableTaskFactory;
        private readonly ICompletionBroker _completionBroker;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
#if VS_SPECIFIC_2017
        private readonly IQuickInfoBroker _quickInfoBroker;
#else
        private readonly IAsyncQuickInfoBroker _quickInfoBroker;
#endif

#if VS_SPECIFIC_2017
        internal DisplayWindowBroker(
            ITextView textView,
            JoinableTaskFactory joinableTaskFactory,
            ICompletionBroker completionBroker,
            ISignatureHelpBroker signatureHelpBroker,
            IQuickInfoBroker quickInfoBroker)
        {
            _textView = textView;
            _joinableTaskFactory = joinableTaskFactory;
            _completionBroker = completionBroker;
            _signatureHelpBroker = signatureHelpBroker;
            _quickInfoBroker = quickInfoBroker;
        }
#else
        internal DisplayWindowBroker(
            ITextView textView,
            JoinableTaskFactory joinableTaskFactory,
            ICompletionBroker completionBroker,
            ISignatureHelpBroker signatureHelpBroker,
            IAsyncQuickInfoBroker quickInfoBroker)
        {
            _textView = textView;
            _joinableTaskFactory = joinableTaskFactory;
            _completionBroker = completionBroker;
            _signatureHelpBroker = signatureHelpBroker;
            _quickInfoBroker = quickInfoBroker;
        }

#endif

        #region IDisplayWindowBroker

        bool IDisplayWindowBroker.IsCompletionActive
        {
            get { return _completionBroker.IsCompletionActive(_textView); }
        }

        bool IDisplayWindowBroker.IsQuickInfoActive
        {
            get { return _quickInfoBroker.IsQuickInfoActive(_textView); }
        }

        bool IDisplayWindowBroker.IsSignatureHelpActive
        {
            get { return _signatureHelpBroker.IsSignatureHelpActive(_textView); }
        }

        ITextView IDisplayWindowBroker.TextView
        {
            get { return _textView; }
        }

        void IDisplayWindowBroker.DismissDisplayWindows()
        {
            if (_completionBroker.IsCompletionActive(_textView))
            {
                _completionBroker.DismissAllSessions(_textView);
            }

            if (_signatureHelpBroker.IsSignatureHelpActive(_textView))
            {
                _signatureHelpBroker.DismissAllSessions(_textView);
            }

            if (_quickInfoBroker.IsQuickInfoActive(_textView))
            {
#if VS_SPECIFIC_2017
                foreach (var session in _quickInfoBroker.GetSessions(_textView))
                {
                    session.Dismiss();
                }
#else
                var session = _quickInfoBroker.GetSession(_textView);
                if (session is object)
                {
                    _joinableTaskFactory.Run(() => session.DismissAsync());
                }
#endif
            }
        }

#endregion
    }

    [Export(typeof(IDisplayWindowBrokerFactoryService))]
    internal sealed class DisplayWindowBrokerFactoryService : IDisplayWindowBrokerFactoryService
    {
        private static readonly object s_key = new object();

        private readonly IJoinableTaskFactoryProvider _joinableTaskFactoryProvider;
        private readonly ICompletionBroker _completionBroker;
        private readonly ISignatureHelpBroker _signatureHelpBroker;
#if VS_SPECIFIC_2017
        private readonly IQuickInfoBroker _quickInfoBroker;
#else
        private readonly IAsyncQuickInfoBroker _quickInfoBroker;
#endif

#if VS_SPECIFIC_2017
        [ImportingConstructor]
        internal DisplayWindowBrokerFactoryService(
            IJoinableTaskFactoryProvider joinableTaskFactoryProvider,
            ICompletionBroker completionBroker,
            ISignatureHelpBroker signatureHelpBroker,
            IQuickInfoBroker quickInfoBroker)
        {
            _joinableTaskFactoryProvider = joinableTaskFactoryProvider;
            _completionBroker = completionBroker;
            _signatureHelpBroker = signatureHelpBroker;
            _quickInfoBroker = quickInfoBroker;
        }
#else
        [ImportingConstructor]
        internal DisplayWindowBrokerFactoryService(
            IJoinableTaskFactoryProvider joinableTaskFactoryProvider,
            ICompletionBroker completionBroker,
            ISignatureHelpBroker signatureHelpBroker,
            IAsyncQuickInfoBroker quickInfoBroker)
        {
            _joinableTaskFactoryProvider = joinableTaskFactoryProvider;
            _completionBroker = completionBroker;
            _signatureHelpBroker = signatureHelpBroker;
            _quickInfoBroker = quickInfoBroker;
        }
#endif

        IDisplayWindowBroker IDisplayWindowBrokerFactoryService.GetDisplayWindowBroker(ITextView textView)
        {
            return textView.Properties.GetOrCreateSingletonProperty(
                s_key,
                () => new DisplayWindowBroker(
                        textView,
                        _joinableTaskFactoryProvider.JoinableTaskFactory,
                        _completionBroker,
                        _signatureHelpBroker,
                        _quickInfoBroker));
        }
    }
}
