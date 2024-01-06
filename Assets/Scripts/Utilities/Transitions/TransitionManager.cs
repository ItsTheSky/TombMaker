using Unity.VisualScripting;
using UnityEngine;

public class TransitionManager : MonoBehaviour
{
    
    public static TransitionManager instance;

    private GameObject _fadeFGPrefab;
    private GameObject _canvas;
    private string _lastDestinationScene;
    private bool _waitingForFadeIn;
    private Animator _animator;
    
    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            DontDestroyOnLoad(gameObject);
        }
    }
    
    private void CheckCanvas()
    {
        if (_canvas == null || _canvas.IsDestroyed())
        {
            _canvas = GameObject.Find("Foreground");
        }
    }
    
    private void CheckFadeFGPrefab()
    {
        if (_fadeFGPrefab == null || _fadeFGPrefab.IsDestroyed())
        {
            _fadeFGPrefab = Resources.Load<GameObject>("BlackForeground");
        }
    }
    
    public void FadeIn()
    {
        CheckCanvas();
        CheckFadeFGPrefab();
        
        var fadeFg = Instantiate(_fadeFGPrefab, _canvas.transform);
        _animator = fadeFg.GetComponent<Animator>();
        
        _waitingForFadeIn = true;
        _animator.Play("FadingFG");
    }

    private void Update()
    {
        if (_waitingForFadeIn && _animator.GetCurrentAnimatorStateInfo(0).IsName("FadingFG") 
                              && _animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            _waitingForFadeIn = false;
            FinishFadeIn();
        }
    }

    public void FinishFadeIn()
    {
        AsyncSceneLoader.instance.LoadScene(_lastDestinationScene);
    }

    public void SwitchScene0(string scene)
    {
        _lastDestinationScene = scene;
        FadeIn();
    }
    
    // ####################################################################################################
    
    public static void SwitchScene(string scene)
    {
        instance.SwitchScene0(scene);
    }

    public static void SwitchToLevelScene()
    {
        SwitchScene("LevelScene");
    }
}