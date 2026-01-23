using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TerminalController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text terminalText;
    [SerializeField] private ScrollRect scrollRect; // assign your Scroll View's ScrollRect here

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
    private Coroutine _scrollRoutine;

    private void OnEnable()
    {
        if (showBlinkingCursor)
            _cursorRoutine = StartCoroutine(CursorBlinkRoutine());
        Render();
    }

    private void OnDisable()
    {
        if (_cursorRoutine != null) StopCoroutine(_cursorRoutine);
        if (_typingRoutine != null) StopCoroutine(_typingRoutine);
        if (_scrollRoutine != null) StopCoroutine(_scrollRoutine);

        _cursorRoutine = null;
        _typingRoutine = null;
        _scrollRoutine = null;
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

    public bool IsTyping => _typingRoutine != null;

    public IEnumerator WaitForContinue(string prompt = "[PRESS ENTER]")
    {
        // show prompt on its own line
        if(!string.IsNullOrWhiteSpace(prompt))
            _buffer += "\n" + prompt;

        Render();

        while (!Input.GetKeyUp(KeyCode.Return) && !Input.GetKeyUp(KeyCode.Space) && !Input.GetKeyUp(KeyCode.Q) && !Input.GetKeyUp(KeyCode.Escape))
            yield return null;

        _buffer += "\n";
        Render();
    }

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

        terminalText.text = showBlinkingCursor
            ? _buffer + (_cursorVisible ? cursorGlyph : " ")
            : _buffer;

        // Force bottom scroll after layout updates
        RequestScrollToBottom();
    }

    private void RequestScrollToBottom()
    {
        if (!scrollRect) return;

        scrollRect.verticalNormalizedPosition = 0f;
    }
}
