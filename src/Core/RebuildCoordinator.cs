namespace SettingsLib.Core;

/// <summary>
/// Coordinates deferred rebuilds of the settings panel. Rebuilds are delayed
/// by a few frames when the panel is already open, or executed immediately
/// once it becomes open and the UI has been captured.
/// </summary>
internal sealed class RebuildCoordinator
{
    private bool pending;
    private int delay = -1;

    /// <summary>
    /// Requests a rebuild. If the panel is already open and ready, a short
    /// frame delay is used to debounce rapid changes.
    /// </summary>
    /// <param name="isOpen">Whether the settings panel is currently open.</param>
    /// <param name="isReady">Whether the UI finder has captured the panel.</param>
    public void RequestRebuild(bool isOpen, bool isReady)
    {
        if (isOpen && isReady)
        {
            delay = 3;
            pending = false;
        }
        else
        {
            pending = true;
            delay = -1;
        }
    }

    /// <summary>
    /// Checks whether a rebuild should be triggered this frame.
    /// </summary>
    /// <param name="isOpen">Whether the settings panel is currently open.</param>
    /// <param name="isReady">Whether the UI finder has captured the panel.</param>
    /// <returns><c>true</c> if the caller should rebuild now.</returns>
    public bool TryRebuild(bool isOpen, bool isReady)
    {
        if (delay > 0)
        {
            delay--;
            if (delay == 0 && isOpen && isReady)
            {
                delay = -1;
                return true;
            }

            return false;
        }

        if (delay == 0)
        {
            delay = -1;
            if (pending && isOpen && isReady)
            {
                pending = false;
                return true;
            }

            return false;
        }

        if (pending && isOpen && isReady)
        {
            pending = false;
            return true;
        }

        return false;
    }
}
