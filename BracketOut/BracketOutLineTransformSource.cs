//***************************************************************************
// 
//    Copyright (c) Microsoft Corporation. All rights reserved.
//    This code is licensed under the Visual Studio SDK license terms.
//    THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
//    ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
//    IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
//    PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//***************************************************************************

namespace BracketOut {
    using System;
    using Microsoft.VisualStudio.Text;
    using Microsoft.VisualStudio.Text.Editor;
    using Microsoft.VisualStudio.Text.Formatting;

    public class BraketOutLineTransformSource : ILineTransformSource {
        #region private members
        IWpfTextView _textView;

        private BraketOutLineTransformSource(IWpfTextView textView) {
            _textView = textView;

            //Sync to changing the caret position. 
            _textView.Caret.PositionChanged += OnCaretChanged;
        }

        /// <summary>
        /// The caret has been moved by the user. Update line transformations to reflect new position
        /// </summary>
        private void OnCaretChanged(object sender, CaretPositionChangedEventArgs args) {
            //Did the caret line number change?
            SnapshotPoint oldPosition = args.OldPosition.BufferPosition;
            SnapshotPoint newPosition = args.NewPosition.BufferPosition;

            if (_textView.TextSnapshot.GetLineNumberFromPosition(newPosition) != _textView.TextSnapshot.GetLineNumberFromPosition(oldPosition)) {
                ITextViewLine line = _textView.Caret.ContainingTextViewLine;
            }
        }
        #endregion

        /// <summary>
        /// Static class factory that ensures a single instance of the line transform source/view.
        /// </summary>
        public static BraketOutLineTransformSource Create(IWpfTextView view) {
            return view.Properties.GetOrCreateSingletonProperty(delegate { return new BraketOutLineTransformSource(view); });
        }

        public LineTransform GetLineTransform(ITextViewLine line, double yPosition, ViewRelativePosition placement) {
            //Vertically compress lines that are far from the caret (based on buffer lines, not view lines).
            int caretLineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(_textView.Caret.Position.BufferPosition);
            int lineNumber = _textView.TextSnapshot.GetLineNumberFromPosition(line.Start);

            var extent = line.Extent;
            var bufferGraph = _textView.BufferGraph;
            string text;
            try {
                var collection = bufferGraph.MapDownToSnapshot(extent, SpanTrackingMode.EdgeInclusive, _textView.TextSnapshot);
                var span = new SnapshotSpan(collection[0].Start, collection[collection.Count - 1].End);
                //text = span.ToString();
                text = span.GetText();
            }
            catch {
                text = null;
                return new LineTransform(0.0, 0.0, 1);
            }

            if (text.Trim() == "{") {
                return new LineTransform(0.0, 0.0, .1);
            }
            else if (text.Trim() == "}") {
                return new LineTransform(0.0, 0.0, .2);
            }

            return new LineTransform(0.0, 0.0, 1);
        }
    }
}
