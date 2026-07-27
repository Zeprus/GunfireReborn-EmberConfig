namespace SettingsLib.Tests;

using System;
using System.Collections.Generic;
using SettingsLib.Core;
using Xunit;

public class InputDispatcherTests
{
    [Fact]
    public void Poll_InvokesOnPressed_WhenPrimaryKeyDown()
    {
        bool pressed = false;
        var entry = new FakeKeybindEntry(42, null, () => pressed = true, null);
        var dispatcher = new InputDispatcher(() => new[] { entry }, k => k == 42, _ => false);

        dispatcher.Poll(true);

        Assert.True(pressed);
    }

    [Fact]
    public void Poll_InvokesOnPressed_WhenSecondaryKeyDown()
    {
        bool pressed = false;
        var entry = new FakeKeybindEntry(1, 42, () => pressed = true, null);
        var dispatcher = new InputDispatcher(() => new[] { entry }, k => k == 42, _ => false);

        dispatcher.Poll(true);

        Assert.True(pressed);
    }

    [Fact]
    public void Poll_InvokesOnReleased_WhenPrimaryKeyUp()
    {
        bool released = false;
        var entry = new FakeKeybindEntry(42, null, null, () => released = true);
        var dispatcher = new InputDispatcher(() => new[] { entry }, _ => false, k => k == 42);

        dispatcher.Poll(true);

        Assert.True(released);
    }

    [Fact]
    public void Poll_DoesNotInvoke_WhenNoMatch()
    {
        bool pressed = false;
        var entry = new FakeKeybindEntry(42, null, () => pressed = true, null);
        var dispatcher = new InputDispatcher(() => new[] { entry }, _ => false, _ => false);

        dispatcher.Poll(true);

        Assert.False(pressed);
    }

    [Fact]
    public void Poll_DoesNotInvoke_WhenPrimaryIsNone()
    {
        bool pressed = false;
        var entry = new FakeKeybindEntry(0, null, () => pressed = true, null);
        var dispatcher = new InputDispatcher(() => new[] { entry }, _ => true, _ => false);

        dispatcher.Poll(true);

        Assert.False(pressed);
    }

    [Fact]
    public void Poll_DoesNotInvoke_WhenCanDispatchIsFalse()
    {
        bool pressed = false;
        var entry = new FakeKeybindEntry(42, null, () => pressed = true, null);
        var dispatcher = new InputDispatcher(() => new[] { entry }, k => k == 42, _ => false);

        dispatcher.Poll(false);

        Assert.False(pressed);
    }

    [Fact]
    public void Poll_PicksUpKeybindsAddedAfterConstruction()
    {
        var keybinds = new List<IKeybindEntry>();
        bool pressed = false;
        var dispatcher = new InputDispatcher(() => keybinds, k => k == 42, _ => false);

        keybinds.Add(new FakeKeybindEntry(42, null, () => pressed = true, null));
        dispatcher.Poll(true);

        Assert.True(pressed);
    }

    private sealed class FakeKeybindEntry : IKeybindEntry
    {
        public string Label { get; }
        public int PrimaryKeyCodeValue { get; }
        public int? SecondaryKeyCodeValue { get; }
        public Action? OnPressed { get; }
        public Action? OnReleased { get; }

        public FakeKeybindEntry(int primary, int? secondary, Action? onPressed, Action? onReleased, string label = "test")
        {
            Label = label;
            PrimaryKeyCodeValue = primary;
            SecondaryKeyCodeValue = secondary;
            OnPressed = onPressed;
            OnReleased = onReleased;
        }
    }
}
