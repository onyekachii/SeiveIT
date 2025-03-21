using CommunityToolkit.Maui.Views;
using SeiveIT.Services.Interface;
using SeiveIT.Views.popups;

namespace SeiveIT.Views;

public partial class LobbyPage : ContentPage
{
	IServiceManager _serviceManager;
    public LobbyPage(IServiceManager sm)
	{
		InitializeComponent();
        _serviceManager = sm;
    }
	public void displayPopup(object sender, EventArgs e) => this.ShowPopup(new AddprojectPopup(_serviceManager));
    
}