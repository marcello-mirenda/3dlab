using System.Threading;

namespace _3DLab;

public partial class MainPage : ContentPage
{
	int count = 0;
    private Task gviewTask;
    private readonly CancellationTokenSource _cancellationTokenSource;

    public MainPage()
	{
		InitializeComponent();
        _cancellationTokenSource = new CancellationTokenSource();
    }

	private async Task UpdateGraphicsView()
	{
		while (!_cancellationTokenSource.IsCancellationRequested)
		{
			await Task.Delay(25);
            GView.Invalidate();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
		gviewTask = UpdateGraphicsView();
    }

    protected override void OnDisappearing()
    {
		_cancellationTokenSource.Cancel();
		WaitHandle.WaitAll(new[] { _cancellationTokenSource.Token.WaitHandle });
        base.OnDisappearing();
    }

    private void OnCounterClicked(object sender, EventArgs e)
	{
		count++;

		if (count == 1)
			CounterBtn.Text = $"Clicked {count} time";
		else
			CounterBtn.Text = $"Clicked {count} times";

		SemanticScreenReader.Announce(CounterBtn.Text);
	}
}

