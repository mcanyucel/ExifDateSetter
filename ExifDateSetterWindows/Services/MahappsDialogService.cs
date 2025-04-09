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
}