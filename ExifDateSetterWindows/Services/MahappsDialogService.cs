using Core.Service;
using MahApps.Metro.Controls.Dialogs;

namespace ExifDateSetterWindows.Services;

public class MahappsDialogService(IDialogCoordinator dialogCoordinator): IDialogService
{
    public async Task ShowInformation(object viewModel, string header, string message)
    {
        await dialogCoordinator.ShowMessageAsync(viewModel, header, message);
    }
}