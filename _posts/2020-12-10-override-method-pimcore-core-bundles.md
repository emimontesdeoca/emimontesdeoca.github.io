---
layout: post
title: "Overriding a core controller of Pimcore"
description: "Learn how to override controllers in Pimcore"
comments: true
keywords: "tutorial"
---


I've been working on a Pimcore project for some time now, never touched PHP and Symfony in my life so it's been a great challenge.

The documentation is great, I mean, seriously it's great, but looks like it's not made for beginners on the langauge/framework so everything that I've done I had to takes notes of it.

[![Image from Gyazo](https://media.giphy.com/media/g4jDE1JnpUNaw/giphy.gif)](https://media.giphy.com/media/g4jDE1JnpUNaw/giphy.gif)

After learning that you can extend Pimcore by using bundles, I've spent hours and hours trying to override controllers, javascript methods and more stuff. 

So in this post I'll explain how I've managed to override a controller. How to override javascript files will come later 😎.

# Creating the bundle

First of all we need to create a bundle for it, it's pretty simple as its documented on the [Pimcore documentation](https://pimcore.com/docs/pimcore/current/Development_Documentation/Extending_Pimcore/Bundle_Developers_Guide/index.html), we just need to run `bin/console pimcore:generate:bundle --namespace=EmiDemo/EmiDemoBundle` on the project folder.

It will prompt some questions but nothing to worry about.

<div style="text-align:center"><img src="https://i.gyazo.com/9aa04169e668506d18638388d0061910.png" /></div>

It will create a new folder under the `src` folder with the namespace that we declared before, and inside we will have all the necessary files for the bundle.

<div style="text-align:center"><img src="https://i.gyazo.com/4d3329c303bb43012faaae43290c613b.png" /></div>

Also, the plugin will be detected by the Pimcore admin site, but it will be disabled, you have to enable it to start using it.

<div style="text-align:center"><img src="https://i.gyazo.com/e0170db9111df69a00bdb3f10473c9a7.png" /></div>

# Overriding a method from a controller

For overriding a method in a controller, first obviously you need to find the action that you want to update, this seems easy but I'll give you the way I usually do (learnt from my teammate [Cesar](https://twitter.com/cesabreu)).

First go into the page that you think the controller takes action, in my case I want to check the one that loads when we open an asset

[![Image from Gyazo](https://i.gyazo.com/df3833858806b14a39f50a0707a19dcd.png)](https://gyazo.com/df3833858806b14a39f50a0707a19dcd)

Then just open the console and go in the network tab, make the same steps again and try to find the action

[![Image from Gyazo](https://i.gyazo.com/5fbd9496d585d145bea3f9a3b950de73.png)](https://gyazo.com/5fbd9496d585d145bea3f9a3b950de73)

With this information you can see that the action that loads the asset is `http://localhost/admin/asset/get-data-by-id?_dc=1607601778450&id=2&type=image`. From that we can see that the controller action is `get-data-by-id`.

## Find the action in the core controller

For me, the simplest way to do it is just to find in all files in the Visual Studio Code

[![Image from Gyazo](https://i.gyazo.com/96cbeea01a1174d45e5a263a997c882a.png)](https://gyazo.com/96cbeea01a1174d45e5a263a997c882a)

Probably it will find more than one so you must take a look and decide which one is the one, in our case we are working with assets so it's clear that we want to use the `AssetController.php`.

## Modify the core controller

It's depends on the developer, I don't usually do it because it's faster to just override the controller and develop from there. But I'd recommend first to update the controller in the core bundle to see if your changes work.

In our case I will just return a message at the beginning of the method to check that it works.

```php
return $this->adminJson(['success' => false, 'message' => "Overriding the getDataByIdAction in the core!!"]);
```

[![Image from Gyazo](https://i.gyazo.com/32139d67b9e5369d5ab38daed1b229ea.png)](https://gyazo.com/32139d67b9e5369d5ab38daed1b229ea)

Then let's reload our asset and check the network tab

[![Image from Gyazo](https://i.gyazo.com/d1653f36e7bece9da6232ede0a431c05.png)](https://gyazo.com/d1653f36e7bece9da6232ede0a431c05)

Let's keep what we have in there, so later we know that we are using the bundle message instead of the core one.

Now let's just copy that to a temporary file to have track what we did.

## Move those changes to the bundle

Now in order to move those changes to our bundle, but first we need some stuff from our core controller:

* Namespace
* Imports
* Controller name

[![Image from Gyazo](https://i.gyazo.com/416b3a527be397aaf6d9f89073c02428.png)](https://gyazo.com/416b3a527be397aaf6d9f89073c02428)

And obviously the method `getDataByIdAction` which is the one that we want to override.

## Expose the controller

We need to expose the controller, in order to do this you need to go into the file `/src/EmiDemo/EmiDemoBundle/Resources/config/pimcore/routing.yml` and add 

```yml
options:
        expose: true
```

Also we need to change the `prefix` to the one we are going to override, in our case the `admin` controller.

So in the end ti will look like this

```yml
emi_demo_emi_demo:
    resource: "@EmiDemoEmiDemoBundle/Controller/"
    type:     annotation
    prefix:   /admin
    options:
        expose: true
```

## DefaultController.php

In our file, we will add the imports and extend the controller with the one we will overide

[![Image from Gyazo](https://i.gyazo.com/b8d798436ed111d4676312f8aeba443d.png)](https://gyazo.com/b8d798436ed111d4676312f8aeba443d)

Then we will just copy the method from the core controller, and, in our case, update the message that we return.

[![Image from Gyazo](https://i.gyazo.com/dcdd9f8166e4ba8820cb8e285c43dec8.png)](https://gyazo.com/dcdd9f8166e4ba8820cb8e285c43dec8)

If you remember, we have a message in the core controller already: `Overriding the getDataByIdAction in the core!!` and now we should be seeing `Overriding the getDataByIdAction in the bundle!!`

[![Image from Gyazo](https://i.gyazo.com/8e16b1de2a40292c843c51576acf43c4.png)](https://gyazo.com/8e16b1de2a40292c843c51576acf43c4)

Let's just not return anything and see that we can now see the page like it was before

[![Image from Gyazo](https://i.gyazo.com/dda8df5f6d721f5ba25d6a056ac7f9bf.png)](https://gyazo.com/dda8df5f6d721f5ba25d6a056ac7f9bf)

Comment out the return statement and reload

[![Image from Gyazo](https://i.gyazo.com/2fdbff7f45c38fb2bc56a3fc73661077.png)](https://gyazo.com/2fdbff7f45c38fb2bc56a3fc73661077)

## That's it

And with this little demo you can see how easy you can override an existing Pimcore core controller. There are some steps that you have to follow but it's nothing hard. Just make sure you expose the controller and clear the cache from time to time while you are making changes, sometimes it get cached and you are stuck thinking it's not working and it's just cached.

If you are wondering about how to override the javascript files, it will come in another tutorial 😁.