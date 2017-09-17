---
layout: post
title:  "keyplus first release"
date:   2017-09-14
categories: keyboards
---

{% include image.html url="/assests/imgs/keyplus_logo.svg" description="" %}

I finally chose a name and released the source code for my wireless split
keyboard fimrware: [keyplus](https://github.com/ahtn/keyplus). The code supports
split keyboards with up to 64 devices, which can be connected either wired
or wirelessly (not Bluetooth).  The code keyboard code runs on
ATxmega32a4u and the wireless dongle code runs on an nRF24LU1+. The code is
still in a beta state, but most of the core features are currently implemented. 
[Here's some pictures of my keyboards that use the firmware](https://imgur.com/a/jzdZO)
I'll post more about my work on the controller I use to run the firmware when I
get a chance on the weekend.

## Cool features in my firmware

* **Easy configuration**: All settings that control the keyboard including
  layout, wireless settings and pin matrix are stored in a [configuration
  files](https://github.com/ahtn/keyplus/blob/ccef6c000a91b398498caf56156175fa7fcd5b26/layouts/basic_split_test.yaml).
  It is possible to make a full custom keyboard with no programming knowledge.
  These settings can be update with [a
  configurator](/assests/imgs/first-prototypes/keyplus-windows-first.png). The
  configurator communicates with devices over USB and requires no drivers.
* **Wireless and wired split support with up to 64 devices!** The devices in a
  split keyboard share layer state. However, it's also possible to have
  multiple split keyboards configured that maintain their layer state
  independently of one another.
* **Similar feature set to TMK**: Currently supports media keys, mouse keys,
  nkro, layers, hold keys, sticky keys (called oneshots in TMK/QMK), modkeys
  and macros (not everything is exposed in the configuration file yet though).
* **Improved layer handling**: One thing that bugged me with TMK
  is that changing layers released any keys that were currently held down. When
  changing layers in my firmware, I first check to see if the key is different
  on the old and new layer. Only if it is different is the key actually released.
  With this implementation, it is also possible to achieve the same effect as
  the QMK *tri-layer* key using ordinary layer keys (or even with sticky layer
  keys).
* **Improved modkey handling:** On my layout I have both `!` and `=` mapped
  to separate keys on the same layer. When programming I often type `!=`, but
  TMK would often output this as `!+`. What the `!` key is actually
  doing sending both `shift` and `1` keycodes to the OS and so pressing `=`
  before the `!` is released would result in the OS seeing `shift` and `=` down
  at the same time so it would generate '+' instead of `=`. This is fixed in my
  firmware.
* **Improved sticky key handling:** In my firmware sticky layer and modifier
  keys can be chained together.
* **Improved keycode handling:** In TMK/QMK there are arbitrary restrictions on
  which special keys can be used with other keycodes. For example in my
  firmware, when making a hold key (different keycodes when tapped vs held), I
  have a system in place so that should make it possible to use any keycode.
  For example, it is possible to make a key that is a sticky shift key when
  tapped and executes a macro when held.
* **Standard USB HID keyboard:** Since the wireless dongle is also just a USB HID
  keyboard, it is possible to use the keyboard in BIOS unlike a Bluetooth
  keyboard. Each device can act as a standalone USB keyboard.
* **Wireless mouse support:** It is possible to pair and use Logitech Unifying
  mice to the keyboards and dongles.
* **Low latency**: In wired split mode, my firmware can achieve [1ms input
  latency](/assests/imgs/latency-tests/my-i2c-remote-key.png) the maximum
  possible for USB keyboards. In wireless split mode, typical latency should be
  under 3ms (unverified). As a comparison a keyboard (non-split) using the
  default scanning algorithm used in QMK has an input latency of at least 5ms.
* **Power efficient**: With just a CR2032 battery, it should be possible to get 3-6
  months battery life with moderate use. The standby time should be several years.
* **MIT software License**: Most of the other popular keyboard firmware (TMK/QMK,
  EasyAVR, kiibohd) are licensed under GPL. This can be an issue as
  microcontroller vendors often release their code under licenses that
  are incompatible with GPL. So it is against the terms of the GPL to
  redistribute binaries that use such code. As an example, Nordic's Bluetooth
  SDK's use GPL incompatible licenses.

## Why write another keyboard firmware

There's several good open source keyboard firmware projects around, so you might
wonder why I went to the effort to write my own. The first version of my split
keyboard firmware was originally based on [TMK](https://github.com/tmk/tmk_keyboard),
but for a couple of reasons I wanted to write my own:

* In TMK the keyboard layout is built into the firmware at compile time. So
  changes to layout required recompiling. I wanted my firmware to be able to
  be generated without needing to compile code and allow the firmware to be
  updated independently of the keyboard layout (this is important, as it will
  be easier to apply security updates for the wireless protocol).
* I like the general idea of how TMK handles layers, but I found the
  implementation has lots of edge cases. Particular how special layer and
  modifier keycodes interact with each other.
* My wireless receiver code runs on an nRF24LU1+ which uses an 8051 processor.
  Porting TMK to the 8051 using the SDCC compiler would be hard, potential
  requiring modifications to nearly every file in TMK.
* For the learning experience. While working on this project I learnt a lot
  about embedded programming and hardware, assembly programming, reverse
  engineering, electrical engineering and PCB design.

[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/702kxr/introducing_keyplus_my_new_wireless_split/)
