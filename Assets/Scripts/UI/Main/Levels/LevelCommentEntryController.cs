using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class LevelCommentEntryController : MonoBehaviour
{
    [HideInInspector] public NewDB.LevelComment comment;
    
    public TMP_Text username;
    public GameObject pin;
    public TMP_Text text;
    public GameObject separator;

    public TMP_Text likes;

    public Button likeBtn;
    public Button dislikeBtn;
    public Button pinBtn;
    public Button deleteBtn;

    public async void Init(NewDB.LevelComment comment, bool isLast)
    {
        this.comment = comment;
        
        pin.SetActive(comment.pinned);
        separator.SetActive(!isLast);

        username.text = comment.username;
        text.text = comment.comment;
        
        likes.text = comment.likes.ToString();
        
        likeBtn.onClick.AddListener(Like);
        dislikeBtn.onClick.AddListener(Dislike);
        pinBtn.onClick.AddListener(Pin);
        deleteBtn.onClick.AddListener(Delete);

        if (comment.userId != (await NewDB.GetLoggedInUserId()))
        {
            deleteBtn.interactable = false;
            pinBtn.interactable = false;
        }
    }

    public void Like()
    {
        
    }
    
    public void Dislike()
    {
        
    }
    
    public async void Pin()
    {
        await NewDB.PinLevelComment(comment.id);
        
        pin.SetActive(!comment.pinned);
        comment.pinned = !comment.pinned;
    }
    
    public async void Delete()
    {
        await NewDB.DeleteLevelComment(comment.id);
        
        Destroy(gameObject);
    }
}