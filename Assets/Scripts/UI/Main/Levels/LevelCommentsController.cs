using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelCommentsController : MonoBehaviour
{

    public GameObject commentPrefab;
    public GameObject commentsContainer;

    public TMP_Text pageInfos;
    public GameObject previousButton;
    public GameObject nextButton;
    public TMP_Text loadingText;
    
    // Comment input
    public TMP_InputField commentInput;
    public GameObject commentInputPanel;
    public Button postCommentButton;

    [HideInInspector] public LevelInfoController controller;

    public void Init(LevelInfoController controller)
    {
        this.controller = controller;
        
        loadingText.text = "Loading comments...";
        loadingText.gameObject.SetActive(true);
        
        previousButton.SetActive(false);
        nextButton.SetActive(false);
        previousButton.GetComponent<Button>().onClick.AddListener(PreviousPage);
        nextButton.GetComponent<Button>().onClick.AddListener(NextPage);

        pageInfos.text = "loading ...";

        LoadCommentsAsync(controller.level.id, 1);
    }
    
    private int _currentPage = 1;

    public async void LoadCommentsAsync(int levelId, int page)
    {
        foreach (Transform child in commentsContainer.transform)
            Destroy(child.gameObject);
     
        var response = await NewDB.GetLevelComments(levelId, page - 1);
        if (response.status != "ok")
        {
            loadingText.text = "Error while loading comments: " + response.message;
            pageInfos.text = "";
            return;
        }

        var comments = response.GetList<NewDB.LevelComment>("comments");
        var pagination = response.Get<NewDB.Pagination>("pagination");
        _currentPage = page;
        if (comments.Count == 0)
        {
            loadingText.text = "No comments yet.";
            pageInfos.text = "";
            nextButton.SetActive(false);
            previousButton.SetActive(false);
            return;
        }
        
        loadingText.gameObject.SetActive(false);
        pageInfos.text = "Showing " + comments.Count + " (page " + pagination.page + "/" + pagination.pages + ")";
        
        nextButton.SetActive(pagination.hasNext);
        previousButton.SetActive(pagination.hasPrevious);
        
        for (int i = 0; i < comments.Count; i++)
        {
            var comment = comments[i];
            var entry = Instantiate(commentPrefab, commentsContainer.transform);
            entry.GetComponent<LevelCommentEntryController>().Init(comment, i == comments.Count - 1);
        }
    }

    private void NextPage()
    {
        LoadCommentsAsync(controller.level.id, _currentPage + 1);
    }
    
    private void PreviousPage()
    {
        LoadCommentsAsync(controller.level.id, _currentPage - 1);
    }

    public async void PostComment()
    {
        print("called");
        var text = commentInput.text;
        if (text.Length <= 0 || text.Length > 100)
        {
            print("denied");
            AlertsManager.ShowAlert("Empty or too long comment.", AlertsManager.AlertType.Error);
            return;
        }
        postCommentButton.interactable = false;
        
        TMP_Text buttonText = postCommentButton.GetComponentInChildren<TMP_Text>();
        var oldText = buttonText.text;
        buttonText.text = "Posting...";
        
        var response = await NewDB.PostComment(controller.level.id, text);
        if (response.status != "ok")
            AlertsManager.ShowAlert(response.message, AlertsManager.AlertType.Error);
        else
        {
            AlertsManager.ShowAlert("Comment posted!");
            HideCommentInput();
            LoadCommentsAsync(controller.level.id, _currentPage);
        }
        
        buttonText.text = oldText;
        postCommentButton.interactable = true;
    }
    
    public void ShowCommentInput()
    {
        commentInput.text = "";
        commentInputPanel.SetActive(true);
    }
    
    public void HideCommentInput()
    {
        commentInputPanel.SetActive(false);
    }
} 