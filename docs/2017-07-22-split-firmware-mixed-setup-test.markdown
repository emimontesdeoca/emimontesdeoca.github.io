---
layout: post
title:  "Mixed wired wireless demo"
date:   2017-07-22
categories: keyboards
---

I got the basis of the mixed connectivity code working now. Here's a demo
of 5 keyboards (3 wireless and 2 wired), and one wireless
mouse, all connected together. The keyboard on the right with the USB cable is acting as the wireless
receiver. The mouse is also wireless connected to the receiving keyboard.

{% include image.html url="/assests/imgs/first-prototypes/mixed-connectivity-demo.jpg" description="Five way mixed wired/wireless setup." %}

At the moment you can connect up to 64 devices at once. In theory you
could have up to 255 devices connected at once; however, because the nRF24lu1+ dongle
only has 2kB of RAM, I decided to cap this at 64 for now.

# Layout and firmware flasher

I also started working on the software for the layout and firmware flasher:

{% include image.html url="/assests/imgs/first-prototypes/programmer-demo.png" description="Layout and firmware flasher demo" %}

{% include image.html url="/assests/imgs/first-prototypes/editor-demo.png" description="Very rough draft of the layout editor" %}


[Comments on reddit](https://www.reddit.com/r/MechanicalKeyboards/comments/6ou3se/mixed_wired_wireless_demo/)
