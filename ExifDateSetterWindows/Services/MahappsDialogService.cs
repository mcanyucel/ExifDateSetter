using Core.Service;
using MahApps.Metro.Controls.Dialogs;

namespace ExifDateSetterWindows.Services;

public class MahappsDialogService(IDialogCoordinator dialogCoordinator): IDialogService
{
    public Task ShowInformation(object viewModel, string header, string message) => dialogCoordinator.ShowMessageAsync(viewModel, header, message);

    public Task ShowError(object viewModel, string header, string message) =>
        dialogCoordinator.ShowMessageAsync(viewModel, header, message, MessageDialogStyle.Affirmative, new MetroDialogSettings
        {
            AffirmativeButtonText = "OK",
            ColorScheme = MetroDialogColorScheme.Accented
        });

    public async Task<bool> ShowQuestion(object viewModel, string header, string message)
    {
        var settings = new MetroDialogSettings
        {
            AffirmativeButtonText = "Yes",
            NegativeButtonText = "No",
            ColorScheme = MetroDialogColorScheme.Inverted
        };
        
        var dialog = await dialogCoordinator.ShowMessageAsync(viewModel, header, message, MessageDialogStyle.AffirmativeAndNegative, settings);
        return dialog == MessageDialogResult.Affirmative;
    }
}