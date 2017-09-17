---
layout: post
title:  "First wireless split prototypes"
date:   2017-04-06
categories: keyboards
---

I got the first wireless split PCB's today.

{% include image.html url="/assests/imgs/first-prototypes/usb-micro.jpg" description="USB-micro Version" %}

The USB-micro version turned out well.  Uploaded some code and everything seems
to be working so far. There's a couple small adjustments I'd like to make to
the PCB but nothing major.


{% include image.html url="/assests/imgs/first-prototypes/usb-c.jpg" description="USB-C Version" %}

In the USB-C version, I didn't realize that USB-C cables are "cross-over" cables
with respect to the pins `A2,A3 -> B10,B11` and `B2,B3 -> A10,A11`. So I ended up
incorrectly assigning the pins for the i2c interconnect.
Anyway for the next iteration, I'm going try do things a bit differently. What I'm
planning will make it so:

* It won't matter what side you plug things in. (i.e. either side can connect to a PC, and either side can be used for daisy chaining)
* I'll be do things in a way that properly comply to the USB standard.
* (*maybe*) Control multiple hosts using the ports at the either end of the daisy chain.
* (*maybe*) Use the spare USB-C port as a charging port.


## What's next

Now that I've got a couple of controllers available, I'll start adding the rest
of the i2c features (at the moment the code only works for two devices connected
via i2c), and I'll give some of the RF features some more thorough testing.

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/644sve/first_wireless_split_controller_prototypes/)
