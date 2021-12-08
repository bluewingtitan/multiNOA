![# multiNoa](https://github.com/bluewingtitan/multiNOA/raw/master/Resources/BadgeDark.png)
> **multi**player **n**etworking **o**f **a**ton


## Intro
multiNoa is a highly dynamic networking library for creating **tick-based, tcp-focused, highly modular** multi-player solutions without the struggle of writing everything from the ground up.

multiNoa handles everything low level for you, so you can focus on what's most important: The gameplay.

## What multiNoa is **not**
multiNoa isn't a Entity-Manager, a Event System, ...

This is not an engine, it's the base layer that is designed to fit right into your game while being as flexible as possible so you always are the one having control.

And: This is very carefully designed to fit games! You technically could use it for something else, but you might be better of using something more fitting.
It's the perfect tool for **some** projects and designed to fit those.

## Why the name?
This project is based upon the networking solution I wrote for my indie-game project aton (feel free to wishlist/buy on steam to support me!). I first went with Photon for Unity (PUN), but quickly saw, that I really want server-authorized multiplayer.

multiNoa is used inside aton in this exact form and will evolve as long as aton will.
I decided to put it out there, since I feel like there are a lot of solutions, but they either cost money to use, or aren't flexible enough to use them in non-generic games.

## What multiNoa is
multiNoa is your best friend when writing a multiplayer game.

multiNoa enables you to write modular, docker-ready server-logic. To stay in control and design everything just as you need it. To enjoy the ease of having the low level stuff worked out.

multiNoa tries to be a big, sturdy base for anything you want to place onto it.

## Wait! No UDP?
Not yet. I am not using UDP in aton* and UDP-Code wouldn't be just a 1:1 copy of the TCP-Code with a few classes switched out, so I will need some time until it will be tested and ready. Before that, I will add an abstraction layer to allow for custom implementations like SteamSockets or UDP to be made by someone else.

\* Why? Because aton only sends most necessary information and thus really profits from TCPs integrity-checks.
