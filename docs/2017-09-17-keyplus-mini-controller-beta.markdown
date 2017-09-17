---
layout: post
title:  "keyplus mini controller prototypes"
date:   2017-09-17
categories: keyboards
---

{% include image.html url="/assests/imgs/first-prototypes/keyplus_mini_beta.jpg" description="The keyplus mini" %}

{% include image.html url="/assests/imgs/first-prototypes/keyplus_mini_profile.jpg" description="Footprint fits under one Cherry MX switch" %}

The keyplus mini is my new controller that I designed for my [wireless split
keyboard project]({{ sitebase.url }}{% post_url 2017-09-14-keyplus-beta %}).
It features a USB Type-C port, ATxmega microcontroller, and 31 GPIO pins. It also has
hardware to automatically switch between USB and battery operation.
It uses dual row 2.0mm pin headers to allow it to fit between Cherry MX switch
footprints. Hardware files and wiring information are [available
here](https://github.com/ahtn/keyboard_pcb/tree/master/keyplus_mini).

The board uses the ATxmega32a4u microcontroller. The ATxmega32a4u is an 8 bit,
32MHz, AVR microprocessor. It's not drop in compatible with the ATmega32u4, but
there's a few good reasons why I chose it over the ATmega32u4:

* It is much more power efficient, capable of running at up to 12MHz down to 1.6V.
* It has an AES coprocessor, which is about 10x faster than a software
  implementation.
* Crystal-less USB operation, for a smaller footprint and less PCB complexity.
* It is also available in 64kb and 128kb flash versions.

## Home cooked PCB's

I used this a chance to try out reflow soldering at home with a stencil. For
the first round, I tried using a second hand toaster oven. The results where
rather catastrophic:

{% include image.html url="/assests/imgs/first-prototypes/keyplus_crispy_panel.jpg" description="" %}
{% include image.html url="/assests/imgs/first-prototypes/this-is-fine.jpg" description="This thing actually enumerated as a USB device." %}

This happened because of hot spots inside the oven and the PCB's in the hottest
part of the oven weren't very happy.  To be honest, I could have mostly avoid
this situation by taking out it earlier, and then reworking the uncooked PCB's
with my hot air gun. But there was a part of me that was curious to see what
would happen...

For my next approach, I used an electric skillet. This went much better and was
much easier to use. However, I still ended up needing to rework some of the
boards because of an issue with my solder paste stencil.

{% include image.html url="/assests/imgs/first-prototypes/keyplus_skillet_panel.jpg" description="" %}

## Production

The next step is to look into organizing professional PCB assembly, so I don't end up
burning down my house. In the meantime I have a some of spare boards from this
run that I'll put up mechmarket for those interested.

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/70mcfb/i_made_a_new_controller_for_custom_keyboards_usb/)
