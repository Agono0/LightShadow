using System;

namespace LightShadow.Contrib.Abdelraouf.Overlay
{
    // One holder of the player input lock. Whoever acquires a token is the only
    // one who can release it, so closing one overlay can no longer clear the
    // lock another overlay still needs (audit v3 5.1).
    public sealed class InputLockToken
    {
        public string Owner { get; private set; }

        // True when the overlay needs the mouse pointer (menus, journal,
        // puzzle panels). Pause reads this so closing it does not hide the
        // cursor while a pointer-driven overlay is still up.
        public bool WantsFreeCursor { get; private set; }

        // True when Esc should close this overlay. Pause does not open on Esc
        // while a dismissable overlay is up.
        public bool Dismissable { get; private set; }

        public bool IsActive { get; internal set; }

        // How the overlay closes itself when something else (respawn, a menu
        // jump) has to clear the stack. May be null.
        internal Action CloseCallback { get; private set; }

        internal InputLockToken(string owner, bool wantsFreeCursor, bool dismissable, Action closeCallback)
        {
            Owner = string.IsNullOrEmpty(owner) ? "unknown" : owner;
            WantsFreeCursor = wantsFreeCursor;
            Dismissable = dismissable;
            CloseCallback = closeCallback;
            IsActive = true;
        }
    }
}
