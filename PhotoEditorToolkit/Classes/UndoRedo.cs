using System;
using System.Collections.Generic;
using System.Linq;

namespace PhotoEditorToolkit.Classes
{
    public class UndoRedo<T>
    {
        private readonly Stack<T> UndoStack;
        private readonly Stack<T> RedoStack;
        public event EventHandler<UndoRedoEventArgs<T>> UndoHappened;
        public event EventHandler<UndoRedoEventArgs<T>> RedoHappened;

        public T CurrentItem { get; private set; }
        public bool CanUndo => UndoStack.Count > 0;
        public bool CanRedo => RedoStack.Count > 0;
        public IReadOnlyList<T> UndoItems => UndoStack?.ToList();
        public IReadOnlyList<T> RedoItems => RedoStack?.ToList();
        public UndoRedo()
        {
            UndoStack = new Stack<T>();
            RedoStack = new Stack<T>();
        }

        public void Reset()
        {
            UndoStack.Clear();
            RedoStack.Clear();
        }

        public void Add(T item) 
        {
            UndoStack.Push(item);
            CurrentItem = item;
            RedoStack.Clear();
        }

        public void Undo()
        {
            if (!CanUndo) return;
            RedoStack.Push(CurrentItem);
            CurrentItem = UndoStack.Pop();
            CheckForFunc(CurrentItem);
            UndoHappened?.Invoke(this, new UndoRedoEventArgs<T>(CurrentItem));
        }

        public void Redo()
        {
            if (!CanRedo) return;
            UndoStack.Push(CurrentItem);
            CurrentItem = RedoStack.Pop();
            CheckForFunc(CurrentItem);
            RedoHappened?.Invoke(this, new UndoRedoEventArgs<T>(CurrentItem));
        }
        public IReadOnlyList<T> ReversedUndoItems()
        {
            var list = UndoStack?.ToList();
            list.Reverse();
            return list;
        }
        void CheckForFunc(T value)
        {
            if (value is Func<object> function)
                function();
        }
    }
    public class UndoRedoEventArgs<T> : EventArgs
    {
        public T CurrentItem { get; }
        public UndoRedoEventArgs(T currentItem) => CurrentItem = currentItem;
    }
}
