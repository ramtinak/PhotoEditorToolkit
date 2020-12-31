using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PhotoEditorToolkit.Classes;

namespace PhotoEditorToolkit.Controls
{
    partial class CustomCanvas
    {
        #region SmoothPathBuilder history 
        public UndoRedo<SmoothPathBuilder> PathBuilderHistory { get; } = new UndoRedo<SmoothPathBuilder>();

        void InitPathBuilderHistory()
        {
            PathBuilderHistory.UndoHappened += OnPathBuilderHistoryUndoRedoHappened;
            PathBuilderHistory.RedoHappened += OnPathBuilderHistoryUndoRedoHappened;
        }

        private void OnPathBuilderHistoryUndoRedoHappened(object sender, UndoRedoEventArgs<SmoothPathBuilder> e)
        {
            _needToRedrawInkSurface = true;
            _canvas.Invalidate();

            StrokesChanged?.Invoke(this, EventArgs.Empty);
        }

		#endregion

		#region Effect history

		#endregion
	}
}
