# Rooms
Rooms are unity that are used to organize clients.
Each Server consists out of one base room that clients normally will get put in (in all default implementations, that is) after connecting.

Each Room is Updatable and may use the thread of the server it's contained in, or use their own in case for games.

A IClient-Instance is only ever able to be in one room, a Room is only ever able to exist in the context of one IServer-Instance.
MultiNoa does not offer extended API for room management, as different games may have completely different needs in this regard.