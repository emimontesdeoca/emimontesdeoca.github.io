---
layout: post
title:  "Split keyboard firmware configuration"
date:   2017-06-29
categories: keyboards
---

Hi just a quick update on how things are going. I made progress on getting the
dual USB-C board working, but at the moment I'm stuck waiting on Cypress's
customer support. I'm pretty sure I've hit a bug in the closed source component
of their SDK, so I don't think I'll be able to make it work without their help.
I've been waiting for about 3 weeks now with no response from them >\_<.


So in the mean time I've been working on the configurator for my wireless/wired
split keyboard firmware (xmega + nRF24 chip set).  The goal of my firmware is
to allow complex configurations without the need to compile the keyboard
firmware. Instead the configuration is loaded onto the keyboard via a USB
interface.  I'm using yaml files to store the configuration: [example (wip)](https://gist.github.com/ahtn/2b84ad3854003539d54172f100a215ca).
I might make a gui configurator later.

{% include image.html url="/assests/imgs/first-prototypes/split-config-connectivity.png" description="Split layout configuration example, with a two way split layout and a numpad." %}

Here's a list of things it supports so far:

* An arbitrary number of devices in a split layout.
* The devices can be used wirelessly or wired together using i2c. When a wired
  device is unplugged it can automatically switch to wireless mode. The wireless
  devices communicate to a USB dongle (nRF24lu1+), or another keyboard device
  that is connected via USB.
* Multiple independent keyboards that maintain their own layer state.
* It's possible to pair Logitech Unifying mice to the configuration.
* How the keyboard matrix is wired is controlled by the configuration file and is
  not hard coded into the firmware.
* The layout and configuration is stored in flash. There will probably be about
  10kb of free space on the (nRF24lu1+ / ATxmega32u4). Layouts need about 2
  bytes per a key per a layer. Can also use 64kb and 128kb versions of the
  ATxmega-a4u for more storage space. Multiple copies of the same keyboard
  can be mapped to the same layout to save storage space.

I still haven't thought of a good name for my firmware yet, so if you have any
good suggestions let me now :)

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/6k8ce3/update_on_wireless_split_keyboard_firmware/)
