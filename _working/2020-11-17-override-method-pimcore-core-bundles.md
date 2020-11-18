---
layout: post
title: "Overriding the core bundles of Pimcore"
description: "Learn how to override controllers and javascript in Pimcore"
comments: true
keywords: "tutorial"
---


I've been working on a Pimcore project for some time now, never touched PHP and Symfony in my life so it's been a great challenge.

The documentation is great, I mean, seriously it's great, but looks like it's not made for beginners on the langauge/framework so everything that I've done I had to document it.

After learning that you can extend Pimcore by using bundles, I've spent hours and hours trying to override controllers, javascript methods and more stuff. 

So in this post I'll explain how I've managed to override controller and Javascript methods.

# Creating the bundle

First of all we need to create a bundle for it, it's pretty simple as its docuemented on the [Pimcore documentation](https://pimcore.com/docs/pimcore/current/Development_Documentation/Extending_Pimcore/Bundle_Developers_Guide/index.html), we just need to run `bin/console pimcore:generate:bundle --namespace=EmiDemo/EmiDemoBundle` on the project folder.

It will prompt some questions but nothing to worry about.

<div style="text-align:center"><img src="https://i.gyazo.com/9aa04169e668506d18638388d0061910.png" /></div>

It will create a new folder under the `src` folder with the namespace that we declared before, and inside we will have all the necessary files for the bundle.

<div style="text-align:center"><img src="https://i.gyazo.com/4d3329c303bb43012faaae43290c613b.png" /></div>



Also, the plugin will be detected by the Pimcore admin site, but it will be disabled, you have to enable it to start using it.

<div style="text-align:center"><img src="https://i.gyazo.com/e0170db9111df69a00bdb3f10473c9a7.png" /></div>

# Overriding a method from a controller

For overriding a method in a controller, first obviously you need to find the action that you want to update, this seems easy but I'll give you the way I usually do.




