---
layout: post
title: "Take page screenshots using Devtools"
description: "Taking all kind of screenshots using Devtools"
comments: true
keywords: "tools"
---

There's been times that I needed to show a client or a teammate a full screenshot of the current state of the website or even a part of it, so I've been using [Full Page Screen Capture](https://chrome.google.com/webstore/detail/full-page-screen-capture/fdpohaocaechififmbbbbbknoalclacl?hl=en) and, for any other kind of screenshots, [Windows' Snippet tool](https://support.microsoft.com/en-us/help/13776/windows-use-snipping-tool-to-capture-screenshots) or [Snaggit](https://www.techsmith.com/store/snagit). If I want to host it online like in this post [Gyazo](https://gyazo.com), which also has a GIF maker.

Recently I found that **Devtools** has a tool that does a full-page screenshot, no extension needed!

This is not new thought, it was included in the [Devtools update in April 2017](https://developers.google.com/web/updates/2017/04/devtools-release-notes), *but looks like I live under a rock and didn't find out until now...*

## Devtools

As stated in the [official Devtools page](https://developers.google.com/web/tools/chrome-devtools/?hl=en):

> Chrome DevTools is a set of web developer tools built directly into the Google Chrome browser. DevTools can help you edit pages on-the-fly and diagnose problems quickly, which ultimately helps you build better websites, faster.

## Running commands in Devtools

You already know how to open Devtools but in case you just forgot and *for the sake of this post*, open it using the key `F12` or the shortcut `Ctrl + Shift + I`. Then you have to open the `Run command` in the menu inside Devtools or use `Ctrl + Shift + P`.

[![Image from Gyazo](https://i.gyazo.com/8209d7d3132efba1843a3d51e4ad2183.gif)](https://gyazo.com/8209d7d3132efba1843a3d51e4ad2183)


## Screenshot tool

[![Image from Gyazo](https://i.gyazo.com/25ea6c9d258a1117efca5a2d92f715e2.gif)](https://gyazo.com/25ea6c9d258a1117efca5a2d92f715e2)

If you type `screenshot` it will filter the commands, there will be 4 types of screenshots methods that you can use:

1. Area screenshot
2. Full size screenshot
3. Node screenshot
4. Capture screenshot

Before checking all the screenshots methods, after devtools finishes processing the image, **it will automatically download it with the name of the webpage**.

[![Image from Gyazo](https://i.gyazo.com/89a4935eb0ddb1a06ae997551fd19677.gif)](https://gyazo.com/89a4935eb0ddb1a06ae997551fd19677)

[![Image from Gyazo](https://i.gyazo.com/e2e29c7fd7e90bb8bfd36ef130793394.png)](https://gyazo.com/e2e29c7fd7e90bb8bfd36ef130793394)

### Area screenshot

The `area screenshot` will let you select a part of the website and make an screenshot.

[![Image from Gyazo](https://i.gyazo.com/f508a479893b6e8af9234d6a77404b26.gif)](https://gyazo.com/f508a479893b6e8af9234d6a77404b26)

Result:

[![Image from Gyazo](https://i.gyazo.com/2b096e1ca2974157e478587a4902c1d2.png)](https://gyazo.com/2b096e1ca2974157e478587a4902c1d2)

### Full size screenshot

The `full size screenshot` will take a screenshot of the entire webpage, from start to bottom.

[![Image from Gyazo](https://i.gyazo.com/e6c30b8eafbef04091bf66c16429017c.gif)](https://gyazo.com/e6c30b8eafbef04091bf66c16429017c)


Result:

[![Image from Gyazo](https://i.gyazo.com/9519ca32c0029a36432b700268741280.png)](https://gyazo.com/9519ca32c0029a36432b700268741280)

### Node screenshot

The `node screenshot` will take let you make a screenshot of a selected node in the inspect element.

[![Image from Gyazo](https://i.gyazo.com/5b936026735964bb789c4bcae02bb792.gif)](https://gyazo.com/5b936026735964bb789c4bcae02bb792)

Result:

[![Image from Gyazo](https://i.gyazo.com/56089c3d89f9b059dfb018d18572276d.png)](https://gyazo.com/56089c3d89f9b059dfb018d18572276d)

### Capture screenshot

The `capture screenshot` will take a screenshot of the current page without scrolling and the size of the browser.

[![Image from Gyazo](https://i.gyazo.com/6dc4f654e8f218e44fcc1fb51ce4c9f5.gif)](https://gyazo.com/6dc4f654e8f218e44fcc1fb51ce4c9f5)

Result:

[![Image from Gyazo](https://i.gyazo.com/3f15d4a004cba6152d1f9606c20540cc.png)](https://gyazo.com/3f15d4a004cba6152d1f9606c20540cc)





