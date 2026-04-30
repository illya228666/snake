using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace SnakeTimeKiller
{
    public class SnakeGameControl : UserControl
    {
        private const int MinimumGridSize = 6;
        private const int MaximumGridSize = 200;

        private static readonly Brush DefaultBoardBackgroundBrush = CreateFrozenBrush("#0F172A");
        private static readonly Brush DefaultGridLineBrush = CreateFrozenBrush("#1E293B");
        private static readonly Brush DefaultSnakeHeadBrush = CreateFrozenBrush("#F8FAFC");
        private static readonly Brush DefaultSnakeBodyBrush = CreateFrozenBrush("#22C55E");
        private static readonly Brush DefaultLowCargoBrush = CreateFrozenBrush("#38BDF8");
        private static readonly Brush DefaultMediumCargoBrush = CreateFrozenBrush("#FBBF24");
        private static readonly Brush DefaultHighCargoBrush = CreateFrozenBrush("#FB7185");
        private static readonly Brush DefaultOverlayBrush = CreateFrozenBrush("#CC020617");
        private static readonly Brush DefaultOverlayTextBrush = CreateFrozenBrush("#F8FAFC");

        private readonly Grid _root;
        private readonly Canvas _boardCanvas;
        private readonly BoardSurface _boardSurface;
        private readonly TextBlock _scoreText;
        private readonly TextBlock _gameOverText;
        private readonly DispatcherTimer _timer;
        private readonly List<FrameworkElement> _snakeElements;

        private SnakeGameEngine _engine;
        private FrameworkElement _cargoElement;
        private GridPosition? _lastCargoPosition;
        private CargoType? _lastCargoType;
        private double _cellSize;

        public static readonly DependencyProperty RowsProperty =
            DependencyProperty.Register(
                nameof(Rows),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(16, OnGameSettingsChanged, CoerceGridSize));

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(
                nameof(Columns),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(24, OnGameSettingsChanged, CoerceGridSize));

        public static readonly DependencyProperty TickIntervalProperty =
            DependencyProperty.Register(
                nameof(TickInterval),
                typeof(TimeSpan),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(TimeSpan.FromMilliseconds(120), OnTickIntervalChanged, CoerceTickInterval));

        public static readonly DependencyProperty AutoStartProperty =
            DependencyProperty.Register(
                nameof(AutoStart),
                typeof(bool),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(true));

        public static readonly DependencyProperty SnakeHeadImageProperty =
            DependencyProperty.Register(
                nameof(SnakeHeadImage),
                typeof(ImageSource),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(null, OnVisualAssetsChanged));

        public static readonly DependencyProperty SnakeBodyImageProperty =
            DependencyProperty.Register(
                nameof(SnakeBodyImage),
                typeof(ImageSource),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(null, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoLowImageProperty =
            DependencyProperty.Register(
                nameof(CargoLowImage),
                typeof(ImageSource),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(null, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoMediumImageProperty =
            DependencyProperty.Register(
                nameof(CargoMediumImage),
                typeof(ImageSource),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(null, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoHighImageProperty =
            DependencyProperty.Register(
                nameof(CargoHighImage),
                typeof(ImageSource),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(null, OnVisualAssetsChanged));

        public static readonly DependencyProperty SnakeHeadBrushProperty =
            DependencyProperty.Register(
                nameof(SnakeHeadBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultSnakeHeadBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty SnakeBodyBrushProperty =
            DependencyProperty.Register(
                nameof(SnakeBodyBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultSnakeBodyBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoLowBrushProperty =
            DependencyProperty.Register(
                nameof(CargoLowBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultLowCargoBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoMediumBrushProperty =
            DependencyProperty.Register(
                nameof(CargoMediumBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultMediumCargoBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty CargoHighBrushProperty =
            DependencyProperty.Register(
                nameof(CargoHighBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultHighCargoBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty BoardBackgroundBrushProperty =
            DependencyProperty.Register(
                nameof(BoardBackgroundBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultBoardBackgroundBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty GridLineBrushProperty =
            DependencyProperty.Register(
                nameof(GridLineBrush),
                typeof(Brush),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(DefaultGridLineBrush, OnVisualAssetsChanged));

        public static readonly DependencyProperty LowCargoScoreProperty =
            DependencyProperty.Register(
                nameof(LowCargoScore),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(10, OnGameSettingsChanged, CoerceNonNegativeInt));

        public static readonly DependencyProperty MediumCargoScoreProperty =
            DependencyProperty.Register(
                nameof(MediumCargoScore),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(25, OnGameSettingsChanged, CoerceNonNegativeInt));

        public static readonly DependencyProperty HighCargoScoreProperty =
            DependencyProperty.Register(
                nameof(HighCargoScore),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(50, OnGameSettingsChanged, CoerceNonNegativeInt));

        private static readonly DependencyPropertyKey ScorePropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(Score),
                typeof(int),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(0));

        public static readonly DependencyProperty ScoreProperty = ScorePropertyKey.DependencyProperty;

        private static readonly DependencyPropertyKey IsGameOverPropertyKey =
            DependencyProperty.RegisterReadOnly(
                nameof(IsGameOver),
                typeof(bool),
                typeof(SnakeGameControl),
                new FrameworkPropertyMetadata(false));

        public static readonly DependencyProperty IsGameOverProperty = IsGameOverPropertyKey.DependencyProperty;

        public SnakeGameControl()
        {
            Focusable = true;
            IsTabStop = true;
            ClipToBounds = true;
            UseLayoutRounding = true;
            SnapsToDevicePixels = true;

            _snakeElements = new List<FrameworkElement>();
            _timer = new DispatcherTimer(DispatcherPriority.Render);
            _timer.Interval = TickInterval;
            _timer.Tick += OnTimerTick;

            _root = new Grid
            {
                Background = Brushes.Transparent,
                ClipToBounds = true
            };

            _boardCanvas = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                ClipToBounds = true,
                SnapsToDevicePixels = true
            };

            _boardSurface = new BoardSurface();
            _boardCanvas.Children.Add(_boardSurface);

            _scoreText = new TextBlock
            {
                Margin = new Thickness(10),
                Padding = new Thickness(8, 4, 8, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                Background = DefaultOverlayBrush,
                Foreground = DefaultOverlayTextBrush,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                IsHitTestVisible = false
            };

            _gameOverText = new TextBlock
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(18, 12, 18, 12),
                Background = DefaultOverlayBrush,
                Foreground = DefaultOverlayTextBrush,
                FontSize = 22,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Visibility = Visibility.Collapsed,
                IsHitTestVisible = false
            };

            _root.Children.Add(_boardCanvas);
            _root.Children.Add(_scoreText);
            _root.Children.Add(_gameOverText);
            Content = _root;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
            SizeChanged += OnSizeChanged;
            PreviewKeyDown += OnPreviewKeyDown;
            MouseLeftButtonDown += OnMouseLeftButtonDown;

            ConfigureGame(false);
        }

        public event EventHandler<SnakeScoreChangedEventArgs> ScoreChanged;

        public event EventHandler<SnakeCargoCollectedEventArgs> CargoCollected;

        public event EventHandler<SnakeGameOverEventArgs> GameOver;

        public int Rows
        {
            get => (int)GetValue(RowsProperty);
            set => SetValue(RowsProperty, value);
        }

        public int Columns
        {
            get => (int)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        public TimeSpan TickInterval
        {
            get => (TimeSpan)GetValue(TickIntervalProperty);
            set => SetValue(TickIntervalProperty, value);
        }

        public bool AutoStart
        {
            get => (bool)GetValue(AutoStartProperty);
            set => SetValue(AutoStartProperty, value);
        }

        public ImageSource SnakeHeadImage
        {
            get => (ImageSource)GetValue(SnakeHeadImageProperty);
            set => SetValue(SnakeHeadImageProperty, value);
        }

        public ImageSource SnakeBodyImage
        {
            get => (ImageSource)GetValue(SnakeBodyImageProperty);
            set => SetValue(SnakeBodyImageProperty, value);
        }

        public ImageSource CargoLowImage
        {
            get => (ImageSource)GetValue(CargoLowImageProperty);
            set => SetValue(CargoLowImageProperty, value);
        }

        public ImageSource CargoMediumImage
        {
            get => (ImageSource)GetValue(CargoMediumImageProperty);
            set => SetValue(CargoMediumImageProperty, value);
        }

        public ImageSource CargoHighImage
        {
            get => (ImageSource)GetValue(CargoHighImageProperty);
            set => SetValue(CargoHighImageProperty, value);
        }

        public Brush SnakeHeadBrush
        {
            get => (Brush)GetValue(SnakeHeadBrushProperty);
            set => SetValue(SnakeHeadBrushProperty, value);
        }

        public Brush SnakeBodyBrush
        {
            get => (Brush)GetValue(SnakeBodyBrushProperty);
            set => SetValue(SnakeBodyBrushProperty, value);
        }

        public Brush CargoLowBrush
        {
            get => (Brush)GetValue(CargoLowBrushProperty);
            set => SetValue(CargoLowBrushProperty, value);
        }

        public Brush CargoMediumBrush
        {
            get => (Brush)GetValue(CargoMediumBrushProperty);
            set => SetValue(CargoMediumBrushProperty, value);
        }

        public Brush CargoHighBrush
        {
            get => (Brush)GetValue(CargoHighBrushProperty);
            set => SetValue(CargoHighBrushProperty, value);
        }

        public Brush BoardBackgroundBrush
        {
            get => (Brush)GetValue(BoardBackgroundBrushProperty);
            set => SetValue(BoardBackgroundBrushProperty, value);
        }

        public Brush GridLineBrush
        {
            get => (Brush)GetValue(GridLineBrushProperty);
            set => SetValue(GridLineBrushProperty, value);
        }

        public int LowCargoScore
        {
            get => (int)GetValue(LowCargoScoreProperty);
            set => SetValue(LowCargoScoreProperty, value);
        }

        public int MediumCargoScore
        {
            get => (int)GetValue(MediumCargoScoreProperty);
            set => SetValue(MediumCargoScoreProperty, value);
        }

        public int HighCargoScore
        {
            get => (int)GetValue(HighCargoScoreProperty);
            set => SetValue(HighCargoScoreProperty, value);
        }

        public int Score
        {
            get => (int)GetValue(ScoreProperty);
            private set => SetValue(ScorePropertyKey, value);
        }

        public bool IsGameOver
        {
            get => (bool)GetValue(IsGameOverProperty);
            private set => SetValue(IsGameOverPropertyKey, value);
        }

        public void Start()
        {
            if (_engine == null || _engine.IsGameOver)
            {
                ConfigureGame(false);
            }

            Focus();
            Keyboard.Focus(this);
            UpdateTimerInterval();
            _timer.Start();
            Render(false);
        }

        public void Pause()
        {
            _timer.Stop();
        }

        public void Reset()
        {
            var shouldRestart = _timer.IsEnabled;
            ConfigureGame(false);

            if (shouldRestart)
            {
                Start();
            }
        }

        public void Turn(Direction direction)
        {
            if (_engine == null)
            {
                return;
            }

            _engine.Turn(direction);
        }

        private static Brush CreateFrozenBrush(string color)
        {
            var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(color));
            brush.Freeze();
            return brush;
        }

        private static object CoerceGridSize(DependencyObject d, object baseValue)
        {
            var value = (int)baseValue;

            if (value < MinimumGridSize)
            {
                return MinimumGridSize;
            }

            if (value > MaximumGridSize)
            {
                return MaximumGridSize;
            }

            return value;
        }

        private static object CoerceNonNegativeInt(DependencyObject d, object baseValue)
        {
            return Math.Max(0, (int)baseValue);
        }

        private static object CoerceTickInterval(DependencyObject d, object baseValue)
        {
            var value = (TimeSpan)baseValue;

            if (value < TimeSpan.FromMilliseconds(40))
            {
                return TimeSpan.FromMilliseconds(40);
            }

            if (value > TimeSpan.FromSeconds(1))
            {
                return TimeSpan.FromSeconds(1);
            }

            return value;
        }

        private static void OnGameSettingsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SnakeGameControl)d;
            if (control._timer == null)
            {
                return;
            }

            var wasRunning = control._timer.IsEnabled;
            control.ConfigureGame(false);

            if (wasRunning && control.IsLoaded)
            {
                control.Start();
            }
        }

        private static void OnTickIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SnakeGameControl)d;
            if (control._timer != null)
            {
                control.UpdateTimerInterval();
            }
        }

        private static void OnVisualAssetsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (SnakeGameControl)d;
            if (control._boardCanvas == null)
            {
                return;
            }

            control.ResetVisualElements();
            control.Render(false);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            Render(false);

            if (AutoStart)
            {
                Start();
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Render(false);
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            Keyboard.Focus(this);
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.W:
                    Turn(Direction.Up);
                    e.Handled = true;
                    break;
                case Key.Right:
                case Key.D:
                    Turn(Direction.Right);
                    e.Handled = true;
                    break;
                case Key.Down:
                case Key.S:
                    Turn(Direction.Down);
                    e.Handled = true;
                    break;
                case Key.Left:
                case Key.A:
                    Turn(Direction.Left);
                    e.Handled = true;
                    break;
            }
        }

        private void OnTimerTick(object sender, EventArgs e)
        {
            if (_engine == null)
            {
                return;
            }

            _engine.Step();
            Render(true);
        }

        private void ConfigureGame(bool keepVisuals)
        {
            DetachEngine();
            _engine = new SnakeGameEngine(
                Rows,
                Columns,
                LowCargoScore,
                MediumCargoScore,
                HighCargoScore);
            AttachEngine();

            Score = _engine.Score;
            IsGameOver = _engine.IsGameOver;
            _lastCargoPosition = null;
            _lastCargoType = null;

            if (!keepVisuals)
            {
                ResetVisualElements();
            }

            UpdateTimerInterval();
            UpdateScoreText();
            Render(false);
        }

        private void AttachEngine()
        {
            if (_engine == null)
            {
                return;
            }

            _engine.ScoreChanged += OnEngineScoreChanged;
            _engine.CargoCollected += OnEngineCargoCollected;
            _engine.GameOver += OnEngineGameOver;
        }

        private void DetachEngine()
        {
            if (_engine == null)
            {
                return;
            }

            _engine.ScoreChanged -= OnEngineScoreChanged;
            _engine.CargoCollected -= OnEngineCargoCollected;
            _engine.GameOver -= OnEngineGameOver;
        }

        private void OnEngineScoreChanged(object sender, SnakeScoreChangedEventArgs e)
        {
            Score = e.NewScore;
            UpdateScoreText();
            ScoreChanged?.Invoke(this, e);
        }

        private void OnEngineCargoCollected(object sender, SnakeCargoCollectedEventArgs e)
        {
            CreateCargoPop(e.Cargo);
            CargoCollected?.Invoke(this, e);
        }

        private void OnEngineGameOver(object sender, SnakeGameOverEventArgs e)
        {
            Pause();
            IsGameOver = true;
            UpdateGameOverText();
            GameOver?.Invoke(this, e);
        }

        private void UpdateTimerInterval()
        {
            _timer.Interval = TickInterval;
        }

        private void Render(bool animate)
        {
            if (_engine == null)
            {
                return;
            }

            UpdateBoardSize();
            UpdateBoardSurface();
            UpdateSnakeElements(animate);
            UpdateCargoElement(animate);
            UpdateScoreText();
            UpdateGameOverText();
        }

        private void UpdateBoardSize()
        {
            var availableWidth = Math.Max(0, ActualWidth);
            var availableHeight = Math.Max(0, ActualHeight);

            if (availableWidth <= 0 || availableHeight <= 0)
            {
                return;
            }

            _cellSize = Math.Max(1, Math.Min(availableWidth / Columns, availableHeight / Rows));
            _boardCanvas.Width = _cellSize * Columns;
            _boardCanvas.Height = _cellSize * Rows;
        }

        private void UpdateBoardSurface()
        {
            _boardSurface.Width = _boardCanvas.Width;
            _boardSurface.Height = _boardCanvas.Height;
            _boardSurface.Rows = Rows;
            _boardSurface.Columns = Columns;
            _boardSurface.CellSize = _cellSize;
            _boardSurface.BoardBackground = BoardBackgroundBrush ?? DefaultBoardBackgroundBrush;
            _boardSurface.GridLine = GridLineBrush ?? DefaultGridLineBrush;
            Canvas.SetLeft(_boardSurface, 0);
            Canvas.SetTop(_boardSurface, 0);
            Panel.SetZIndex(_boardSurface, 0);
            _boardSurface.InvalidateVisual();
        }

        private void UpdateSnakeElements(bool animate)
        {
            var snake = _engine.Snake;

            while (_snakeElements.Count > snake.Count)
            {
                var index = _snakeElements.Count - 1;
                _boardCanvas.Children.Remove(_snakeElements[index]);
                _snakeElements.RemoveAt(index);
            }

            while (_snakeElements.Count < snake.Count)
            {
                var isHead = _snakeElements.Count == 0;
                var element = CreateSnakeElement(isHead);
                _snakeElements.Add(element);
                _boardCanvas.Children.Add(element);
            }

            for (var i = 0; i < snake.Count; i++)
            {
                var isHead = i == 0;
                var element = _snakeElements[i];
                ApplySegmentSizing(element, isHead);
                MoveElementToCell(element, snake[i], isHead ? 0.04 : 0.10, animate);
                Panel.SetZIndex(element, isHead ? 3 : 2);
            }
        }

        private FrameworkElement CreateSnakeElement(bool isHead)
        {
            var source = isHead ? SnakeHeadImage : SnakeBodyImage;

            if (source != null)
            {
                return new Image
                {
                    Source = source,
                    Stretch = Stretch.Uniform,
                    RenderTransformOrigin = new Point(0.5, 0.5)
                };
            }

            return new Border
            {
                Background = isHead
                    ? SnakeHeadBrush ?? DefaultSnakeHeadBrush
                    : SnakeBodyBrush ?? DefaultSnakeBodyBrush,
                BorderBrush = isHead ? DefaultSnakeBodyBrush : null,
                BorderThickness = isHead ? new Thickness(1) : new Thickness(0),
                RenderTransformOrigin = new Point(0.5, 0.5)
            };
        }

        private void ApplySegmentSizing(FrameworkElement element, bool isHead)
        {
            var inset = _cellSize * (isHead ? 0.04 : 0.10);
            var size = Math.Max(1, _cellSize - (inset * 2));

            element.Width = size;
            element.Height = size;

            var border = element as Border;
            if (border != null)
            {
                border.CornerRadius = new CornerRadius(Math.Max(2, size * (isHead ? 0.32 : 0.24)));
            }
        }

        private void UpdateCargoElement(bool animate)
        {
            var cargo = _engine.Cargo;

            if (cargo == null)
            {
                if (_cargoElement != null)
                {
                    _boardCanvas.Children.Remove(_cargoElement);
                    _cargoElement = null;
                }

                _lastCargoPosition = null;
                _lastCargoType = null;
                return;
            }

            var mustCreate =
                _cargoElement == null ||
                !_lastCargoType.HasValue ||
                _lastCargoType.Value != cargo.Type;

            if (mustCreate)
            {
                if (_cargoElement != null)
                {
                    _boardCanvas.Children.Remove(_cargoElement);
                }

                _cargoElement = CreateCargoElement(cargo.Type);
                _boardCanvas.Children.Add(_cargoElement);
            }

            ApplyCargoSizing(_cargoElement);
            MoveElementToCell(_cargoElement, cargo.Position, 0.16, false);
            Panel.SetZIndex(_cargoElement, 4);

            var moved = !_lastCargoPosition.HasValue || !_lastCargoPosition.Value.Equals(cargo.Position);
            if (animate && (mustCreate || moved))
            {
                AnimateCargoAppear(_cargoElement);
            }

            _lastCargoPosition = cargo.Position;
            _lastCargoType = cargo.Type;
        }

        private FrameworkElement CreateCargoElement(CargoType type)
        {
            var source = GetCargoImage(type);

            if (source != null)
            {
                return new Image
                {
                    Source = source,
                    Stretch = Stretch.Uniform,
                    RenderTransformOrigin = new Point(0.5, 0.5),
                    RenderTransform = new ScaleTransform(1, 1)
                };
            }

            return new Border
            {
                Background = GetCargoBrush(type),
                BorderBrush = DefaultOverlayTextBrush,
                BorderThickness = new Thickness(1),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(1, 1)
            };
        }

        private void ApplyCargoSizing(FrameworkElement element)
        {
            var inset = _cellSize * 0.16;
            var size = Math.Max(1, _cellSize - (inset * 2));
            element.Width = size;
            element.Height = size;

            var border = element as Border;
            if (border != null)
            {
                border.CornerRadius = new CornerRadius(Math.Max(2, size * 0.50));
            }
        }

        private ImageSource GetCargoImage(CargoType type)
        {
            switch (type)
            {
                case CargoType.Low:
                    return CargoLowImage;
                case CargoType.Medium:
                    return CargoMediumImage;
                case CargoType.High:
                    return CargoHighImage;
                default:
                    return null;
            }
        }

        private Brush GetCargoBrush(CargoType type)
        {
            switch (type)
            {
                case CargoType.Low:
                    return CargoLowBrush ?? DefaultLowCargoBrush;
                case CargoType.Medium:
                    return CargoMediumBrush ?? DefaultMediumCargoBrush;
                case CargoType.High:
                    return CargoHighBrush ?? DefaultHighCargoBrush;
                default:
                    return DefaultLowCargoBrush;
            }
        }

        private void MoveElementToCell(FrameworkElement element, GridPosition position, double insetFactor, bool animate)
        {
            var inset = _cellSize * insetFactor;
            var targetLeft = (position.Column * _cellSize) + inset;
            var targetTop = (position.Row * _cellSize) + inset;

            if (!animate || double.IsNaN(Canvas.GetLeft(element)) || double.IsNaN(Canvas.GetTop(element)))
            {
                element.BeginAnimation(Canvas.LeftProperty, null);
                element.BeginAnimation(Canvas.TopProperty, null);
                Canvas.SetLeft(element, targetLeft);
                Canvas.SetTop(element, targetTop);
                return;
            }

            var duration = TimeSpan.FromMilliseconds(Math.Min(110, Math.Max(45, TickInterval.TotalMilliseconds * 0.75)));
            var easing = new CubicEase { EasingMode = EasingMode.EaseOut };

            element.BeginAnimation(
                Canvas.LeftProperty,
                new DoubleAnimation(Canvas.GetLeft(element), targetLeft, duration) { EasingFunction = easing });
            element.BeginAnimation(
                Canvas.TopProperty,
                new DoubleAnimation(Canvas.GetTop(element), targetTop, duration) { EasingFunction = easing });
        }

        private void AnimateCargoAppear(FrameworkElement element)
        {
            var duration = TimeSpan.FromMilliseconds(160);
            var easing = new BackEase { Amplitude = 0.28, EasingMode = EasingMode.EaseOut };
            var scale = EnsureScaleTransform(element);

            element.Opacity = 0;
            element.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0, 1, duration));
            scale.BeginAnimation(ScaleTransform.ScaleXProperty, new DoubleAnimation(0.50, 1, duration) { EasingFunction = easing });
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, new DoubleAnimation(0.50, 1, duration) { EasingFunction = easing });
        }

        private void CreateCargoPop(SnakeCargo cargo)
        {
            if (cargo == null || _cellSize <= 0)
            {
                return;
            }

            var size = Math.Max(1, _cellSize * 0.72);
            var pop = new Ellipse
            {
                Width = size,
                Height = size,
                Stroke = GetCargoBrush(cargo.Type),
                StrokeThickness = Math.Max(1, _cellSize * 0.06),
                Opacity = 0.75,
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = new ScaleTransform(0.70, 0.70)
            };

            Canvas.SetLeft(pop, (cargo.Position.Column * _cellSize) + ((_cellSize - size) / 2));
            Canvas.SetTop(pop, (cargo.Position.Row * _cellSize) + ((_cellSize - size) / 2));
            Panel.SetZIndex(pop, 5);
            _boardCanvas.Children.Add(pop);

            var duration = TimeSpan.FromMilliseconds(220);
            var scale = (ScaleTransform)pop.RenderTransform;
            var scaleAnimation = new DoubleAnimation(0.70, 1.70, duration)
            {
                EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
            };
            var fadeAnimation = new DoubleAnimation(0.75, 0, duration);
            fadeAnimation.Completed += (sender, args) => _boardCanvas.Children.Remove(pop);

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
            pop.BeginAnimation(UIElement.OpacityProperty, fadeAnimation);
        }

        private static ScaleTransform EnsureScaleTransform(FrameworkElement element)
        {
            var scale = element.RenderTransform as ScaleTransform;
            if (scale != null)
            {
                return scale;
            }

            scale = new ScaleTransform(1, 1);
            element.RenderTransform = scale;
            element.RenderTransformOrigin = new Point(0.5, 0.5);
            return scale;
        }

        private void ResetVisualElements()
        {
            _boardCanvas.Children.Clear();
            _boardCanvas.Children.Add(_boardSurface);
            _snakeElements.Clear();
            _cargoElement = null;
            _lastCargoPosition = null;
            _lastCargoType = null;
        }

        private void UpdateScoreText()
        {
            _scoreText.Text = "Score " + Score;
        }

        private void UpdateGameOverText()
        {
            if (_engine == null || !_engine.IsGameOver)
            {
                _gameOverText.Visibility = Visibility.Collapsed;
                return;
            }

            _gameOverText.Text = "Game Over" + Environment.NewLine + "Score " + Score;
            _gameOverText.Visibility = Visibility.Visible;
        }

        private sealed class BoardSurface : FrameworkElement
        {
            public int Rows { get; set; }

            public int Columns { get; set; }

            public double CellSize { get; set; }

            public Brush BoardBackground { get; set; }

            public Brush GridLine { get; set; }

            protected override void OnRender(DrawingContext drawingContext)
            {
                base.OnRender(drawingContext);

                var bounds = new Rect(0, 0, RenderSize.Width, RenderSize.Height);
                drawingContext.DrawRectangle(BoardBackground ?? DefaultBoardBackgroundBrush, null, bounds);

                if (Rows <= 0 || Columns <= 0 || CellSize <= 0)
                {
                    return;
                }

                var pen = new Pen(GridLine ?? DefaultGridLineBrush, 1);

                for (var column = 1; column < Columns; column++)
                {
                    var x = column * CellSize;
                    drawingContext.DrawLine(pen, new Point(x, 0), new Point(x, RenderSize.Height));
                }

                for (var row = 1; row < Rows; row++)
                {
                    var y = row * CellSize;
                    drawingContext.DrawLine(pen, new Point(0, y), new Point(RenderSize.Width, y));
                }
            }
        }
    }
}
