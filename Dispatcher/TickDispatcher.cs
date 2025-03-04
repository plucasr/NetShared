namespace Shared;

public class TickDispatcher
{
    private Timer timer;
    private int intervalMilliseconds;
    private Action tickAction;

    public TickDispatcher(int intervalMilliseconds, Action tickAction)
    {
        this.intervalMilliseconds = intervalMilliseconds;
        this.tickAction = tickAction;

        // Initialize the timer with the tickAction as the callback
        timer = new Timer(TickCallback, null, Timeout.Infinite, Timeout.Infinite);
    }

    public void Start()
    {
        // Start the timer to invoke the tickAction at the specified interval
        timer.Change(0, intervalMilliseconds);
    }

    public void Stop()
    {
        // Stop the timer
        timer.Change(Timeout.Infinite, Timeout.Infinite);
    }

    private void TickCallback(object? state)
    {
        // Invoke the tickAction
        tickAction.Invoke();
    }
}
