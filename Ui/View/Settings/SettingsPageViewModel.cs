﻿using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using _1RM.Model;
using _1RM.Service;
using _1RM.Utils;
using Shawn.Utils.Wpf;
using _1RM.Service.DataSource;
using _1RM.View.Settings.DataSource;
using _1RM.View.Settings.General;
using _1RM.View.Settings.Launcher;
using _1RM.View.Settings.ProtocolConfig;
using _1RM.View.Settings.Theme;
using _1RM.Service.DataSource.DAO;
using _1RM.View.Settings.CredentialVault;

namespace _1RM.View.Settings
{
    public partial class SettingsPageViewModel : NotifyPropertyChangedBaseScreen
    {
        private readonly DataSourceService _dataSourceService;
        private ConfigurationService _configurationService => IoC.Get<ConfigurationService>();
        private readonly GlobalData _appData;


        public SettingsPageViewModel(DataSourceService dataSourceService, GlobalData appData)
        {
            _dataSourceService = dataSourceService;
            _appData = appData;
        }

        protected override void OnViewLoaded()
        {
            ShowPage(CurrentPage);
        }


        private INotifyPropertyChanged? _selectedViewModel;
        public INotifyPropertyChanged SelectedViewModel
        {
            get => _selectedViewModel ??= GeneralSettingViewModel;
            set => SetAndNotifyIfChanged(ref _selectedViewModel, value);
        }

        public void ShowPage(EnumMainWindowPage page)
        {
            if (page == EnumMainWindowPage.List
                || page == EnumMainWindowPage.About
                || CurrentPage == page)
                return;

            CurrentPage = page;
            switch (page)
            {
                case EnumMainWindowPage.SettingsGeneral:
                    SelectedViewModel = GeneralSettingViewModel;
                    break;
                case EnumMainWindowPage.SettingsData:
                    SelectedViewModel = IoC.Get<DataSourceViewModel>();
                    break;
                case EnumMainWindowPage.SettingsCredentialVault:
                    SelectedViewModel = IoC.Get<CredentialVaultViewModel>();
                    break;
                case EnumMainWindowPage.SettingsLauncher:
                    SelectedViewModel = IoC.Get<LauncherSettingViewModel>();
                    break;
                case EnumMainWindowPage.SettingsTheme:
                    SelectedViewModel = IoC.Get<ThemeSettingViewModel>();
                    break;
                case EnumMainWindowPage.SettingsRunners:
                    SelectedViewModel = IoC.Get<ProtocolRunnerSettingsPageViewModel>();
                    break;
                default:
                    return;
            }
        }

        private EnumMainWindowPage _currentPage = EnumMainWindowPage.SettingsGeneral;
        public EnumMainWindowPage CurrentPage
        {
            get => _currentPage;
            private set => SetAndNotifyIfChanged(ref _currentPage, value);
        }

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => SetAndNotifyIfChanged(ref _progressBarVisibility, value);
        }

        public GeneralSettingViewModel GeneralSettingViewModel => IoC.Get<GeneralSettingViewModel>();
        public LauncherSettingViewModel LauncherSettingViewModel => IoC.Get<LauncherSettingViewModel>();


        public bool TabHeaderShowIconButton
		{
            get => _configurationService.General.TabHeaderShowIconButton;
            set
            {
                if (SetAndNotifyIfChanged(ref _configurationService.General.TabHeaderShowIconButton, value))
                {
                    _configurationService.Save();
                }
            }
        }

        public bool TabHeaderShowCloseButton
        {
            get => _configurationService.General.TabHeaderShowCloseButton;
            set
            {
                if (SetAndNotifyIfChanged(ref _configurationService.General.TabHeaderShowCloseButton, value))
                {
                    _configurationService.Save();
                }
            }
        }

        public bool TabHeaderShowReConnectButton
        {
            get => _configurationService.General.TabHeaderShowReConnectButton;
            set
            {
                if (SetAndNotifyIfChanged(ref _configurationService.General.TabHeaderShowReConnectButton, value))
                {
                    _configurationService.Save();
                }
            }
        }

        private RelayCommand? _cmdSaveAndGoBack;
        public RelayCommand CmdSaveAndGoBack
        {
            get
            {
                return _cmdSaveAndGoBack ??= new RelayCommand((o) =>
                {
                    // check if Db is ok
                    var res = _dataSourceService.LocalDataSource?.Database_SelfCheck() ?? DatabaseStatus.New(EnumDatabaseStatus.AccessDenied);
                    if (res.Status != EnumDatabaseStatus.OK)
                    {
                        ShowPage(EnumMainWindowPage.SettingsData);
                        MessageBoxHelper.ErrorAlert(res.GetErrorMessage);
                        return;
                    }

                    // do not check additional sources here, because team database may not be connected when one leaves working place.
                    //foreach (var additionalSource in _dataSourceService.AdditionalSources)
                    //{
                    //    var status = additionalSource.Value.Database_SelfCheck();
                    //    if (status.Status != EnumDatabaseStatus.OK)
                    //    {
                    //        ShowPage(EnumMainWindowPage.SettingsData);
                    //        MessageBoxHelper.ErrorAlert(status.GetErrorMessage);
                    //        return;
                    //    }
                    //}

                    if (_configurationService.Launcher.LauncherEnabled != IoC.TryGet<LauncherWindowViewModel>()?.SetHotKey(_configurationService.Launcher.LauncherEnabled, _configurationService.Launcher.HotKeyModifiers, _configurationService.Launcher.HotKeyKey))
                    {
                        ShowPage(EnumMainWindowPage.SettingsLauncher);
                        MessageBoxHelper.ErrorAlert(IoC.Translate("hotkey_registered_fail"));
                        return;
                    }

                    Task.Factory.StartNew(() =>
                    {
                        _configurationService.Save();
                        IoC.Get<ProtocolConfigurationService>().Save();
                    });

                    IoC.Get<MainWindowViewModel>().ShowList(false);
                    _selectedViewModel = null;
                });
            }
        }



        private RelayCommand? _cmdShowPage;
        public RelayCommand CmdShowPage
        {
            get
            {
                return _cmdShowPage ??= new RelayCommand((o) =>
                {
                    if (o is EnumMainWindowPage page)
                    {
                        ShowPage(page);
                    }
                });
            }
        }





        public bool ListPageIsCardView
        {
            get => _configurationService.General.ListPageIsCardView;
            set
            {
                if (SetAndNotifyIfChanged(ref _configurationService.General.ListPageIsCardView, value))
                {
                    _configurationService.Save();
                }
            }
        }
    }
}
