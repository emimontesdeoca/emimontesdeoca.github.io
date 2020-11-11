---
layout: post
title: "Configure the Apple Magic Keyboard 2 on Windows 10"
description: "Make use of all the features of the AMK2 on Windows 10"
comments: true
keywords: "tutorial"
---

[![Image from Gyazo](https://i.gyazo.com/9c8641cdd22bd528b2141bad1322c74a.jpg)](https://gyazo.com/9c8641cdd22bd528b2141bad1322c74a)


Just yesterday I bought an Apple Magic Keyboard 2, even if I have like 5 mechanical keyboards, because I wanted to try it and it was wireless. 

My main OS is Windows 10, I love it and I don't want to change it so with that in mind I knew that it would be necessary to do some things to make the keyboard work perfectly. I know how Apple works and how they like to keep their devices to their ecosystem.

## Issues

If you pair the keyboard, you will recognize a few things:

* Function keys don't work
* Some keys are mapped wrong (this happened to me in the spanish version)

## Documentation

In order to make it work, I had to read a lot from the web, but these two links are the ones that helped me to make it work:

*  [Make full use of Apple Magic Keyboard/Mouse/Trackpad on Windows](https://www.bluetoothgoodies.com/info/apple-devices/)
* [How do I use my f-1 - f12 keys without pressing FN on Windows 7 using bootcamp on a Macbook Pro?](https://superuser.com/questions/82826/how-do-i-use-my-f-1-f12-keys-without-pressing-fn-on-windows-7-using-bootcamp-o) 

## Installing the Apple Keyboard driver

Some of this steps are from the documentation noted before:

1. Install 7zip (https://www.7-zip.org/) to your computer if you don't have it.
2. Install Python (version 2.x) (https://www.python.org/downloads/) to your computer if you don't have it.
     * IMPORTANT: The latest version of the Python is 3.x. But, you need version 2.x because the brigadier script is not compatible with version 3.x.
     * (option) The installer, by default, doesn't add python.exe to your PATH. If you want, you need to enable this option. (see the screenshot on the right)
If you already have another version of Python, you probably don't want to enable this option.
3. Download brigadier (a Python script that helps you to download the latest Boot Camp version).
4. Please right-click the following link and save the file using "Save link as...". https://raw.githubusercontent.com/timsutton/brigadier/master/brigadier
5. Open command prompt window (aka. DOS box) and change directory to where you downloaded the brigadier script.
6. Assuming the brigadier script was saved as “brigadier.txt”, please run the following command:
7. If Python version 2.x is in your PATH: python brigadier.txt --model=MacBook13,2
    * Otherwise: [Path to the Python version 2.x]\python.exe brigadier.txt --model=MacBook13,2
8. It will download a big bundle with all the drivers from bootcamp
9. Create a folder called `BootCamp` and copy the `BootCamp-xxx-yyyyyy\BootCamp\Drivers\Apple\BootCamp.msi` and  `BootCamp-xxx-yyyyyy\BootCamp\Drivers\Apple\AppleKeyboardMagic2` into it.
10. Run an admin powershell and execute the `BootCamp.msi`, it will install some stuff but we need to update the driver using the contents of the `AppleKeyboardMagic2` folder
11. Start Device Manager (`devmgmt.msc`)
12. Expand `Human Interface Devices` node
13. Look for `Bluetooth HID Device`
14. Update the driver using the contents of the `AppleKeyboardMagic2` folder
15. Reboot computer


You should see the bluetooth keyboard now detected as a Apple Keyboard

[![Image from Gyazo](https://i.gyazo.com/278f6bd3e419d6688ccfadf6918ff309.png)](https://gyazo.com/278f6bd3e419d6688ccfadf6918ff309)

### Update FN keys behavior

If you installed everything properly you will notice that the FN keys are enabled by default, it means that you need to press `fn` + `F5` to actually press the `F5` button.

In order to fix this, I found a solution, noted in the documentation section, that works by changing some entry in the regedit.

1. Open regedit
2. Go to `HKEY_CURRENT_USER\SOFTWARE\Apple Inc.\Apple Keyboard Support`
3. Create or update `OSXFnBehavior` andset it to `0`
4. Reboot computer

### Update keys mapping

If you have a problem with the mappings, you can use [SharpKeys](https://www.randyrants.com/category/sharpkeys/) and update them.

[![Image from Gyazo](https://i.gyazo.com/ee0301205ffeddaae4241db40002864d.png)](https://gyazo.com/ee0301205ffeddaae4241db40002864d)

It's really simple to use, but remember to logout or restart the computer to enable the updates, since it updates the registry.

For my case, I had to update the `Windows`, `alt`, `º` and `<>` keys.




