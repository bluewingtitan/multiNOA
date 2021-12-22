![# multiNoa](https://user-images.githubusercontent.com/38633608/145415538-b8cf07df-0c24-4fd3-8136-1d2e0324a548.png)
> **multi**player **n**etworking **o**f **a**ton

STILL HIGHLY IN THE WORKS!

## Intro
multiNoa is a highly dynamic networking library for creating **tick-based, tcp-focused, highly modular** multiplayer solutions without the struggle of writing the network layer from the ground up.

multiNoa handles everything low level for you, so you can focus on what's most important: The gameplay.

## What multiNoa is **not**
multiNoa isn't a Entity-Manager, a Event System, an engine in any way ...

It's a base layer, designed to fit right into your game while being as flexible as possible — so you always are the one having control.

And: It's carefully designed to fit games! You technically could use it for something else, but you might be better of using something else.
It's the perfect tool for **some** projects and designed to fit those.

## Why the name?
This project is based upon the networking solution I wrote for my indie-game project aton (feel free to wishlist/buy on steam to support me!). I first went with Photon for Unity (PUN), but quickly decided, that I really wanted server-authorized multiplayer and maintain the control over my spendings, so cloud hosting myself was the way to go.

multiNoa is used inside aton in this exact form and will evolve with aton and other projects I will use it in.
I decided to put it out there, since I feel like there are a lot of solutions, but they either cost money to use, or aren't flexible enough to use them for types of games like aton.

## What multiNoa is
multiNoa is your best friend when writing a multiplayer game.

multiNoa enables you to write modular server-logic. To stay in control and design everything just as you need it. To enjoy the ease of having the low level stuff worked out.

multiNoa tries to be a big, sturdy base for anything you want to place onto it.

## Wait! No UDP?
Not yet. I am not using UDP in aton* and UDP-Code wouldn't be just a 1:1 copy of the TCP-Code with a few classes switched out, so I will need some time until it will be tested and ready. Before that, I will add an abstraction layer to allow for custom implementations like SteamSockets or UDP to be made by someone else.

\* Why? Because aton only sends most necessary information and simulates everything predictable client-side for visuals and thus really profits from TCPs simplicity in implementation.

## Now. How does it work?
It's easy!
For each type of message you want to send, you define packet struct with it's unique id and define the properties that should be synced with the NetworkProperty Attribute:

```c#
[PacketStruct(Message.PacketId)]
struct Message
{
    public const int PacketId = 15;

    [NetworkProperty]
    public string Message {get; private set;}
    
    [NetworkProperty]
    public int SomeNumber {get; private set;}
}
```

Now you can use it like this:
```c#
var packet = new Message{
    Message = "Message Content",
    SomeNumber = 1293
};
```

And send it like this:
```c#
client.SendData(packet);
```

Want to do some handling?
```c#
[PacketHandler]
public static class Handlers
{
    [HandlerMethod(Message.PacketId)]
    public static void HandleMessage(Message m, ConnectionBase connection)
    {
        Console.WriteLine($"New Message: {m.Message}. Number: {m.SomeNumber}\nFrom: {connection.GetEndpointIp()}");
    }
}
```


MultiNoa is that flexible, I was able to write a basic chat app with it in around 5 minutes.
