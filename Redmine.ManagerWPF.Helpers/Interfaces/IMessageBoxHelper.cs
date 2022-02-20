namespace Redmine.ManagerWPF.Helpers.Interfaces
{
    public interface IMessageBoxHelper
    {
        void ShowWarningInfoBox(string text, string caption);

        void ShowInformationBox(string text, string caption);

        bool ShowConfirmationBox(string text, string caption);
    }
}