namespace AzureNamingTool.Models
{
    public class StateContainer
    {
        private bool? _verified;
        private bool? _admin;
        private bool? _password;
        private string? _apptheme;
        private bool? _newsenabled;
        public bool _reloadnav;
        public bool? _configurationdatasynced;

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
            get => _apptheme ?? "bg-default text-dark";
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

        public bool NewsEnabled
        {
            get => _newsenabled ?? true;
            set
            {
                _newsenabled = value;
                NotifyStateChanged();
            }
        }

        public void SetNewsEnabled(bool newsenabled)
        {
            _newsenabled = newsenabled;
            NotifyStateChanged();
        }

        public void SetNavReload(bool reloadnav)
        {
            _reloadnav = reloadnav;
            NotifyStateChanged();
        }

        public bool ConfigurationDataSynced
        {
            get => _configurationdatasynced ?? false;
            set
            {
                _configurationdatasynced = value;
                NotifyStateChanged();
            }
        }

        public void SetConfigurationDataSynced(bool configurationdatasynced)
        {
            _configurationdatasynced = configurationdatasynced;
            NotifyStateChanged();
        }

        public event Action? OnChange;

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}