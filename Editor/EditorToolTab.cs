using EvroDev.LevelEditorTool;
using EvroDev.LevelEditorTool.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EvroDev.LevelEditorTool.Tabs
{
    public abstract class EditorToolTab : Object
    {
        public abstract string Name { get; }
        public LevelEditorTool currentEditor;
        public EditorToolTab(LevelEditorTool currentEditor)
        {
            this.currentEditor = currentEditor;
        }

        public bool allowDrag { get => currentEditor.allowDrag; set => currentEditor.allowDrag = value; }

        public abstract void RenderTab();
        public abstract void OnEnable();
        public abstract void OnClickDown(Vector2 mousePosition);
        public abstract void OnClickUp(Vector2 mousePosition);
        public abstract void OnClickHold(Vector2 mousePosition);
        public abstract void OnMouseMove(Vector2 mousePosition);

    }
}
