using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UsersController : MonoBehaviour
{
    
    [Header("Prefabs & Containers")]
    public Transform usersContainer;
    public Transform sortingMethodContainer;
    public GameObject sortingMethodButtonPrefab;
    public GameObject userEntryPrefab;
    
    [Header("Columns 1 to 3")]
    public Image column1Icon;
    public Image column2Icon;
    public Image column3Icon;
    
    [Header("Loading & error")]
    public Image loadingIcon;
    public TMP_Text errorText;
    
    [Header("Pagination")]
    public TMP_Text paginationText;
    public Button nextPage;
    public Button previousPage;

    [Header("Sorting Methods")] 
    public Sprite starIcon;
    public Sprite starsIcon;
    public Sprite levelIcon;
    public Sprite shieldIcon;
    public Sprite dateIcon;
    public Sprite progressLevelIcon;
    public Sprite progressDotsIcon;

    private int _currentPage = 1;
    private SortingMethod _sortingMethod;
    private Dictionary<SortingMethod, Button> _sortingMethodButtons = new ();

    private void Start()
    {
        var sortingMethods = new SortingMethod[]
        {
            new StarSortingMethod(),
            new AdminUserSorting(),
            new PublishedLevelUserSorting(),
            new CreationDateUserSorting(),
            new ProgressLevelUserSorting()
        };

        nextPage.onClick.AddListener(OnNextPage);
        previousPage.onClick.AddListener(OnPreviousPage);
        
        foreach (var sortingMethod in sortingMethods)
        {
            var button = Instantiate(sortingMethodButtonPrefab, sortingMethodContainer).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = sortingMethod.name;
            button.onClick.AddListener(() => SelectSortingMethod(sortingMethod));
            _sortingMethodButtons.Add(sortingMethod, button);
        }
        
        SelectSortingMethod(sortingMethods[0]);
    }

    public void WaitForUsers()
    {
        HidePagination();
        errorText.gameObject.SetActive(false);
        loadingIcon.gameObject.SetActive(true);
    }
    
    public void ShowError(string error)
    {
        loadingIcon.gameObject.SetActive(false);
        errorText.gameObject.SetActive(true);
        errorText.text = error;
    }
    
    public void DeleteAllUsers()
    {
        foreach (Transform child in usersContainer) 
            Destroy(child.gameObject);
    }
    
    public void SelectSortingMethod(SortingMethod sortingMethod)
    {
        foreach (var button in _sortingMethodButtons)
            button.Value.interactable = button.Key != sortingMethod;
        
        _sortingMethod = sortingMethod;
        DeleteAllUsers();
        
        sortingMethod.InitColumns(this);
        sortingMethod.SortUsers(this, _currentPage);
    }
    
    public void HidePagination()
    {
        paginationText.gameObject.SetActive(false);
        nextPage.gameObject.SetActive(false);
        previousPage.gameObject.SetActive(false);
    }
    
    public void InitPagination(NewDB.Pagination pagination)
    {
        paginationText.gameObject.SetActive(true);
        nextPage.gameObject.SetActive(true);
        previousPage.gameObject.SetActive(true);
        
        paginationText.text = $"Showing {pagination.size} (page {pagination.page}/{pagination.pages})";
        nextPage.gameObject.SetActive(pagination.hasNext);
        previousPage.gameObject.SetActive(pagination.hasPrevious);
    }
    
    public void OnNextPage()
    {
        _currentPage++;
        
        _sortingMethod.SortUsers(this, _currentPage);
    }
    
    public void OnPreviousPage()
    {
        _currentPage--;
        
        _sortingMethod.SortUsers(this, _currentPage);
    }
    
    public void CleanUp()
    {
        loadingIcon.gameObject.SetActive(false);
        errorText.gameObject.SetActive(false);
    }
    
    // ##################################################################################################
    
    public abstract class SortingMethod
    {

        public string name;
        
        public abstract void InitColumns(UsersController controller);
        
        public abstract void SortUsers(UsersController controller, int page);
    }
    
    public class StarSortingMethod : SortingMethod
    {

        public StarSortingMethod()
        {
            name = "Collected Stars";
        }
        
        public override void InitColumns(UsersController controller)
        {
            controller.column1Icon.sprite = controller.starIcon;
            controller.column1Icon.color = new Color(1, 1, 0);
            
            controller.column2Icon.sprite = controller.starIcon;
            controller.column2Icon.color = new Color(0, 1, 1);
            
            controller.column3Icon.sprite = controller.starsIcon;
            controller.column3Icon.color = new Color(1, 1, 1);
        }

        public override async void SortUsers(UsersController controller, int page)
        {
            controller.WaitForUsers();
            var response = await NewDB.RetrieveUserList(page, 0);
            if (response == null || response.status != "ok")
            {
                var error = response == null ? "Unknown error occured." : response.message;
                controller.ShowError(error);
                return;
            }

            var pagination = response.Get<NewDB.Pagination>("pagination");
            controller.InitPagination(pagination);

            var users = response.GetList<NewDB.UserWithStars>("users");
            bool wasLastShadowed = false;
            foreach (var user in users)
            {
                var entry = Instantiate(controller.userEntryPrefab, controller.usersContainer).GetComponent<UserEntryController>();
                entry.Init(controller, user.user.username, user.officialStars.ToString(), user.unofficialStars.ToString(), user.officialStars + user.unofficialStars + "");
                entry.SetShadow(wasLastShadowed);
                wasLastShadowed = !wasLastShadowed;
            }
            
            controller.CleanUp();
        }
            
    }
    
    public class AdminUserSorting : SortingMethod
    {
        
        public AdminUserSorting()
        {
            name = "Admins";
        }
        
        public override void InitColumns(UsersController controller)
        {
            controller.column1Icon.color = new Color(0, 0, 0, 0);
            controller.column2Icon.color = new Color(0, 0, 0, 0);
            
            controller.column3Icon.sprite = controller.shieldIcon;
            controller.column3Icon.color = new Color(0, 1, 1);
        }

        public override async void SortUsers(UsersController controller, int page)
        {
            controller.WaitForUsers();
            var response = await NewDB.RetrieveUserList(page, 1);
            if (response == null || response.status != "ok")
            {
                var error = response == null ? "Unknown error occured." : response.message;
                controller.ShowError(error);
                return;
            }

            var pagination = response.Get<NewDB.Pagination>("pagination");
            controller.InitPagination(pagination);

            var users = response.GetList<NewDB.User>("users");
            bool wasLastShadowed = false;
            foreach (var user in users)
            {
                var entry = Instantiate(controller.userEntryPrefab, controller.usersContainer).GetComponent<UserEntryController>();
                entry.Init(controller, user.username, "", "", user.admin ? "Yes" : "No");
                entry.SetShadow(wasLastShadowed);
                wasLastShadowed = !wasLastShadowed;
            }
            
            controller.CleanUp();
        }
        
    }
    
    public class PublishedLevelUserSorting : SortingMethod
    {
        
        public PublishedLevelUserSorting()
        {
            name = "Published Levels";
        }
        
        public override void InitColumns(UsersController controller)
        {
            controller.column1Icon.color = new Color(0, 0, 0, 0);
            controller.column2Icon.color = new Color(0, 0, 0, 0);
            
            controller.column3Icon.sprite = controller.levelIcon;
            controller.column3Icon.color = new Color(1, 1, 0);
        }

        public override async void SortUsers(UsersController controller, int page)
        {
            controller.WaitForUsers();
            var response = await NewDB.RetrieveUserList(page, 3);
            if (response == null || response.status != "ok")
            {
                var error = response == null ? "Unknown error occured." : response.message;
                controller.ShowError(error);
                return;
            }

            var pagination = response.Get<NewDB.Pagination>("pagination");
            controller.InitPagination(pagination);

            var users = response.GetList<NewDB.UserWithPublishedLevels>("users");
            bool wasLastShadowed = false;
            foreach (var user in users)
            {
                var entry = Instantiate(controller.userEntryPrefab, controller.usersContainer).GetComponent<UserEntryController>();
                entry.Init(controller, user.user.username, "", "", user.publishedLevels.ToString());
                entry.SetShadow(wasLastShadowed);
                wasLastShadowed = !wasLastShadowed;
            }
            
            controller.CleanUp();
        }
        
    }
    
    public class CreationDateUserSorting : SortingMethod
    {
        
        public CreationDateUserSorting()
        {
            name = "Creation Date";
        }
        
        public override void InitColumns(UsersController controller)
        {
            controller.column1Icon.color = new Color(0, 0, 0, 0);
            
            controller.column2Icon.sprite = controller.dateIcon;
            controller.column2Icon.color = new Color(1, 1, 0);
            
            controller.column3Icon.color = new Color(0, 0, 0, 0);
        }

        public override async void SortUsers(UsersController controller, int page)
        {
            controller.WaitForUsers();
            var response = await NewDB.RetrieveUserList(page, 2);
            if (response == null || response.status != "ok")
            {
                var error = response == null ? "Unknown error occured." : response.message;
                controller.ShowError(error);
                return;
            }

            var pagination = response.Get<NewDB.Pagination>("pagination");
            controller.InitPagination(pagination);

            var users = response.GetList<NewDB.User>("users");
            bool wasLastShadowed = false;
            foreach (var user in users)
            {
                var entry = Instantiate(controller.userEntryPrefab, controller.usersContainer).GetComponent<UserEntryController>();
                long creationDateMillis = user.creationDate;
                var timeSpan = TimeSpan.FromMilliseconds(creationDateMillis);
                var creationDate = DateTime.UnixEpoch.Add(timeSpan);
                entry.Init(controller, user.username, creationDate.Day.ToString(), creationDate.Month.ToString(), creationDate.Year.ToString());
                entry.SetShadow(wasLastShadowed);
                wasLastShadowed = !wasLastShadowed;
            }
            
            controller.CleanUp();
        }
        
    }
    
    public class ProgressLevelUserSorting : SortingMethod
    {
        
        public ProgressLevelUserSorting()
        {
            name = "Progress Level";
        }
        
        public override void InitColumns(UsersController controller)
        {
            controller.column1Icon.color = new Color(0, 0, 0, 0);
            
            controller.column2Icon.sprite = controller.progressLevelIcon;
            controller.column2Icon.color = new Color(1, 1, 0);
            
            controller.column3Icon.sprite = controller.progressDotsIcon;
            controller.column3Icon.color = new Color(1, 1, 0);
        }

        public override async void SortUsers(UsersController controller, int page)
        {
            controller.WaitForUsers();
            var response = await NewDB.RetrieveUserList(page, 4);
            if (response == null || response.status != "ok")
            {
                var error = response == null ? "Unknown error occured." : response.message;
                controller.ShowError(error);
                return;
            }

            var pagination = response.Get<NewDB.Pagination>("pagination");
            controller.InitPagination(pagination);

            var users = response.GetList<NewDB.UserWithProgressLevels>("users");
            bool wasLastShadowed = false;
            foreach (var user in users)
            {
                var entry = Instantiate(controller.userEntryPrefab, controller.usersContainer).GetComponent<UserEntryController>();
                entry.Init(controller, user.user.username, "", user.userLevel.ToString(), user.currentExperience.ToString());
                entry.SetShadow(wasLastShadowed);
                wasLastShadowed = !wasLastShadowed;
            }
            
            controller.CleanUp();
        }
        
    }
        
}