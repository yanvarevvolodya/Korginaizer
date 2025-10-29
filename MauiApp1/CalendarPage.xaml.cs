using MauiApp1.Views;

namespace MauiApp1;

public partial class CalendarPage : BasePage
{
    public CalendarPage()
    {
        InitializeComponent();

        SetHeaderTitle("Календарь");
        SetActiveButton("CalendarPage");

        // Календарь не имеет специальной кнопки в шапке
        SetHeaderButton(null);

        SetPageContent(new Label
        {
            Text = "Календарь будет здесь...",
            TextColor = Colors.White,
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
            FontSize = 16
        });
    }
}