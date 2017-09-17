---
layout: post
title:  "Measuring wired split keyboard input latency"
date:   2017-04-16
categories: keyboards
---

## How to use a logic analyzer to measure input latency
A [logic analyzer](https://en.wikipedia.org/wiki/Logic_analyzer) is like an
oscilloscope except it can only measure digital voltage levels.  Since it only
needs to detect two voltage levels, the circuitry is simpler and hence cheaper
to manufacture than an analog oscilloscope.  The one I bought was about $10 and
has a 24MHz sample rate, 8 channels and connects to your computer via USB.  I
used [sigrok](https://sigrok.org/) an open source program for working
with logic analyzers and oscilloscopes to record data from my computer.

{% include image.html url="/assests/imgs/latency-tests/logic-analyzer.jpg" description="A simple USB logic analyzer." %}

To measure input latency of a keyboard, we want to use the logic analyzer to
record 3 kind different kinds of events

1. When a key is pressed or released.
2. When the firmware detects that the key has changed.
3. When the firmware passes information over USB.

Measuring 1 is easy as we can just connect a probe to a column of the key matrix
and set all the rows of the matrix low when we are not scanning the matrix. Then
when any key in that column is pressed, the probe will see the signal pulled low.
We can measuring 2 and 3 with a logic analyzer by connecting probes to any of the
spare IO pins, and then modify the firmware to toggle those pins whenever the events
occur.

Also, keep in mind that when measuring input latency, that the input latency
is limited by the poll interval of a USB endpoint. For full speed USB2.0
devices the minimum poll interval is 1ms. So reducing the latency below 0.5ms
won't have much of an effect because our reports will on average always be
ready before the USB endpoint is.


## Measuring input latency of tmk/qmk's Let's Split keyboard

The [Let's Split](https://github.com/qmk/qmk_firmware/tree/master/keyboards/lets_split)
is a split keyboard that use two pro micro's, one in each half. One controller connects
to the host via USB, while the other scans its matrix and transmits it to the host side
over either a serial or an i2c connection.

### Let's Split serial
Let's Split firmware uses a simple bi-directional, one-wire serial protocol at about 40k baud
implemented in software. The master polls the slave for it's key matrix in
between scans of its own matrix. Here's what a typical serial transaction looks
like:

{% include image.html url="/assests/imgs/latency-tests/ls-serial.png" description="Let's Split serial transaction" %}

### Let's Split serial input latency

The [setup I used](/assests/imgs/latency-tests/ls-setup.jpg) was two pro micro's on a breadboard
each connected to an external matrix. Here's the results from recording a typical
key press on the master and slave sides:

{% include image.html url="/assests/imgs/latency-tests/ls-serial-local-key.png" description="Let's Split master key press using serial" %}

{% include image.html url="/assests/imgs/latency-tests/ls-serial-remote-key.png" description="Let's Split slave key press using serial" %}

As we can see the latency is about 15ms when the key is pressed from the master
side, and 30ms when the key comes from the slave side. The latency is this bad
primarily because the master polls the whole matrix state of the slave every
iteration of the main loop.

We can also see that the slave key presses take twice as long to register as the
master. This is because the master's contiuous polling of the slave brings its
scanning algorithm to a complete crawl.

An easy fix for the Let's Split serial code would be to make it no longer rely
on the master polling the slave, but instead have the "slave" be the one
sending the messages whenever its matrix state changes. This way once the
matrix change is detected, it would only about 2ms before the master is
informed of the change.

### Let's split I²C
Let's split code also supports an optional I²C mode. The I²C mode operates at
400k baud. However, because I was using a stale version of the repository, the
version I tested was only operating at 100k baud (I only realized this after
I had made the measurements).

Here's what a typical I²C transaction looks like:

{% include image.html url="/assests/imgs/latency-tests/ls-i2c.png" description="Let's Split i2c transaction" %}

### Let's Split I²C input latency

Using the same test setup as before, here's the results of the I²C mode at 100k baud.

{% include image.html url="/assests/imgs/latency-tests/ls-i2c-local-key.png" description="Let's Split master key press with i2c" %}

{% include image.html url="/assests/imgs/latency-tests/ls-i2c-remote-key.png" description="Let's Split slave key press with i2c" %}

It's a bit strange this time as the slave key presses actually tend to register
faster (7ms) than master key presses (~10ms).  Again the reason for is because
of the interplay between the debounce algorithm and the master continuously
polling the slave. But most of the latency here is due to the debouncing algorithm
used. And if I had used I²C at 400k baud, the debouncing algorithm would be the
primary source of input latency.


## Measuring input latency of my controller in wired mode

The controller I'm working on (I really need to come up with a name for it), also supports split
wired keyboards via I²C. It uses hardware I²C of the atxmega32a4u, at 400k baud.
It uses an interrupt based approach and only sends the minimal amount of information
needed to convey changes in matrix state. Here's what the latency looks like for
my controller with [the setup I used](/assests/imgs/latency-tests/my-controler-setup.jpg):

{% include image.html url="/assests/imgs/latency-tests/my-i2c-local-key.png" description="My controller local key press." %}

{% include image.html url="/assests/imgs/latency-tests/my-i2c-remote-key.png" description="My controller foreign key press." %}

The debouncing algorithm that I used reasons that, if we know a switch is not bouncing,
then detecting a change of state indicates the key has been pressed/released. Further
changes are then not accepted for that key are then not accepted until the debouncing
period is over.

## Conclusion

It goes to show that it's hard to know exactly how well you code performs unless
you have a concrete way of profiling it. Also, the debouncing algorithm used in
a keyboard can have a big effect on input latency.

Next time I'll look at the latency of my controller in wireless split mode.

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/65pa0p/keyboard_science_measuring_the_input_latency_of/)
