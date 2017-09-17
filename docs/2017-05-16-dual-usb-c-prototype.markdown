---
layout: post
title:  "Dual USB-C split keyboard prototype"
date:   2017-05-16
categories: keyboards
---

{% include image.html url="/assests/imgs/first-prototypes/dual-usb-type-c.jpg" description="Dual USB-C split keyboard controller" %}

I redesigned the split keyboard controller to use two USB-C ports. The design
allows for the controllers to be daisy chained with one another with one device
on the end of the chain connected to the host. The device connected to the host
uses USB 2.0. The rest of the devices use [USB-C alternate
modes](https://en.wikipedia.org/wiki/USB-C#Alternate_Mode_partner_specifications)
to reconfigure some of the USB-C data lines to carry I2C for board-to-board
communication.

Also the pins of the TRRS connector are shorted together to the adjacent pins
when the cable is inserted and removed, which means its unsafe to plug/unplug
the device while it is powered. I thought I could design around this, but I
couldn't think of a way to do so easily. Using USB-C avoids this issue.

A lot of the components in this design are probably unnecessary, so I should be
able to shrink the form factor quite a bit.

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/6bh648/new_controller_prototypes_for_my_dual_usb_c_split/)
