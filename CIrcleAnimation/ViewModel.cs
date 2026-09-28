using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CircleAnimation
{
    public class ViewModel : INotifyPropertyChanged
    {
        private readonly DbHelper _dbService;
        private readonly DispatcherTimer _timer;
        private const double CenterX = 150;
        private const double CenterY = 150;
        private const double PathRadius = 100;
        private const double DotRadius = 20;

        private double _angleInDegrees;
        private bool _isAnimating;
        private string _statusMessage = "Нажмите «Пуск».";

        public ObservableCollection<AnimationRecord> Records { get; }
            = new ObservableCollection<AnimationRecord>();

        //левая граница квадрата вокруг кружка
        public double CircleX =>
            CenterX + PathRadius * Math.Cos(_angleInDegrees * Math.PI / 180.0) - DotRadius;

        //верхняя граница
        public double CircleY =>
            CenterY + PathRadius * Math.Sin(_angleInDegrees * Math.PI / 180.0) - DotRadius;

        public bool IsAnimating
        {
            get => _isAnimating;
            set { if (_isAnimating != value) { _isAnimating = value; OnPropertyChanged(); } }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { if (_statusMessage != value) { _statusMessage = value; OnPropertyChanged(); } }
        }

        /// <summary>Текст на кнопке: «Пуск» или «Стоп».</summary>
        public string ButtonText => IsAnimating ? "Стоп" : "Пуск";

        public RelayCommand ToggleCommand { get; }

        public ViewModel()
        {
            _dbService = new DbHelper();

            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)  // ≈60 FPS
            };
            _timer.Tick += OnTimerTick;

            ToggleCommand = new RelayCommand(param => _ = ToggleAsync());

            _ = LoadRecordsAsync();
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            _angleInDegrees = (_angleInDegrees + 3.0) % 360.0;
            OnPropertyChanged(nameof(CircleX));
            OnPropertyChanged(nameof(CircleY));
        }

        private async Task ToggleAsync()
        {
            if (IsAnimating)
            {
                // --- ОСТАНОВКА ---
                _timer.Stop();
                IsAnimating = false;
                StatusMessage = "Анимация остановлена.";
            }
            else
            {
                // --- ЗАПУСК ---
                _timer.Start();
                IsAnimating = true;

                var record = new AnimationRecord { StartedAt = DateTime.Now };

                try
                {
                    await _dbService.SaveStartAsync(record);
                    Records.Insert(0, record);
                    StatusMessage = $"Запуск сохранён: {record.StartedAt:HH:mm:ss}.";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка БД: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                    StatusMessage = "Ошибка сохранения в БД.";
                }
            }

            OnPropertyChanged(nameof(ButtonText));
        }

        private async Task LoadRecordsAsync()
        {
            try
            {
                var fromDb = await _dbService.LoadRecordsAsync();
                foreach (var r in fromDb)
                    Records.Add(r);

                StatusMessage = $"Загружено записей: {Records.Count}.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}",
                                "Ошибка",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}