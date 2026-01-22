using System.Collections;
using TMPro;
using UnityEngine;

public class TerminalController : MonoBehaviour
{
    [SerializeField] private TMP_Text terminalText;

    [Header("Typing")]
    [SerializeField] private float charsPerSecond = 80f;
    [SerializeField] private bool enableTypewriter = true;

    [Header("Cursor")]
    [SerializeField] private bool showBlinkingCursor = true;
    [SerializeField] private string cursorGlyph = "_";   // try "_" if you prefer
    [SerializeField] private float cursorBlinkSeconds = 0.5f;

    private string _buffer = "";
    private bool _cursorVisible = true;

    private Coroutine _typingRoutine;
    private Coroutine _cursorRoutine;

    private void OnEnable()
    {
        if (showBlinkingCursor)
            _cursorRoutine = StartCoroutine(CursorBlinkRoutine());
        Render();
    }

    private void OnDisable()
    {
        if (_cursorRoutine != null) StopCoroutine(_cursorRoutine);
        _cursorRoutine = null;
    }

    // ---------------- Public API ----------------

    public void Clear()
    {
        StopTyping();
        _buffer = "";
        Render();
    }

    public void SetText(string text)
    {
        StopTyping();
        _buffer = text ?? "";
        Render();
    }

    public void AppendLineInstant(string line)
    {
        StopTyping();
        _buffer += (line ?? "") + "\n";
        Render();
    }

    /// <summary>
    /// Type a line with optional typewriter effect. Adds a newline at the end.
    /// </summary>
    public Coroutine TypeLine(string line)
    {
        StopTyping();
        _typingRoutine = StartCoroutine(TypeRoutine(line ?? ""));
        return _typingRoutine;
    }

    public void StopTyping()
    {
        if (_typingRoutine != null)
        {
            StopCoroutine(_typingRoutine);
            _typingRoutine = null;
        }
    }

    public IEnumerator WaitForContinue(string prompt = "[PRESS ENTER]")
    {
        // show prompt on its own line
        _buffer += "\n" + prompt;
        Render();

        while (!Input.GetKeyUp(KeyCode.Return) && !Input.GetKeyUp(KeyCode.Space))
            yield return null;

        _buffer += "\n";
        Render();
    }

    /// <summary>
    /// If you want: call this in Update when a key is pressed to instantly finish current typing.
    /// </summary>
    public bool IsTyping => _typingRoutine != null;

    // ---------------- Internals ----------------

    private IEnumerator TypeRoutine(string line)
    {
        if (!enableTypewriter)
        {
            _buffer += line + "\n";
            Render();
            _typingRoutine = null;
            yield break;
        }

        float delay = 1f / Mathf.Max(1f, charsPerSecond);

        for (int i = 0; i < line.Length; i++)
        {
            _buffer += line[i];
            Render();
            yield return new WaitForSeconds(delay);
        }

        _buffer += "\n";
        Render();
        _typingRoutine = null;
    }

    private IEnumerator CursorBlinkRoutine()
    {
        while (true)
        {
            _cursorVisible = !_cursorVisible;
            Render();
            yield return new WaitForSeconds(cursorBlinkSeconds);
        }
    }

    private void Render()
    {
        if (!terminalText) return;

        if (!showBlinkingCursor)
        {
            terminalText.text = _buffer;
            return;
        }

        // Cursor at end of buffer. Add a space before cursor if you want breathing room:
        // terminalText.text = _buffer + (_cursorVisible ? cursorGlyph : " ");
        terminalText.text = _buffer + (_cursorVisible ? cursorGlyph : " ");
    }
}
