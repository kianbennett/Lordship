using System.Collections;
using UnityEngine;
using TMPro;

public class TextDisplay : MonoBehaviour
{
    public enum State { Initialising, Idle, Busy }

    [SerializeField] private TMP_Text _displayText;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private float _shortWaitTime = 0.1f, _longWaitTime = 0.8f;
    private string _displayString;
    private WaitForSeconds _shortWait;
    private WaitForSeconds _longWait;
    private State _state = State.Initialising;

    public bool IsIdle { get { return _state == State.Idle; } }
    public bool IsBusy { get { return _state != State.Idle; } }

    private void Awake()
    {
        // _displayText = GetComponent<TMP_Text>();
        _shortWait = new WaitForSeconds(_shortWaitTime);
        _longWait = new WaitForSeconds(_longWaitTime);

        _displayText.text = string.Empty;
        _state = State.Idle;
    }

    private void Update() 
    {
        // Update panel height from text
        float height = Mathf.Lerp(_rectTransform.sizeDelta.y, _displayText.preferredHeight + 24, Time.deltaTime * 20);
        _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
    }

    // Coroutine that adds letters one by one to _displayText
    private IEnumerator DoShowText(string text)
    {
        int currentLetter = 0;
        char[] charArray = text.ToCharArray();

        while (currentLetter < charArray.Length)
        {
            _displayText.text += charArray[currentLetter++];
            yield return _shortWait;
        }

        _displayText.text += "\n";
        _displayString = _displayText.text;
        _state = State.Idle;
    }

    // Coroutine that adds blinking cursor to the end of _displayString when requiring input
    private IEnumerator DoAwaitingInput()
    {
        bool on = true;

        while (enabled)
        {
            _displayText.text = string.Format("{0}> {1}", _displayString, ( on ? "|" : " "));
            on = !on;
            yield return _longWait;
        }
    }

    // Coroutine that deletes the text backwards
    private IEnumerator DoClearText()
    {
        int currentLetter = 0;
        char[] charArray = _displayText.text.ToCharArray();

        while (currentLetter < charArray.Length)
        {
            if (currentLetter > 0 && charArray[currentLetter - 1] != '\n')
            {
                charArray[currentLetter - 1] = ' ';
            }

            if (charArray[currentLetter] != '\n')
            {
                charArray[currentLetter] = '_';
            }

            _displayText.text = charArray.ArrayToString();
            ++currentLetter;
            yield return null;
        }

        _displayString = string.Empty;
        _displayText.text = _displayString;
        _state = State.Idle;
    }

    // If the state is idle then start the coroutine that shows the text one letter at a time
    public void Display(string text)
    {
        if (_state == State.Idle)
        {
            StopAllCoroutines();
            _state = State.Busy;
            StartCoroutine(DoShowText(text));
        }
    }

    // Once the text has finished scrolling, start the coroutine that shows the blinking cursor
    public void ShowWaitingForInput()
    {
        if (_state == State.Idle)
        {
            StopAllCoroutines();
            StartCoroutine(DoAwaitingInput());
        }
    }

    // Start the coroutine that deletes the text backwards
    public void Clear()
    {
        // if (_state == State.Idle)
        // {
        //     StopAllCoroutines();
        //     _state = State.Busy;
        //     StartCoroutine(DoClearText());
        // }
        _displayText.text = "";
    }
}
