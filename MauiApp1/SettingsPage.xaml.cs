using MauiApp1.Views;

namespace MauiApp1
{
    public partial class SettingsPage : BasePage
    {
        public SettingsPage()
        {
            InitializeComponent();

            SetHeaderTitle("Настройки");
            SetActiveButton("SettingsPage");

            // Настройки не имеют специальной кнопки в шапке
            SetHeaderButton(null);

            SetPageContent(new Label
            {
                Text = "Настройки будут здесь...",
                TextColor = Colors.White,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                FontSize = 16
            });
        }
    }
}