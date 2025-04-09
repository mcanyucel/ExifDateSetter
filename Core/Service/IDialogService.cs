namespace Core.Service;

public interface IDialogService
{
    public Task ShowInformation(object viewModel, string header, string message);
    public Task ShowError(object viewModel, string header, string message);
}