---
title: "jordgubbe"
layout: post
date: 2018-05-24 12:00
tag: projects
headerImage: true
projects: true
hidden: true # don't count this post in blog pagination
description: "🍓 A metro styled landing page for your links and apps."
category: project
author: emi
externalLink: false
---

<h1 align="center">jordgubbe :strawberry:</h1>

 <p align="center">A landing page where you can list all the apps and services stored in different places and access them from a single page.</p>
 
  <p align="center">:star: You can try an example <a href="https://emimontesdeoca.github.io/jordgubbe/">here</a> :star:</p>

[![https://gyazo.com/3ddcf8d28366bc7bab946731d7c665c5](https://i.gyazo.com/3ddcf8d28366bc7bab946731d7c665c5.png)](https://gyazo.com/3ddcf8d28366bc7bab946731d7c665c5)

## Introduction
So the main idea for this landing page was to have all my usual links plus the services stored on different servers/vms, like Deluge, CouchPotato, Sonarr and many more.

I came up with this when I was looking for a landing page, there were good ones but I wanted to make something fresh and either material design o metro design, ended up picking metro because, well, it looks cool.

Also I used [simpleWeather](http://simpleweatherjs.com/) and added some weather information on the header.

## Where are the links stored?
Everything regarding the links are stored in a .json file, there for now, you can add the links with its title and colour.

This is ithe scheme of the json file:

```json
{  
   "name": "Sonarr",
   "link": "https://sonarr.tv/",
   "colour": "cyan"
}
```
`name`: is the title that is shown in the page.<br>
`link`: URL to the link/service/app.<br>
`colour`: Background colour of service, to see the available colours go to [css/colors.css](https://github.com/emimontesdeoca/jordgubbe/blob/master/css/colors.css).

## What about the weather?

Everything is pretty easy to modify, in the [js/app.js](https://github.com/emimontesdeoca/jordgubbe/blob/master/js/app.js) file you can see how the simplyWeather is, if you just want to change the location, change the loadWeather function call:

```javascript
{  
   $(document).ready(function() {
        loadWeather('your_location', '');
    });
}
```

## Questions and issues

You can use the github issue tracker for bug reports, feature requests, questions.

## Thanks

Thanks to simplyWeather for its API, r/homelab for being a cool subreddit, and myself for finishing the project(for now!).

## License

MIT License

