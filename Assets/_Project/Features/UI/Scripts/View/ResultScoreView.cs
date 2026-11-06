using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class ResultScoreView : MonoBehaviour
{
    [SerializeField, Min(0.01f)] private float _digitDuration = 0.25f;
    [SerializeField, Min(1)] private int _digitChanges = 10;

    private Sequence _sequence;
    [SerializeField] private TMP_Text _scoreLabel;
    [SerializeField, Min(0f)] private float _minimumScoreScale = 1f;
    [SerializeField, Min(0f)] private float _maximumScoreScale = 1.5f;
    [SerializeField] private RectTransform _apple;
    [SerializeField, Min(0f)] private float _minimumAppleScale = 0.6f;
    [SerializeField, Min(0f)] private float _maximumAppleScale = 1f;
    [SerializeField] private int _minimumAppleScore = 500;
    [SerializeField] private int _maximumAppleScore = 2000;
    [SerializeField] private AudioSource _growthSoundSource;
    [SerializeField] private AudioClip _growthClip;
    [SerializeField, Min(0.01f), Tooltip("SEを再生するリンゴのスケール増分。小さいほど頻繁に再生します。")]
    private float _growthSoundScaleInterval = 0.2f;
    [SerializeField, Range(0f, 1f)] private float _growthSoundVolume = 1f;
    [SerializeField] private Image _characterImage;
    [SerializeField] private Sprite _sadFace;
    [SerializeField] private Sprite _normalFace;
    [SerializeField] private Sprite _happyFace;
    [SerializeField] private Image _backgroundImage;
    [SerializeField] private Sprite _badBackground;
    [SerializeField] private Sprite _normalBackground;
    [SerializeField] private Sprite _goodBackground;
    [SerializeField] private int _maximumSadScore = 500;
    [SerializeField] private int _minimumHappyScore = 2000;
    private Vector2 _baseAppleSize;
    private Vector2 _finalAppleSize;
    private Vector3 _baseScoreScale;
    private Vector3 _finalScoreScale;
    private int _finalScore;
    private string _score;
    private char[] _digits;

    public bool IsComplete { get; private set; }
    public bool IsGrowthSoundPlaying => _growthSoundSource.isPlaying;

    private void Awake()
    {
        _baseAppleSize = _apple.sizeDelta;
        _baseScoreScale = _scoreLabel.rectTransform.localScale;
    }

    public void Play(int score)
    {
        _finalScore = score;
        _score = score.ToString(CultureInfo.InvariantCulture);
        _digits = _score.ToCharArray();
        int firstDigit = _score[0] == '-' ? 1 : 0;
        for (int index = firstDigit; index < _digits.Length; index++)
            _digits[index] = '0';

        RenderDigits();
        _apple.sizeDelta = _baseAppleSize * _minimumAppleScale;
        _scoreLabel.rectTransform.localScale = _baseScoreScale * _minimumScoreScale;
        float scoreProgress = Mathf.InverseLerp(_minimumAppleScore, _maximumAppleScore, score);
        float finalAppleScale = Mathf.Lerp(_minimumAppleScale, _maximumAppleScale, scoreProgress);
        _finalAppleSize = _baseAppleSize * finalAppleScale;
        _finalScoreScale = _baseScoreScale * Mathf.Lerp(_minimumScoreScale, _maximumScoreScale, scoreProgress);
        int startScore = Mathf.Min(score, _minimumAppleScore);
        int growthTargetScore = Mathf.Min(score, _maximumAppleScore);
        ShowExpression(startScore);

        _sequence = DOTween.Sequence().SetUpdate(true);
        for (int index = _digits.Length - 1; index >= firstDigit; index--)
        {
            int digitIndex = index;
            _sequence.Append(DOVirtual.Int(0, _digitChanges, _digitDuration, step =>
            {
                _digits[digitIndex] = (char)('0' + (step + digitIndex) % 10);
                RenderDigits();
            }).SetEase(Ease.Linear));
            _sequence.AppendCallback(() =>
            {
                _digits[digitIndex] = _score[digitIndex];
                RenderDigits();
            });
        }

        float duration = _sequence.Duration();
        if (_finalAppleSize != _apple.sizeDelta)
        {
            Tweener growth = _apple.DOSizeDelta(_finalAppleSize, duration).SetEase(Ease.Linear);
            growth.OnUpdate(() => ShowExpression(Mathf.Lerp(startScore, growthTargetScore,
                growth.ElapsedPercentage())));
            _sequence.Insert(0f, growth);

            float scaleIncrease = finalAppleScale - _minimumAppleScale;
            float soundInterval = Mathf.Max(0.01f, _growthSoundScaleInterval);
            int soundCount = Mathf.FloorToInt(scaleIncrease / soundInterval + 0.00001f);
            for (int step = 1; step <= soundCount; step++)
            {
                float soundTime = duration * Mathf.Clamp01(step * soundInterval / scaleIncrease);
                _sequence.InsertCallback(soundTime,
                    () => _growthSoundSource.PlayOneShot(_growthClip, _growthSoundVolume));
            }
        }

        if (_finalScoreScale != _scoreLabel.rectTransform.localScale)
            _sequence.Insert(0f, _scoreLabel.rectTransform.DOScale(_finalScoreScale, duration).SetEase(Ease.Linear));

        _sequence.OnComplete(ShowFinalScore);
    }

    public void Complete()
    {
        _sequence.Kill();
        _growthSoundSource.Stop();
        ShowFinalScore();
    }

    private void ShowFinalScore()
    {
        IsComplete = true;
        _scoreLabel.text = _score;
        _apple.sizeDelta = _finalAppleSize;
        _scoreLabel.rectTransform.localScale = _finalScoreScale;
        ShowExpression(_finalScore);
    }

    private void ShowExpression(float score)
    {
        Sprite expression = _normalFace;
        Sprite background = _normalBackground;
        if (score <= _maximumSadScore)
        {
            expression = _sadFace;
            background = _badBackground;
        }
        else if (score >= _minimumHappyScore)
        {
            expression = _happyFace;
            background = _goodBackground;
        }

        if (_characterImage.sprite != expression)
            _characterImage.sprite = expression;
        if (_backgroundImage.sprite != background)
            _backgroundImage.sprite = background;
    }

    private void RenderDigits() => _scoreLabel.text = new string(_digits);

    private void OnDestroy() => _sequence?.Kill();
}
