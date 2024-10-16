﻿using System.IO;
using System;
using Microsoft.Extensions.Logging;
using Chorizite.Core.Plugins;
using Chorizite.Core.Plugins.AssemblyLoader;
using Core.AC.Lib;
using Chorizite.Core.Net;
using Chorizite.ACProtocol;
using Core.UI;
using Core.AC.UIModels;
using Chorizite.Core.Render;
using System.Collections.Generic;
using System.Xml.Linq;
using Core.UI.Lib;
using Chorizite.Core.Backend;

namespace Core.AC {
    public class CoreACPlugin : IPluginCore, ScreenProvider<GameScreen> {
        private readonly Dictionary<GameScreen, string> _registeredGameScreens = [];
        private GameScreen _currentScreen = GameScreen.None;
        private CharSelectScreenModel _charSelectModel;
        private DatPatchScreenModel _datPatchModel;

        internal static ILogger<CoreACPlugin>? Log;
        internal static CoreACPlugin Instance;
        internal CoreUIPlugin CoreUI { get; }
        public IClientBackend ClientBackend { get; }
        internal NetworkParser Net { get; }

        public GameInterface Game { get; }

        public GameScreen CurrentScreen {
            get => _currentScreen;
            set {
                // TODO: validate screen transitions
                if (_currentScreen != value) {
                    var oldScreen = _currentScreen;
                    _currentScreen = value;
                    CoreUI.Screen = _currentScreen.ToString();
                    OnScreenChanged?.Invoke(this, new ScreenChangedEventArgs(oldScreen, _currentScreen));
                }
            }
        }

        /// <summary>
        /// Called when the game screen changes.
        /// </summary>
        public event EventHandler<ScreenChangedEventArgs>? OnScreenChanged;

        protected CoreACPlugin(AssemblyPluginManifest manifest, IClientBackend clientBackend, IPluginManager pluginManager, NetworkParser net, CoreUIPlugin coreUI, ILogger<CoreACPlugin> log) : base(manifest) {
            Instance = this;
            Log = log;
            Net = net;
            CoreUI = coreUI;
            ClientBackend = clientBackend;

            Game = new GameInterface(log, Net.Messages);

            /*
            if (IsRunningInClient) {
                if (!IsHotReload) {
                    _models.Add(GameScreen.DatPatch, new DatPatchScreenModel("DatPatchScreen", this));
                    _models.Add(GameScreen.CharSelect, new CharSelectScreenModel("CharSelectScreen", this));
                }
                else if (File.Exists(Path.Combine(DataDirectory, "models.json"))) {
                    var serializeOptions = new JsonSerializerOptions {
                        WriteIndented = true,
                        Converters = {
                            new UIDataModelJsonConverter(this)
                        }
                    };
                    try {
                        var models = JsonSerializer.Deserialize<Dictionary<GameScreen, UIDataModel>>(File.ReadAllText(Path.Combine(DataDirectory, "models.json")), serializeOptions);
                        foreach (var model in models) {
                            _models.Add(model.Key, model.Value);
                        }
                    }
                    catch (Exception ex) {
                        _log?.LogError(ex, "Error deserializing models.json");
                    }
                }
            }
            */

            _charSelectModel = new CharSelectScreenModel();
            _datPatchModel = new DatPatchScreenModel();

            CoreUI.RegisterUIModel("CharSelectScreen", _charSelectModel);
            CoreUI.RegisterUIModel("DatPatchScreen", _datPatchModel);

            RegisterScreen(GameScreen.CharSelect, Path.Combine(AssemblyDirectory, "assets", "CharSelect.rml"));
            RegisterScreen(GameScreen.DatPatch, Path.Combine(AssemblyDirectory, "assets", "DatPatch.rml"));

            ClientBackend.OnScreenChanged += ClientBackend_OnScreenChanged;

            Log?.LogDebug($"CoreAC Version: {Manifest.Version}");
        }

        private void ClientBackend_OnScreenChanged(object? sender, EventArgs e) {
            CurrentScreen = (GameScreen)ClientBackend.GameScreen;
        }

        /// <inheritdoc/>
        public bool RegisterScreen(GameScreen screen, string rmlPath) {
            if (_registeredGameScreens.ContainsKey(screen)) {
                _registeredGameScreens[screen] = rmlPath;
            }
            else {
                _registeredGameScreens.Add(screen, rmlPath);
            }

            return CoreUI.RegisterScreen(screen.ToString(), rmlPath);
        }

        /// <inheritdoc/>
        public void UnregisterScreen(GameScreen screen, string rmlPath) {
            if (_registeredGameScreens.TryGetValue(screen, out var rmlFile) && rmlFile == rmlPath) {
                _registeredGameScreens.Remove(screen);
            }

            CoreUI.UnregisterScreen(screen.ToString(), rmlPath);
        }

        public override void Dispose() {
            ClientBackend.OnScreenChanged -= ClientBackend_OnScreenChanged;

            CoreUI.UnregisterUIModel("CharSelectScreen", _charSelectModel);
            CoreUI.UnregisterUIModel("DatPatchScreen", _datPatchModel);

            _charSelectModel.Dispose();
            _datPatchModel.Dispose();

            UnregisterScreen(GameScreen.CharSelect, Path.Combine(AssemblyDirectory, "assets", "CharSelect.rml"));
            UnregisterScreen(GameScreen.DatPatch, Path.Combine(AssemblyDirectory, "assets", "DatPatch.rml"));
        }
    }
}
