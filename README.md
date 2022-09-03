![multiNoa](https://user-images.githubusercontent.com/38633608/145415538-b8cf07df-0c24-4fd3-8136-1d2e0324a548.png "multiNoa Logo")
> **multi**player **n**etworking **o**f **a**ton

I STOPPED WORKING ON multiNOA! Many of it's parts got recycled into new, cleaner libraries (better seperation, simpler design).

## Intro
multiNoa is a highly dynamic networking library for creating **tick-based, tcp-focused, highly modular** multiplayer solutions without the struggle of writing the network layer from the ground up.

multiNoa handles all things low level networking for you, so you can focus on what's most important: The game.

## What multiNoa is **not**
multiNoa isn't a Entity-Manager, a Event System, a engine in any way ...

It's a base layer, designed to be easy to build upon, extenable and reliable.

And: It's carefully designed to fit games, especially minigames with multiple game instances per server instance. You technically could use it for something else (a chat-app, a discord clone), but you might be better of using something else.
It's a great tool for **some** projects and designed to fit those.

## Why the name?
This project is based upon the networking solution I wrote for my indie-game project aton (feel free to wishlist/buy on steam to support me!). I first went with Photon for Unity (PUN), but quickly decided, that I really wanted server-authorized multiplayer and over all felt like any reasonable existing solutions either did not enough or too little to me, or forced me into a structure I did not like. Don't get me wrong, multiNoa also needs a structure, it's just the one I like.

multiNoa is used inside my current game projects and will elvolve with them. It's also coded with high extensibility in mind!
I decided to put it out there, since I feel like there are a lot of solutions, but they either cost money to use, or aren't flexible enough to use them for types of games I like to develope: Small Team/1v1 games with high amount instancing and rooming required, together with quite some matchmaking stuff.

## What multiNoa is
multiNoa may be your best friend when writing a multiplayer game. I won't promise that, but it was just that for me.

multiNoa enables you to write modular server-logic. To stay in control and design everything just as you need it. To enjoy the ease of having the low level stuff worked out. To easily manage connections and clients, authorization, sending, receiving and handling packets.

multiNoa tries to be a big, sturdy base for anything you want to place onto it. It does not force you to use all it offers, but it's there if you need it.

## Wait! No UDP?
Not yet. I am working on it for my current project!


MultiNoa is that flexible, I was able to write a basic chat app with it in around 10 minutes.
