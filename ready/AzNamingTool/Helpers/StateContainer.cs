using AzureNamingTool.Models;

namespace AzureNamingTool.Helpers
{
    public class StateContainer
    {
        private bool? _verified;
        private bool? _admin;
        private bool? _password;
        private string _apptheme;

        public bool Verified
        {
            get => _verified ?? false;
            set
            {
                _verified = value;
                NotifyStateChanged();
            }
        }

        public void SetVerified(bool verified)
        {
            _verified = verified;
            NotifyStateChanged();
        }

        public bool Admin
        {
            get => _admin ?? false;
            set
            {
                _admin = value;
                NotifyStateChanged();
            }
        }

        public void SetAdmin(bool admin)
        {
            _admin = admin;
            NotifyStateChanged();
        }

        public bool Password
        {
            get => _password ?? false;
            set
            {
                _password = value;
                NotifyStateChanged();
            }
        }
        
        public void SetPassword(bool password)
        {
            _password = password;
            NotifyStateChanged();
        }


        public string AppTheme
        {
            get => _apptheme ?? "bg-default text-black";
            set
            {
                _apptheme = value;
                NotifyStateChanged();
            }
        }


        public void SetAppTheme(string value)
        {
            _apptheme = value;
            NotifyStateChanged();
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
