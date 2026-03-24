using System;
using System.Text;

namespace OpenAutoMapper.Generator.Helpers;

/// <summary>
/// Indentation-aware string builder for generating C# source code.
/// </summary>
internal sealed class SourceStringBuilder
{
    private readonly StringBuilder _sb = new StringBuilder();
    private int _indentLevel;
    private const string IndentUnit = "    ";

    public SourceStringBuilder()
    {
    }

    public SourceStringBuilder(int initialIndent)
    {
        _indentLevel = initialIndent;
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    public void AppendLine(string line)
    {
        if (string.IsNullOrEmpty(line))
        {
            _sb.AppendLine();
            return;
        }

        AppendIndent();
        _sb.AppendLine(line);
    }

    public void Append(string text)
    {
        _sb.Append(text);
    }

    public void AppendIndented(string text)
    {
        AppendIndent();
        _sb.Append(text);
    }

    public void OpenBrace()
    {
        AppendLine("{");
        _indentLevel++;
    }

    public void CloseBrace()
    {
        _indentLevel--;
        AppendLine("}");
    }

    public void CloseBraceWithSemicolon()
    {
        _indentLevel--;
        AppendLine("};");
    }

    public void Indent()
    {
        _indentLevel++;
    }

    public void Unindent()
    {
        if (_indentLevel > 0)
            _indentLevel--;
    }

    private void AppendIndent()
    {
        for (int i = 0; i < _indentLevel; i++)
        {
            _sb.Append(IndentUnit);
        }
    }

    public override string ToString()
    {
        return _sb.ToString();
    }
}
