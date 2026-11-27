using System;
using System.Collections.Generic;

namespace LightShadow.Contrib.Abdelraouf.Overlay
{
    // The bookkeeping behind OverlayStack, with no UnityEngine types so EditMode
    // tests can drive it directly. The caller passes the frame number.
    public sealed class OverlayLockSet
    {
        readonly List<InputLockToken> tokens = new List<InputLockToken>();
        int closedFrame = int.MinValue;

        public int Count => tokens.Count;

        public bool WantsFreeCursor
        {
            get
            {
                for (int i = 0; i < tokens.Count; i++) if (tokens[i].WantsFreeCursor) return true;
                return false;
            }
        }

        public bool AnyDismissable
        {
            get
            {
                for (int i = 0; i < tokens.Count; i++) if (tokens[i].Dismissable) return true;
                return false;
            }
        }

        public InputLockToken Acquire(string owner, bool wantsFreeCursor, bool dismissable, Action closeCallback)
        {
            var token = new InputLockToken(owner, wantsFreeCursor, dismissable, closeCallback);
            tokens.Add(token);
            return token;
        }

        // Idempotent. A token that is not in the set (released twice, or
        // dropped by Clear) changes nothing, so it can never free a lock that
        // belongs to someone else.
        public bool Release(InputLockToken token, int frame)
        {
            if (token == null || !token.IsActive) return false;
            token.IsActive = false;
            bool removed = tokens.Remove(token);
            if (removed) closedFrame = frame;
            return removed;
        }

        public bool ClosedOnFrame(int frame)
        {
            return closedFrame == frame;
        }

        public bool Holds(string owner)
        {
            for (int i = 0; i < tokens.Count; i++) if (tokens[i].Owner == owner) return true;
            return false;
        }

        // Closes the most recently opened dismissable overlay through its own
        // callback. Returns true if one was asked to close.
        public bool TryCloseTop()
        {
            for (int i = tokens.Count - 1; i >= 0; i--)
            {
                InputLockToken token = tokens[i];
                if (!token.Dismissable || token.CloseCallback == null) continue;
                token.CloseCallback();
                return true;
            }
            return false;
        }

        // Asks every dismissable overlay to close itself, then drops whatever
        // is still held. Returns how many tokens were dropped without closing.
        public int Clear(bool invokeCallbacks, int frame)
        {
            if (invokeCallbacks)
            {
                var snapshot = new List<InputLockToken>(tokens);
                for (int i = snapshot.Count - 1; i >= 0; i--)
                {
                    InputLockToken token = snapshot[i];
                    if (token.IsActive && token.Dismissable && token.CloseCallback != null) token.CloseCallback();
                }
            }

            int dropped = tokens.Count;
            for (int i = 0; i < tokens.Count; i++) tokens[i].IsActive = false;
            tokens.Clear();
            closedFrame = frame;
            return dropped;
        }
    }
}
