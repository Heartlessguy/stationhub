﻿using ReactiveUI;
using UnitystationLauncher.Models;
using System.Reactive;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using Serilog;

namespace UnitystationLauncher.ViewModels
{
    public class LauncherViewModel : ViewModelBase
    {
        private readonly AuthManager authManager;
        private readonly Lazy<LoginViewModel> logoutVM;
        string username;
        ViewModelBase news;
        PanelBase[] panels;
        ViewModelBase? selectedPanel;

        private readonly HttpClient http;

        public LauncherViewModel(
            AuthManager authManager, 
            ServersPanelViewModel serversPanel, 
            InstallationsPanelViewModel installationsPanel,
            NewsViewModel news,
            Lazy<LoginViewModel> logoutVM, HttpClient http )
        {
            this.authManager = authManager;
            this.logoutVM = logoutVM;
            this.http = http;
            News = news;
            Panels = new PanelBase[]
            {
                serversPanel,
                installationsPanel
            };
            Username = this.authManager!.AuthLink.User.DisplayName;
            Logout = ReactiveCommand.Create(LogoutImp);
            ShowUpdateReqd = ReactiveCommand.Create(ShowUpdateImp);
            SelectedPanel = serversPanel;
            ValidateClientVersion();
        }

        async Task ValidateClientVersion()
        {
            var data = await http.GetStringAsync(Config.validateUrl);
            Config.serverHubClientConfig = JsonConvert.DeserializeObject<HubClientConfig>(data);

            if(Config.serverHubClientConfig.buildNumber != Config.currentBuild)
            {
                Log.Information($"Client is old ({Config.currentBuild}) new version is ({Config.serverHubClientConfig.buildNumber})");
                ShowUpdateReqd.Execute().Subscribe();
            }
        }

        public string Username
        {
            get => username;
            set => this.RaiseAndSetIfChanged(ref username, value);
        }

        public ViewModelBase News
        {
            get => news;
            set => this.RaiseAndSetIfChanged(ref news, value);
        }

        public PanelBase[] Panels
        {
            get => panels;
            set => this.RaiseAndSetIfChanged(ref panels, value);
        }

        public ViewModelBase? SelectedPanel
        {
            get => selectedPanel;
            set => this.RaiseAndSetIfChanged(ref selectedPanel, value);
        }

        public ReactiveCommand<Unit, ViewModelBase> Logout { get; }
        public ReactiveCommand<Unit, ViewModelBase> ShowUpdateReqd { get; }

        ViewModelBase LogoutImp()
        {
            File.Delete("settings.json");
            return logoutVM.Value;
        }

        ViewModelBase ShowUpdateImp()
        {
            return logoutVM.Value;
        }
    }
}
