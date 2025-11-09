using Game.DataStores;
using mehmetsrl.MVC.core;
using UnityEngine;
using UnityEngine.UI;


public class DashboardPageView : View<DashboardPageModel>
{
    [SerializeField]
    private Text _levelText;
    [SerializeField]
    private Button _levelButton;
    
    #region Accesors
    private new DashboardPageController Controller => base.Controller as DashboardPageController;

    #endregion
    
    private void OnStartNewLevelClicked()
    {
        Controller.LoadLevel();
    }

    protected override void OnCreate()
    {
        Debug.Assert(_levelText != null);
        Debug.Assert(_levelButton != null);
    }

    protected override void OnStateChanged(ViewState state)
    {
        base.OnStateChanged(state);
        switch (state)
        {
            case ViewState.Visible:
                _levelButton.onClick.AddListener(OnStartNewLevelClicked);
                break;
            case ViewState.Invisible:
                _levelButton.onClick.RemoveListener(OnStartNewLevelClicked);
                break;
        }
    }

    public override void UpdateView()
    {
        _levelText.text = $"Level {GameDataStore.Instance.CurrentLevelId}";
    }
}