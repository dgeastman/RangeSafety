using System.Text.RegularExpressions;
using UnityEngine;

namespace RangeSafety
{
    public interface IEditable
    {
        string text { get; set; }
    }

    public class EditableInt : IEditable
    {
        [Persistent]
        public int val;

        public bool parsed;
        [Persistent]
        public string _text;
        public virtual string text
        {
            get { return _text; }
            set
            {
                _text = value;
                _text = Regex.Replace(_text, @"[^\d+-]", ""); //throw away junk characters
                int parsedValue;
                parsed = int.TryParse(_text, out parsedValue);
                if (parsed) val = parsedValue;
            }
        }

        public EditableInt() : this(0) { }

        public EditableInt(int val)
        {
            this.val = val;
            _text = val.ToString();
        }

        public static implicit operator int(EditableInt x)
        {
            return x.val;
        }

        public static implicit operator EditableInt(int x)
        {
            return new EditableInt(x);
        }
    }
  
    public class EditableDouble : IEditable
    {
        [Persistent]
        public double _val;
        public virtual double val
        {
            get { return _val; }
            set
            {
                _val = value;
                _text = _val.ToString();
            }
        }

        public bool parsed;
        [Persistent]
        public string _text;
        public virtual string text
        {
            get { return _text; }
            set
            {
                _text = value;
                _text = Regex.Replace(_text, @"[^\d+-.]", ""); //throw away junk characters
                double parsedValue;
                parsed = double.TryParse(_text, out parsedValue);
                if (parsed) _val = parsedValue;
            }
        }

        public EditableDouble() : this(0) { }

        public EditableDouble(double val)
        {
            _val = val;
        }

        public static implicit operator EditableDouble(double x)
        {
            return new EditableDouble(x);
        }
    }

    public static class GUIUtils
    {
        static GUIStyle _labelNoWrap;
        public static GUIStyle LabelNoWrap
        {
            get
            {
                if (_labelNoWrap == null)
                {
                    _labelNoWrap = new GUIStyle(GUI.skin.label)
                    {
                        wordWrap = false,
                    };
                }
                return _labelNoWrap;
            }
        }

        public static void SimpleTextBox(string leftLabel, IEditable ed, string rightLabel = "", float width = 100, GUIStyle rightLabelStyle = null)
        {
            if (rightLabelStyle == null)
                rightLabelStyle = GUI.skin.label;
            GUILayout.BeginHorizontal();
            GUILayout.Label(leftLabel, rightLabelStyle, GUILayout.ExpandWidth(true));
            ed.text = GUILayout.TextField(ed.text, GUILayout.ExpandWidth(true), GUILayout.Width(width));
            GUILayout.Label(rightLabel, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }

        public static void SimpleLabel(string leftLabel, string rightLabel = "")
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(leftLabel, GUILayout.ExpandWidth(true));
            GUILayout.Label(rightLabel, GUILayout.ExpandWidth(false));
            GUILayout.EndHorizontal();
        }
    }
}
