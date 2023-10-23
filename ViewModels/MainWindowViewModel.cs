﻿using LasAnalyzer.Services;
using ReactiveUI;
using System.Reactive.Subjects;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using LasAnalyzer.Models;

namespace LasAnalyzer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private LasFileReader _lasFileReader;

        public ReactiveCommand<Unit, Unit> OpenLasFileCommand { get; }
        private BehaviorSubject<GraphData> _lasDataSubject = new BehaviorSubject<GraphData>(null);

        public MainWindowViewModel()
        {
            _lasFileReader = new LasFileReader();

            OpenLasFileCommand = ReactiveCommand.CreateFromTask(OpenLasFileAsync);

            // Подписка на изменения в BehaviorSubject и привязка к свойству LasData
            _lasDataSubject
                .ToProperty(this, x => x.LasData);
        }

        public GraphData LasData => _lasDataSubject.Value;

        private async Task<Unit> OpenLasFileAsync()
        {
            var file = await DoOpenFilePickerAsync();
            if (file is null) return Unit.Default;

            var lasData = _lasFileReader.OpenLasFile(file.Path.AbsolutePath);
            if (lasData is not null)
            {
                _lasDataSubject.OnNext(lasData);
            }

            return Unit.Default;
        }

        private async Task<IStorageFile?> DoOpenFilePickerAsync()
        {
            if (Application.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop ||
                desktop.MainWindow?.StorageProvider is not { } provider)
                throw new NullReferenceException("Missing StorageProvider instance.");

            FilePickerFileType LasFileType = new("Las files")
            {
                Patterns = new[] { "*.las" },
            };

            var files = await provider.OpenFilePickerAsync(new FilePickerOpenOptions()
            {
                Title = "Open Text File",
                FileTypeFilter = new[] { LasFileType },
                AllowMultiple = false
            });

            return files?.Count >= 1 ? files[0] : null;
        }
    }
}